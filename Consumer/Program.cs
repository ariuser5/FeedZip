using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using XmlConfigureApp;

namespace Consumer
{
	class Program
	{

		private static XmlConfig config;


		[STAThread]
		static void Main(string[] args) {

			config = XmlConfig.Read("xmlconfig.xml");
			MainScreen();
		}

		static void MainScreen() {

			void render() {
				Console.Clear();
				Console.WriteLine("Press ENTER to open a .zip file.");
				Console.WriteLine("Press ESC quit application.");
			}

			ConsoleKeyInfo keyInfo;
			do {
				render();
				keyInfo = Console.ReadKey();

				if(keyInfo.Key == ConsoleKey.Enter)
					OpenZip();

			} while(keyInfo.Key != ConsoleKey.Escape);

		}

		static void InspectZipScreen(string zipFile, IEnumerable<string> imgFiles) {

			void render() {
				Console.Clear();
				Console.WriteLine("Inspecting file \"" + zipFile + "\':");
				Console.WriteLine();

				for(var i = 0; i < imgFiles.Count(); i++)
					Console.WriteLine(string.Format("{0}. {1}", i + 1, imgFiles.ElementAt(i)));

				Console.WriteLine();
				Console.WriteLine("Write the number of each file to open it.");
				Console.WriteLine("Write ESC to return to main screen.");
				Console.Write(">");
			}


			string cmd;
			do {
				render();
				cmd = Console.ReadLine();

				if(int.TryParse(cmd, out int i) &&
				   (i > 0 && i < imgFiles.Count())) {

					Process.Start(imgFiles.ElementAt(i));
				}

			} while(cmd.ToLower() != "esc");

			Console.Clear();
		}

		static void OpenZip() {

			using(var fileDialog = new OpenFileDialog
			{
				Multiselect = false,
				Filter = "Zip Files (*.zip) | *.zip"
			}) {
				var dialogResult = fileDialog.ShowDialog();

				if(dialogResult == DialogResult.OK) {
					var discoveredImgFiles = InspectZip(fileDialog.FileName);

					InspectZipScreen(fileDialog.FileName, discoveredImgFiles);
				}
			}

		}

		static string[] InspectZip(string zipFile) {
			var location = Path.GetDirectoryName(zipFile);
			var zipName = Path.GetFileNameWithoutExtension(zipFile);
			var unzippedDir = Directory.CreateDirectory(Path.Combine(location, zipName));
			var imgFiles = new List<string>();

			ZipFile.ExtractToDirectory(zipFile, unzippedDir.FullName);
			File.Delete(zipFile);

			foreach(var fi in unzippedDir.GetFiles("*.*", SearchOption.AllDirectories)) {
				var pattern = "(?!\\.meta)\\.[^\\d\\W]{3,4}$";

				if(fi.Name == ".meta") {
					RecordToDB(fi);
				} else if(Regex.IsMatch(fi.Name, pattern)) {
					imgFiles.Add(fi.FullName);
				}
			}

			return imgFiles.ToArray();
		}

		static void RecordToDB(FileInfo metaFile) {
			var connectionString = ConfigurationManager.ConnectionStrings["DBKey"].ConnectionString;

			using(var con = new SqlConnection(connectionString)) {
				var sqlInsertRecord = "EXEC InsertRecord @p0, @p1";
				var sqlInsertFile = "EXEC InsertFile @p0, @p1, @p2, @p3, @p4";

				con.Open();

				using(var sqlCmd = new SqlCommand(sqlInsertRecord, con)
				{
					CommandTimeout = 5,
					CommandType = System.Data.CommandType.Text
				}) {
					sqlCmd.Parameters.AddWithValue("@p0", metaFile.Directory.Name);
					sqlCmd.Parameters.AddWithValue("@p1", metaFile.Directory.FullName);

					sqlCmd.ExecuteNonQuery();
				}

				var rows = File.ReadAllLines(metaFile.FullName);

				foreach(var row in rows) {

					using(var sqlCmd = new SqlCommand(sqlInsertFile, con)
					{
						CommandTimeout = 5,
						CommandType = System.Data.CommandType.Text
					}) {
						var columns = row.Split(new string[] { config.delimiter },
												StringSplitOptions.None);
						var dateTimeStr = columns[1].Split(new[] { " " }, StringSplitOptions.None);

						sqlCmd.Parameters.AddWithValue("@p0", columns[0]);
						sqlCmd.Parameters.AddWithValue("@p1", columns[2]);
						sqlCmd.Parameters.AddWithValue("@p2", dateTimeStr[0]);
						sqlCmd.Parameters.AddWithValue("@p3", dateTimeStr[1]);
						sqlCmd.Parameters.AddWithValue("@p4", metaFile.Directory.Name);

						sqlCmd.ExecuteNonQuery();
					}

				}

			}

		}

	}
}
