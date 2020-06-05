using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlConfigureApp;

namespace Feeder
{
	class Program
	{

		private static DateTime myDate;
		private static XmlConfig config;
		private static string dayOfWeek;
		private static string tempDir;

		private static List<string> storedFiles;


		static void Main(string[] args) {

			myDate = DateTime.Now;
			config = XmlConfig.Read("xmlconfig.xml");
			dayOfWeek = myDate.DayOfWeek.ToString().ToLower();
			storedFiles = new List<string>();

			tempDir = Directory.CreateDirectory("temp").FullName;

			CopySrcFiles();
			WriteMeta();
			PackToZip();

			Directory.Delete(tempDir, true);
		}

		static void CopySrcFiles() {
			var destDir = Directory.CreateDirectory(Path.Combine(tempDir, dayOfWeek));
			var srcFiles = Directory.
				GetFiles(config.sourceDir).
				Where(f => config.AllowedExtensions.
					  Any(e => Path.GetExtension(f) == e));

			foreach(var file in srcFiles) {
				var fileName = Path.GetFileName(file);
				var targetFile = Path.Combine(destDir.FullName, fileName);

				File.Copy(file, targetFile);
				storedFiles.Add(targetFile);
			}
		}

		static void WriteMeta() {
			var records = new List<string>();
			var counter = new DailyCounter("count", myDate);

			foreach(var file in storedFiles) {
				var fileName = Path.GetFileName(file);
				var countIndex = (++counter.Count).ToString("D5");
				var id = Helpers.GetJulianDate(myDate) + countIndex;
				var columns = new[]
				{
					id,
					myDate.ToString(config.metaDateFormat + " " + config.metaTimeFormat),
					Path.Combine(dayOfWeek, fileName)
				};

				records.Add(string.Join(config.delimiter, columns));
			}

			File.WriteAllLines(Path.Combine(tempDir, ".meta"), records);
		}

		static void PackToZip() {
			var zipName = myDate.ToString(config.zipFileNameFormat) + ".zip";
			var targetDir = Directory.CreateDirectory(config.targetDir);
			var zipPath = Path.Combine(targetDir.FullName, zipName);

			ZipFile.CreateFromDirectory(tempDir, zipPath);
		}


	}
}
