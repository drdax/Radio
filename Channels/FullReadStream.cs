using System;
using System.IO;

namespace DrDax.RadioClient {
	/// <summary>
	/// Plūsma, kurai no datu avota jānolasa tik baitu, cik prasīts.
	/// </summary>
	public abstract class FullReadStream : Stream {
		/// <summary>Datu avots, piemēram, HTTP plūsma ar MP3.</summary>
		protected Stream sourceStream;
		/// <summary>Nolasīto baitu skaits kopš paša sākuma.</summary>
		protected long pos;
		/// <summary>No datu avota nolasītie pašlaik aktuālie baiti.</summary>
		/// <remarks>Var būt visi vai tikai ne-ICE baiti.</remarks>
		protected readonly byte[] readAheadBuffer=new byte[4096]; // 4KB, parasti datu apjoms ir manāmi mazāks.;
		/// <summary>
		/// Aktuālo baitu skaits masīvā <see cref="readAheadBuffer"/>.
		/// </summary>
		protected int readAheadLength;
		/// <summary>
		/// Baitu indeks masīvā <see cref="readAheadBuffer"/> sākot ar kuru pašlaik kopē datus.
		/// </summary>
		protected int readAheadOffset;

		public override long Position {
			get { return pos; }
			set { throw new InvalidOperationException(); }
		}
		public override long Length { get { return pos; } }
		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return false; } }
		public override bool CanWrite { get { return false; } }

		public override void Flush() {
			throw new InvalidOperationException();
		}
		public override long Seek(long offset, SeekOrigin origin) {
			throw new InvalidOperationException();
		}
		public override void SetLength(long value) {
			throw new InvalidOperationException();
		}
		public override void Write(byte[] buffer, int offset, int count) {
			throw new InvalidOperationException();
		}
	}
}
