using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Roadkill.Api.Common.Models
{
	public class PageModel
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		public string SeoFriendlyTitle { get; set; }

		[Required]
		public string CreatedBy { get; set; }

		public DateTime CreatedOn { get; set; }

		public string LastModifiedBy { get; set; }

		public DateTime LastModifiedOn { get; set; }

		[Required]
		public bool IsLocked { get; set; }

		[Required]
		public string TagsAsCsv { get; set; }

		public IEnumerable<string> TagList { get; set; }
	}
}
