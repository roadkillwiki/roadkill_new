using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Roadkill.Tests.Unit")]

namespace Roadkill.Text
{
	public class TextSettings
	{
		public string AttachmentsFolder { get; set; }
		public string AttachmentsUrlPath { get; set; }
		public string CustomTokensPath { get; set; }
		public string HtmlElementWhiteListPath { get; set; }
		public bool UseHtmlWhiteList { get; set; }

		public TextSettings()
		{
			AttachmentsFolder = Path.Combine(Directory.GetCurrentDirectory(), "attachments");
			AttachmentsUrlPath = "/attachments/";
		}
	}
}