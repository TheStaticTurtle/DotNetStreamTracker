using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StreamTracker {
	public class NotifiedTracker : Tracker {
		protected Dictionary<ReadOnlyTrackedStreamStream, Int64> streamRead;
		protected Dictionary<ReadOnlyTrackedStreamStream, Int64> streamTotal;

		public NotifiedTracker() : this(Array.Empty<Stream>()) { }
		public NotifiedTracker(Stream stream) : this(new[] { stream }) { }
		public NotifiedTracker(Stream[] streams) : base(streams) {
			streamRead = new Dictionary<ReadOnlyTrackedStreamStream, Int64>();
			streamTotal = new Dictionary<ReadOnlyTrackedStreamStream, Int64>();
		}

		public override void Add(ReadOnlyTrackedStreamStream item) {
			base.Add(item);
			streamRead.Add(item, item.Length);
			item.ProgressUpdate += this.Item_ProgressUpdate;
		}

		public override Boolean Remove(ReadOnlyTrackedStreamStream item) {
			item.ProgressUpdate -= this.Item_ProgressUpdate;
			streamRead.Remove(item);
			return base.Remove(item);
		}

		private void Item_ProgressUpdate(ReadOnlyTrackedStreamStream _sender, Int64 _read, Int64 _total) {
			if (UpdatesEnabled) {
				streamRead[_sender] = _read;
				streamTotal[_sender] = _total;

				Notify(
					streamRead.Values.Sum(),
					streamTotal.Values.Sum()
				);
			}
		}
	}
}
