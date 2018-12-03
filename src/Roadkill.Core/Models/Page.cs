using System;

namespace Roadkill.Core.Models
{
	public class Page
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public string CreatedBy { get; set; }

		public DateTime CreatedOn { get; set; }

		public string LastModifiedBy { get; set; }

		public DateTime LastModifiedOn { get; set; }

		public string Tags { get; set; }

		public bool IsLocked { get; set; }
	}
}