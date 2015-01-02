using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using NAudio.Wave;

namespace DrDax.RadioClient {
	/// <summary>Atskaņo MP3 raidstaciju caur HTTP protokolu.</summary>
	// Kods pārsvarā ņemts no Mark Heath piemēra "NAudio MP3 streaming".
	public abstract class StreamChannel<StreamType> : Channel where StreamType : FullReadStream {
		/// <summary>Skaņas datu sniedzējs.</summary>
		private BufferedWaveProvider bufferedWaveProvider;
		/// <summary>Atskaņotājs.</summary>
		private IWavePlayer waveOut;
		/// <summary>Signāla skaļuma regulators.</summary>
		private VolumeWaveProvider16 volumeProvider;
		/// <summary>Uzdevums, kurš uztver radio signālu.</summary>
		private Task streamTask;
		/// <summary>Taimers, pēc kura regulāri tiek atskaņots saņemtais signāls.</summary>
		private readonly Timer playbackTimer;
		/// <summary>Vai ieslēgts klusums.</summary>
		private bool isMuted=false;
		/// <summary>Radio signāla plūsma.</summary>
		protected readonly StreamType stream;
		/// <summary>Skaļums pirms tiek izveidots stacijas uztvērējs vai saglabāts gadījumā, ja ieslēgts klusums.</summary>
		private float volume=(float)Settings.Default.Volume;
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

		protected StreamChannel(StreamType stream, BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu)
			: base(logo, timezone, hasGuide, brand, menu) {
			playbackTimer=new Timer(250); // Ceturtdaļsekundes.
			playbackTimer.Elapsed+=playbackTimer_Elapsed;
			unexpectedStop=UnexpectedStop;
			this.stream=stream;
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
				streamTask=Task.Run((Func<Task>)StreamMp3);
			}
		}
		public override void Stop() {
			if (PlaybackState != PlaybackState.Stopped) {
				PlaybackState=PlaybackState.Stopped;
				playbackTimer.Stop();
				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut=null;
				}
				try { streamTask.Wait(1000); } catch {}
				stream.Close();
			}
		}

		/// <summary>Liek MP3 plūsmu buferī.</summary>
		private async Task StreamMp3() {
			try { await stream.Open(); } catch { unexpectedStop.BeginInvoke(null, null); return; }
			IMp3FrameDecompressor decompressor=null;
			byte[] buffer=new byte[65536]; // Atspiests skaņas fragments.
			bool hasException=false;
			PlaybackState=PlaybackState.Buffering;
			playbackTimer.Start();
			int sampleRate=0; // Iepriekšēja skaņas fragmenta frekvence, piemēram, 44100.
			while (PlaybackState == PlaybackState.Buffering || PlaybackState == PlaybackState.Playing) {
				if (bufferedWaveProvider != null
					&& bufferedWaveProvider.BufferLength-bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond/4) {
					await Task.Delay(500); // Kamēr buferī pietiekami datu, lejuplāde var pagaidīt.
					continue;
				}
				Mp3Frame frame=null;
				try { frame=Mp3Frame.LoadFromStream(stream); } catch (Exception) {
					hasException=PlaybackState != PlaybackState.Stopped;
					break;
				}
				if (frame.SampleRate != sampleRate) {
					if (decompressor == null) waveOut=new WaveOut();
					else decompressor.Dispose();
					Debug.WriteLine("{0} Hz, {1} bps, {2} bytes", frame.SampleRate, frame.BitRate, frame.FrameLength);
					// Sagatavo dekoderi saskaņā ar pirmā skaņas fragmenta īpatnībām.
					decompressor=new AcmMp3FrameDecompressor(
						new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
					bufferedWaveProvider=new BufferedWaveProvider(decompressor.OutputFormat);
					bufferedWaveProvider.BufferDuration=TimeSpan.FromSeconds(3);
					volumeProvider=new VolumeWaveProvider16(bufferedWaveProvider);
					volumeProvider.Volume=isMuted?0:volume;
					if (waveOut == null) break; // Kad apstādina atskaņošanu, tad novērš kļūdu nākamā rindā.
					waveOut.Init(volumeProvider);
					// Frekvencei jābūt nemainīgai visas plūsmas garumā, bet var tikt nepareizi noteikta pēc pirmā fragmenta.
					sampleRate=frame.SampleRate;
				}
				try {
					bufferedWaveProvider.AddSamples(buffer, 0, decompressor.DecompressFrame(frame, buffer, 0));
				} catch { hasException=true; break; }
			};
			if (PlaybackState != PlaybackState.Stopped) stream.Close();
			if (decompressor != null) decompressor.Dispose();
			if (hasException) unexpectedStop.BeginInvoke(null, null);
		}
		/// <summary>Regulāri atskaņot buferoto ierakstu.</summary>
		private void playbackTimer_Elapsed(object sender, ElapsedEventArgs e) {
			if (PlaybackState != PlaybackState.Stopped && bufferedWaveProvider != null) {
				var bufferedSeconds=bufferedWaveProvider.BufferedDuration.TotalSeconds;
				// Lielāks buferis nodrošina mazāku raustīšanos.
				if (bufferedSeconds < 0.5 && PlaybackState == PlaybackState.Playing) {
					PlaybackState=PlaybackState.Buffering;
					waveOut.Pause(); // Apstājas, kamēr aizpildās buferis.
				} else if (bufferedSeconds > 2 && PlaybackState == PlaybackState.Buffering) {
					waveOut.Play(); // Sāk atskaņot.
					PlaybackState=PlaybackState.Playing;
				}
			}
		}
		public override void Dispose() {
			base.Dispose();
			unexpectedStop=null;
			playbackTimer.Elapsed-=playbackTimer_Elapsed; // Noņemam cirkulāro atsauci.
		}
	}
	public class SegmentedChannel : StreamChannel<SegmentedStream> {
		/// <param name="baseUrl">Atskaņojamās plūsmas adreses sākumdaļa (ar slīpsvītru galā).</param>
		public SegmentedChannel(string url, BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu=null)
			: base(new SegmentedStream(url), logo, timezone, hasGuide, brand, menu) {}
	}
}