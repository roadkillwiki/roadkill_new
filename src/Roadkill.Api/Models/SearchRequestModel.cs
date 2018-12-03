using System;
using System.Collections.Generic;

namespace Roadkill.Api.Models
{
	public class SearchRequestModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string SeoFriendlyTitle { get; set; }

		public string CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public string LastModifiedBy { get; set; }
		public DateTime LastModifiedOn { get; set; }
		public bool IsLocked { get; set; }

		public string TagsAsCsv { get; set; }
		public IEnumerable<string> TagList { get; set; }
	}
}