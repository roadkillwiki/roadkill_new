namespace Roadkill.Text.Parsers.Images
{
	/// <summary>
	/// Holds information when an image is processed, giving the caller the ability to translate the outputted HTML.
	/// </summary>
	public class HtmlImageTag
	{
		public enum HorizontalAlignment
		{
			Left,
			Center,
			Right,
			None
		}

		/// <summary>
		/// The original image source url.
		/// </summary>
		public string OriginalSrc { get; set; }

		/// <summary>
		/// The source url used inside the HTML.
		/// </summary>
		public string Src { get; set; }

		/// <summary>
		/// The alt tag for the image.
		/// </summary>
		public string Alt { get; set; }

		/// <summary>
		/// The title tag for the image.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The type of horizontal align. Defaults to "Inline".
		/// </summary>
		public HorizontalAlignment HorizontalAlign { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="HtmlImageTag"/> class.
		/// </summary>
		public HtmlImageTag(string originalSrc, string src, string alt, string title, HorizontalAlignment horizontalAlign = HorizontalAlignment.None)
		{
			OriginalSrc = originalSrc;
			Src = src;
			Alt = alt;
			Title = title;
			HorizontalAlign = horizontalAlign;
		}
	}
}