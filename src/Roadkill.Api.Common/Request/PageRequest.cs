using System;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Api.Common.Request
{
	/// <summary>
	/// Represents page information in Roadkill.
	/// </summary>
	public class PageRequest
	{
		/// <summary>
		/// The unique Id of the page. This is generated on the server.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The title of the page.
		/// </summary>
		[Required]
		public string Title { get; set; }

		/// <summary>
		/// The url-friendly slug for the page title.
		/// </summary>
		public string SeoFriendlyTitle { get; set; }

		/// <summary>
		/// The user who created the page.
		/// </summary>
		[Required]
		public string CreatedBy { get; set; }

		/// <summary>
		/// The UTC-based date the page was created.
		/// </summary>
		public DateTime CreatedOn { get; set; }

		/// <summary>
		/// The user who last modified the page. If the page has
		/// note been modified yet, this will match the CreatedBy property.
		/// </summary>
		public string LastModifiedBy { get; set; }

		/// <summary>
		/// The date the page was last modified on. If the page has
		/// note been modified yet, this will match the creation date.
		/// </summary>
		public DateTime LastModifiedOn { get; set; }

		/// <summary>
		/// Whether the page is locked so no edits can be made (except by admins).
		/// </summary>
		[Required]
		public bool IsLocked { get; set; }

		/// <summary>
		/// The list of tags, comma separated, for the page.
		/// </summary>
		[Required]
		public string TagsAsCsv { get; set; }
	}
}
