using System;

namespace Roadkill.Core.Entities
{
	public class SearchablePage
	{
		public Guid Id { get; set; }

		public int PageId { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public string Author { get; set; }

		public DateTime DateTime { get; set; }

		public string Tags { get; set; }
	}
}
