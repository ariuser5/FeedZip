using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XmlConfigureApp
{

	[XmlRoot("config")]
	public class XmlConfig
	{

		public static XmlConfig Read(string path) {
			var serializer = new XmlSerializer(typeof(XmlConfig));
			var retObj = (XmlConfig)null;

			using(var reader = new FileStream(path, FileMode.Open)) {
				retObj = (XmlConfig)serializer.Deserialize(reader);
			}

			return retObj;
		}



		[XmlElement("delimiter")]
		public string delimiter;

		[XmlElement("sourceDir")]
		public string sourceDir;

		[XmlElement("targetDir")]
		public string targetDir;

		[XmlElement("allowedExtensions")]
		public string fileExtensions;

		[XmlElement("metaDateFormat")]
		public string metaDateFormat;

		[XmlElement("metaTimeFormat")]
		public string metaTimeFormat;

		[XmlElement("zipFileNameFormat")]
		public string zipFileNameFormat;


		public XmlConfig() { }


		public string[] AllowedExtensions {
			get => fileExtensions.
				Split(new[] { ',' },
					  StringSplitOptions.RemoveEmptyEntries).
				Select(w => w.Trim()).
				ToArray();
		}

	}
}
