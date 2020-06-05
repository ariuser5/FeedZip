using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feeder
{
	class Helpers
	{

		public static string GetJulianDate(DateTime dateTime) {
			return string.Format("{0:yy}{1:D3}", dateTime, dateTime.DayOfYear);
		}

	}
}
