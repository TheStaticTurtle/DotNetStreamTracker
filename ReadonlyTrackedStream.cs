using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTracker {
	public delegate void TrackedStreamUpdate(ReadOnlyTrackedStreamStream sender, Int64 read, Int64 total);

	public class ReadOnlyTrackedStreamStream : Stream, IDisposable {
		private Int64 _originalLength;
		private Stream _parent;

		public event TrackedStreamUpdate ProgressUpdate;

		public ReadOnlyTrackedStreamStream(Stream stream) {
			_parent = stream;
			_originalLength = stream.Length;
		}

		public Stream Parent => this._parent;

		public override Boolean CanRead => this._parent != null && _parent.CanRead;
		public override Boolean CanSeek => this._parent != null && _parent.CanSeek;
		public override Boolean CanWrite => this._parent != null && _parent.CanWrite;
		public override Int64 Length => this._parent != null ? _parent.Length : _originalLength;
		public override Int64 Position {
			get => this._parent != null ? _parent.Position : _originalLength;
			set { if (this._parent != null) { this._parent.Position = value; } }
		}

		public override void SetLength(Int64 value) {
			if (_parent != null) {
				_parent.SetLength(value);
				_originalLength = value;
			}
			Notify();
		}
		public override void Flush() {
			_parent?.Flush();
			Notify();
		}
		public override void Write(Byte[] buffer, Int32 offset, Int32 count) {
			Notify();
			throw new NotSupportedException();
		}

		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count) {
			if (this._parent == null) return 0;

			Int32 len = _parent.Read(buffer, offset, count);
			if (ReadCompleted && this._parent != null) {
				this._parent = null;
				Notify(true);
			}
			Notify();
			return len;
		}
		public override Int64 Seek(Int64 offset, SeekOrigin origin) {
			if (this._parent == null) return 0;

			Int64 len = _parent.Seek(offset, origin);
			if (ReadCompleted && this._parent != null) {
				this._parent = null;
				Notify(true);
			}
			Notify();
			return len;
		}

		public bool ReadCompleted {
			get {
				try {
					return this._parent == null || (this.Position >= (this.Length - 1));
				} catch (ObjectDisposedException) {
					return true;
				}
			}
		}

		// Very simple rate-limit to make sure we don't overload the listener if the stream is read byte by byte
		private DateTime nextNotifyDate = DateTime.MinValue;
		protected void Notify(bool force = false) {
			if (DateTime.Now > nextNotifyDate || force) {
				nextNotifyDate = DateTime.Now + TimeSpan.FromMilliseconds(50);

				ProgressUpdate?.Invoke(this, Position, Length);
			}
		}

		public new void Dispose() {
			ProgressUpdate?.Invoke(this, _originalLength, _originalLength);
			base.Dispose();
		}
	}
}
