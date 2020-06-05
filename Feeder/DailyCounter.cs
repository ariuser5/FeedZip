using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feeder
{
	class DailyCounter
	{

		private readonly string path;
		private readonly DateTime targetDateTime;

		public DailyCounter(string path, DateTime targetDateTime) {
			this.path = path;
			this.targetDateTime = targetDateTime;

			if(!File.Exists(path)) {
				Count = 0;
			} else {
				var text = File.ReadAllText(path);
				var tokens = text.Split(':');
				var currentJulianDate = Helpers.GetJulianDate(targetDateTime);

				if(currentJulianDate != tokens[0])
					Count = 0;
			}

		}


		public int Count {
			get {
				var text = File.ReadAllText(path);
				var tokens = text.Split(':');

				return int.Parse(tokens[1]);
			}
			set {
				File.WriteAllText(path, Helpers.GetJulianDate(targetDateTime) + ":" + value);
			}
		}

	}

}
