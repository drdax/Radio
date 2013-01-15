using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using NAudio.Wave;

namespace DrDax.RadioClient {
	/// <summary>Atskaņo MP3 raidstaciju caur HTTP protokolu.</summary>
	// Kods pārsvarā ņemts no Mark Heath piemēra "NAudio MP3 streaming".
	public class HttpChannel : Channel {
		/// <summary>Programmas identifikators, griežoties pie servera.</summary>
		public string UserAgent=null;
		/// <summary>
		/// Savienošanās ar serveri ilgums milisekundēs pēc kura savienojums uzskatāms par neveiksmīgu.
		/// Tiek palielināts ar katru neveiksmīgu pieslēgumu.
		/// </summary>
		private int connectionTimeout=5000;
		/// <summary>Skaņas datu sniedzējs.</summary>
		private BufferedWaveProvider bufferedWaveProvider;
		/// <summary>Atskaņotājs.</summary>
		private IWavePlayer waveOut;
		/// <summary>
		/// Vai signāla plūsma ir beigusies. Praksē vienmēr vajadzētu būt <c>false</c>.
		/// </summary>
		private volatile bool fullyDownloaded;
		/// <summary>Signāla skaļuma regulators.</summary>
		private VolumeWaveProvider16 volumeProvider;
		/// <summary>Uzdevums, kurš uztver radio signālu.</summary>
		private Task streamTask;
		/// <summary>Taimers, pēc kura regulāri tiek atskaņots saņemtais signāls.</summary>
		private readonly Timer playbackTimer;
		private HttpWebRequest request;
		/// <summary>Vai ieslēgts klusums.</summary>
		private bool isMuted=false;
		/// <summary>
		/// Vai radio kanālam nepieciešami un plūsmā faktiski ir ICE meta dati.
		/// </summary>
		private bool hasMetaTitles;
		/// <summary>
		/// Vai radio signāls nāk kā M3U dinamiskais saraksts, kurā katrs atskaņojamais gabaliņš numurēts.
		/// </summary>
		private readonly bool useM3uStreaming;
		/// <summary>Skaļums pirms tiek izveidots stacijas uztvērējs vai saglabāts gadījumā, ja ieslēgts klusums.</summary>
		private float volume=(float)Properties.Settings.Default.Volume;
		/// <summary>Atsauce uz <see cref="UnexpectedStop"/> asinhronam izsaukumam.</summary>
		/// <remarks>Ja sauktu pa taisno, izveidotos mūžīgais cikls.</remarks>
		private Action unexpectedStop;

		public override double Volume {
			get { return volume; }
			set {
				if (!isMuted && volumeProvider != null)
					volumeProvider.Volume=(float)value;
				volume=(float)value;
				NotifiyPropertyChanged("Volume");
			}
		}

		public HttpChannel(string url, BitmapImage logo, TimeZoneInfo timezone, Guide guide, Brand brand)
			: this(false, url, logo, timezone, guide, brand) {}
		public HttpChannel(bool useM3uStreaming, string url, BitmapImage logo, TimeZoneInfo timezone, Guide guide, Brand brand)
			: base(url, logo, timezone, guide, brand) {
			playbackTimer=new Timer(250); // Ceturtdaļsekundes.
			playbackTimer.Elapsed+=playbackTimer_Elapsed;
			hasMetaTitles=guide is IcyGuide;
			unexpectedStop=UnexpectedStop;
			this.useM3uStreaming=useM3uStreaming;
		}

		protected override bool GetIsMuted() {
			return isMuted;
		}
		protected override void SetIsMuted(bool value) {
			if (volumeProvider != null)
				volumeProvider.Volume=value ? 0:volume;
			isMuted=value;
		}
		public override void Play() {
			if (PlaybackState == PlaybackState.Stopped) {
				bufferedWaveProvider=null;
				PlaybackState=PlaybackState.Connecting;
				streamTask=Task.Factory.StartNew(StreamMP3, url);
				playbackTimer.Start();
			}
		}
		public override void Stop() {
			if (PlaybackState != PlaybackState.Stopped) {
				PlaybackState=PlaybackState.Stopped;
				playbackTimer.Stop();
				if (!fullyDownloaded && request != null) request.Abort();
				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut=null;
				}
				try { streamTask.Wait(); } catch {}
			}
		}

		/// <summary>Buffers MP3 stream from HTTP.</summary>
		/// <param name="state">(<c>string</c>) Full stream URL.</param>
		private void StreamMP3(object state) {
			FullReadStream mp3Stream;
			if (useM3uStreaming) {
				try {
					mp3Stream=new M3uFullReadStream((string)state);
				} catch { unexpectedStop.BeginInvoke(null, null); return; }
			} else {
				fullyDownloaded=false;
				request=(HttpWebRequest)WebRequest.Create((string)state);
				request.ApplyProxy();
				request.UserAgent=UserAgent;
				request.Timeout=connectionTimeout;
				// Pieprasa ICE meta datus.
				if (hasMetaTitles) request.Headers.Add("Icy-MetaData", "1");
				HttpWebResponse response=null;
				try {
					response=(HttpWebResponse)request.GetResponse();
				} catch (WebException) {
					if (connectionTimeout == 30000) connectionTimeout=5000;
					else connectionTimeout+=5000;
					unexpectedStop.BeginInvoke(null, null); return;
				}

				Stream responseStream=null;
				ushort metaInt=0; // ICE meta datu izmēra baita numurs plūsmā.
				hasMetaTitles=hasMetaTitles && ushort.TryParse(response.GetResponseHeader("icy-metaint"), out metaInt);
				Debug.WriteLine("metaint="+metaInt);
				try {
					responseStream=response.GetResponseStream();
				} catch { unexpectedStop.BeginInvoke(null, null); return; }
				mp3Stream=new IcyFullReadStream(responseStream, metaInt, hasMetaTitles ? ((IcyGuide)Guide).ProcessMetaHeader:(Action<byte[], int>)null);
			}
			IMp3FrameDecompressor decompressor=null;
			byte[] buffer=new byte[65536]; // Atspiests skaņas fragments.
			bool hasException=false;
			PlaybackState=PlaybackState.Buffering;
			do {
				if (bufferedWaveProvider != null
					&& bufferedWaveProvider.BufferLength-bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond/4) {
					System.Threading.Thread.Sleep(500); // Kamēr buferī pietiekami datu, lejuplāde var pagaidīt.
					continue;
				}
				Mp3Frame frame=null;
				try {
					frame=Mp3Frame.LoadFromStream(mp3Stream);
				} catch (EndOfStreamException) { fullyDownloaded=true; break; }
				catch (Exception) {
					hasException=PlaybackState != PlaybackState.Stopped;  // Stopped jābūt, ja izsauca request.Abort().
					break;
				}
				if (decompressor == null) {
					Debug.WriteLine("{0} Hz, {1} bps, {2} bytes", frame.SampleRate, frame.BitRate, frame.FrameLength);
					// Sagatavo dekoderi saskaņā ar pirmā skaņas fragmenta īpatnībām.
					decompressor=new AcmMp3FrameDecompressor(
						new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
					bufferedWaveProvider=new BufferedWaveProvider(decompressor.OutputFormat);
					bufferedWaveProvider.BufferDuration=TimeSpan.FromSeconds(10);
					volumeProvider=new VolumeWaveProvider16(bufferedWaveProvider);
					volumeProvider.Volume=isMuted?0:volume;
					waveOut=new WaveOut();
					waveOut.Init(volumeProvider);
				}
				try {
					bufferedWaveProvider.AddSamples(buffer, 0, decompressor.DecompressFrame(frame, buffer, 0));
				} catch { hasException=true; break; }
			} while (PlaybackState != PlaybackState.Stopped);
			mp3Stream.Dispose();
			if (decompressor != null) decompressor.Dispose();
			if (hasException) unexpectedStop.BeginInvoke(null, null);
		}
		/// <summary>Regulāri atskaņot buferoto ierakstu.</summary>
		private void playbackTimer_Elapsed(object sender, ElapsedEventArgs e) {
			if (PlaybackState != PlaybackState.Stopped && waveOut != null) {
				var bufferedSeconds=bufferedWaveProvider.BufferedDuration.TotalSeconds;
				// Lielāks buferis nodrošina mazāku raustīšanos.
				if (bufferedSeconds < 0.5 && PlaybackState == PlaybackState.Playing && !fullyDownloaded) {
					PlaybackState=PlaybackState.Buffering;
					waveOut.Pause(); // Apstājas, kamēr aizpildās buferis.
				} else if (bufferedSeconds > 4 && PlaybackState == PlaybackState.Buffering) {
					waveOut.Play(); // Sāk atskaņot.
					PlaybackState=PlaybackState.Playing;
				} else if (fullyDownloaded && bufferedSeconds == 0)
					unexpectedStop.BeginInvoke(null, null); // Nav vairāk datu, ko atskaņot. Tā nevajadzētu notikt.
			}
		}
		public override void Dispose() {
			base.Dispose();
			unexpectedStop=null;
			playbackTimer.Elapsed-=playbackTimer_Elapsed; // Noņemam cirkulāro atsauci.
		}
	}
}