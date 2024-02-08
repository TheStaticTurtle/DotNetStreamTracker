using System;
using System.IO;
using System.Timers;
using Timer = System.Timers.Timer;

namespace StreamTracker {
	public class PollingTracker : Tracker {
		private readonly Timer updateTimer;

		public PollingTracker(int notifyInterval = 100) : this(Array.Empty<Stream>(), notifyInterval) { }
		public PollingTracker(Stream stream, int notifyInterval = 100) : this(new[] { stream }, notifyInterval) { }
		public PollingTracker(Stream[] streams, int notifyInterval = 100) : base(streams) {
			updateTimer = new Timer { AutoReset = true, Enabled = false, Interval = notifyInterval, };
			updateTimer.Elapsed += this.UpdateTimer_Elapsed; ;
		}

		public override Boolean UpdatesEnabled {
			get {
				return base.UpdatesEnabled;
			}
			set {
				base.UpdatesEnabled = value;
				if (value) {
					updateTimer.Start();
				} else {
					updateTimer.Stop();
				}
			}
		}

		private void UpdateTimer_Elapsed(Object sender, ElapsedEventArgs e) {
			Int64 total = 0;
			Int64 totalRead = 0;
			foreach (ReadOnlyTrackedStreamStream trackedStream in trackedStreams) {
				total += trackedStream.Length;
				totalRead += trackedStream.Position;
			}

			Notify(totalRead, total);
		}
	}
}
