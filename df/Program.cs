using System;
using System.Net;

namespace df {
	class Program {
		private static bool downloadComplete;
		private static long bytesReceived;
		private static long totalBytesToReceive;
		private static int progressPercentage;
		private static Exception exception;

		static void Main(string[] args) {
			Console.WriteLine("");
			Console.WriteLine("df v0.0.1");

			var url = string.Empty;
			var file = string.Empty;
			var isUrl = true;
			var displayHelp = false;

			foreach (var arg in args) {
				switch (arg) {
					case "-f":
						isUrl = false;
						break;

					case "-h":
					case "/?":
						displayHelp = true;
						break;

					default:
						if (isUrl)
							url += arg + " ";
						else
							file += arg + " ";

						break;
				}
			}

			url = url.Trim();
			file = file.Trim();

			if (string.IsNullOrWhiteSpace(url))
				displayHelp = true;

			if (displayHelp) {
				Console.WriteLine("Download a file from the great tubes...");
				Console.WriteLine("");
				Console.WriteLine("Usage: df url [-f file]");
				Console.WriteLine("");
				Console.WriteLine("Omitting -f, top men will try to guess the filename from URL.");
				Console.WriteLine("");

				return;
			}

			if (string.IsNullOrWhiteSpace(file)) {
				if (url.EndsWith("/"))
					file = url
						.Replace(":", "-")
						.Replace("/", "-");
				else
					file = url.Substring(url.LastIndexOf('/') + 1);
			}

			

			var webClient = new WebClient();
			var start = DateTime.Now;
			var update = DateTime.Now.AddSeconds(-2);

			Console.WriteLine("");
			Console.WriteLine("URL: " + url);
			Console.WriteLine("File: " + file);
			Console.WriteLine("");
			Console.WriteLine("Download Started:  " + start.ToString("yyyy-MM-dd hh:mm:ss"));

			webClient.DownloadProgressChanged += (o, e) => {
				bytesReceived = e.BytesReceived;
				totalBytesToReceive = e.TotalBytesToReceive;
				progressPercentage = e.ProgressPercentage;
			};

			webClient.DownloadFileCompleted += (o, e) => {
				downloadComplete = true;

				if (e.Error != null)
					exception = e.Error;
			};

			webClient.DownloadFileAsync(
				new Uri(url),
				file);

			while (downloadComplete == false) {
				if (DateTime.Now < update &&
					progressPercentage < 100)
					continue;

				update = DateTime.Now.AddMilliseconds(100);

				Console.Write(
					"Download Progress: {0}% - {1} of {2}         ",
					progressPercentage,
					formatBytes(bytesReceived),
					formatBytes(totalBytesToReceive));

				Console.CursorLeft = 0;
			}

			var end = DateTime.Now;
			var duration = end - start;

			Console.WriteLine("");

			if (exception != null)
				Console.WriteLine("Error: " + exception.Message);

			Console.WriteLine("Download Ended:    " + end.ToString("yyyy-MM-dd hh:mm:ss"));
			Console.WriteLine("Download Took:     " + duration);

			Console.WriteLine("");
		}

		private static string formatBytes(long bytes) {
			if (bytes > (1024 * 1024 * 1024))
				return ((int)bytes / (1024 * 1024 * 1024)) + " Mb";
			if (bytes > (1024*1024))
				return ((int) bytes/(1024*1024)) + " Mb";
			if (bytes > 1024)
				return ((int) bytes/1024) + " Kb";

			return bytes + " b";
		}
	}
}