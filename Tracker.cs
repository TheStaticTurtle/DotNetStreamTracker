using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace StreamTracker {
	public delegate void StreamTrackerUpdate(Tracker sender, Int64 read, Int64 total);
	public delegate void StreamTrackerUpdatePercentage(Tracker sender, double percentage);

	public abstract class Tracker : ICollection<ReadOnlyTrackedStreamStream> {
		protected List<ReadOnlyTrackedStreamStream> trackedStreams;

		public Int32 Count => trackedStreams.Count;

		public Boolean IsReadOnly => false;

		public event StreamTrackerUpdatePercentage ProgressPercentageUpdate;
		public event StreamTrackerUpdate ProgressUpdate;

		public Tracker() : this(Array.Empty<Stream>()) { }
		public Tracker(Stream stream) : this(new[] { stream }) { }
		public Tracker(Stream[] streams) {
			trackedStreams = new List<ReadOnlyTrackedStreamStream>();
			foreach (Stream stream in streams) Add(stream);
		}

		public virtual Boolean UpdatesEnabled { get; set; }
		protected void Notify(Int64 totalRead, Int64 total) {
			if (UpdatesEnabled) {
				ProgressUpdate?.Invoke(this, totalRead, total);
				ProgressPercentageUpdate?.Invoke(this, Math.Round((totalRead / (double)total) * 100, 2));
			}
		}

		public virtual ReadOnlyTrackedStreamStream Add(Stream item) {
			ReadOnlyTrackedStreamStream trackedStream = new ReadOnlyTrackedStreamStream(item);
			Add(trackedStream);
			return trackedStream;
		}
		public virtual void Add(ReadOnlyTrackedStreamStream item) {
			trackedStreams.Add(item);
		}

		public virtual Boolean Remove(Stream item) {
			ReadOnlyTrackedStreamStream trackedStream = trackedStreams.Find(x => x.Parent == item);
			if (trackedStream != null) {
				return Remove(trackedStream);
			}
			return false;
		}
		public virtual Boolean Remove(ReadOnlyTrackedStreamStream item) {
			return trackedStreams.Remove(item);
		}
		public void Clear() {
			trackedStreams.Clear();
		}

		public Boolean Contains(Stream item) {
			ReadOnlyTrackedStreamStream trackedStream = trackedStreams.Find(x => x.Parent == item);
			return trackedStream != null;
		}
		public Boolean Contains(ReadOnlyTrackedStreamStream item) {
			return trackedStreams.Contains(item);
		}

		public IEnumerator<ReadOnlyTrackedStreamStream> GetEnumerator() {
			return trackedStreams.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() {
			return trackedStreams.GetEnumerator();
		}

		public void CopyTo(ReadOnlyTrackedStreamStream[] array, Int32 arrayIndex) {
			trackedStreams.CopyTo(array, arrayIndex);
		}
	}
}