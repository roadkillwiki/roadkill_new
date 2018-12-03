using System;

namespace Roadkill.Core.Models
{
	public class SearchablePage
	{
		public int Id { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public string Author { get; set; }

		public DateTime DateTime { get; set; }

		public string Tags { get; set; }
	}
}