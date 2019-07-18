using System;
using AutoMapper;
using Roadkill.Core.Entities;

namespace Roadkill.Api.Common.Response
{
	[AutoMap(typeof(PageVersion))]
	public class PageVersionResponse
	{
		public Guid Id { get; set; }
		public int PageId { get; set; }
		public string Text { get; set; }
		public string Author { get; set; }
		public DateTime DateTime { get; set; }
	}
}
