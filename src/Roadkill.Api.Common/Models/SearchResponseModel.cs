using System;

namespace Roadkill.Api.Common.Models
{
	/// <summary>
	/// Contains data for a single search result from a search query.
	/// </summary>
	public class SearchResponseModel
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public string EncodedTitle { get; set; }

		/// <summary>
		/// The summary of the content (the first 150 characters of text with all HTML removed).
		/// </summary>
		public string ContentSummary { get; set; }

		/// <summary>
		/// The length of the content in bytes.
		/// </summary>
		public int ContentLength { get; set; }

		/// <summary>
		/// Formats the page length in bytes using KB or bytes if it is less than 1024 bytes.
		/// </summary>
		public string ContentLengthInKB
		{
			get
			{
				if (ContentLength > 1024)
					return ContentLength / 1024 + "KB";
				else
					return ContentLength + " bytes";
			}
		}

		public string CreatedBy { get; set; }

		public DateTime CreatedOn { get; set; }

		public string Tags { get; set; }

		/// <summary>
		/// The lucene.net score for the search result.
		/// </summary>
		public float Score { get; set; }
	}
}