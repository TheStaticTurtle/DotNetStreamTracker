# StreamTracker

Simple utility class to track the progress of a stream

Can be used to track the progess of a file copy or upload

## Example
### Event based notifications
This method will hook the `Read`, `Seek`, `Write`, ... functions of a stream and will send events (Interval capped at 50ms to avoid spaming the events).
```c#

static void Main(string[] args) {
	Tracker tracker = new PollingTracker();
	tracker.ProgressPercentageUpdate += Tracker_ProgressPercentageUpdate;

	byte[] hashedData;
	using (SHA512 sha256 = SHA512.Create()) {
		using(FileStream stream = File.OpenRead(args[0])) {
			var trackedStream = tracker.Add(stream);
			tracker.UpdatesEnabled = true;
			hashedData = sha256.ComputeHash(trackedStream);
			tracker.UpdatesEnabled = false;
		}
	}
}

private static void Tracker_ProgressPercentageUpdate(Object sender, Double percentage) {
	Console.WriteLine($"{percentage} %");
}
```

### Polling based notifications
This method will use a timer object to poll the stream status at regular, user-defined interval.
```c#

static void Main(string[] args) {
	Tracker tracker = new PollingTracker();
	tracker.ProgressPercentageUpdate += Tracker_ProgressPercentageUpdate;

	byte[] hashedData;
	using (SHA512 sha256 = SHA512.Create()) {
		using(FileStream stream = File.OpenRead(args[0])) {
			var trackedStream = tracker.Add(stream);
			tracker.UpdatesEnabled = true;
			hashedData = sha256.ComputeHash(trackedStream);
			tracker.UpdatesEnabled = false;
		}
	}
}

private static void Tracker_ProgressPercentageUpdate(Object sender, Double percentage) {
	Console.WriteLine($"{percentage} %");
}
```
