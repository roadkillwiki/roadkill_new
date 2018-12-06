using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Roadkill.Tests.Unit")]

namespace Roadkill.Text
{
	[SuppressMessage("ReSharper", "CA1056", Justification = "AttachmentsUrlPath is a path not a Uri")]
	public class TextSettings
	{
		public TextSettings()
		{
			AttachmentsFolder = Path.Combine(Directory.GetCurrentDirectory(), "attachments");
			AttachmentsUrlPath = "/attachments/";
		}

		public string AttachmentsFolder { get; set; }

		public string AttachmentsUrlPath { get; set; }

		public string CustomTokensPath { get; set; }

		public string HtmlElementWhiteListPath { get; set; }

		public bool UseHtmlWhiteList { get; set; }
	}
}
