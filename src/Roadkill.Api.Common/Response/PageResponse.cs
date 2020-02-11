using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Roadkill.Core.Entities;

namespace Roadkill.Api.Common.Response
{
	/// <summary>
	/// Represents page information in Roadkill.
	/// </summary>
	[AutoMap(typeof(Page))]
	public class PageResponse
	{
		/// <summary>
		/// The unique JwtToken of the page. This is generated on the server.
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
		[Ignore]
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
		/// The list of tags, comma seperated, for the page.
		/// </summary>
		[Required]
		[Ignore]
		public string TagsAsCsv { get; set; }

		/// <summary>
		/// The tags for the page as an array.
		/// </summary>
		[Ignore]
		public IEnumerable<string> TagList { get; set; }
	}
}
