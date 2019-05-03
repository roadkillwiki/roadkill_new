using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.JWT;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize(Policy = PolicyNames.Admin)]
	public class ExportController : ControllerBase
	{
		private readonly IPageRepository _pageRepository;

		public ExportController(IPageRepository pageRepository)
		{
			_pageRepository = pageRepository;
		}

		[HttpPost]
		[Route(nameof(ExportPagesToXml))]
		public async Task<string> ExportPagesToXml()
		{
			IEnumerable<Page> allPages = await _pageRepository.AllPages();
			XmlSerializer serializer = new XmlSerializer(typeof(List<Page>));

			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				serializer.Serialize(writer, allPages);
				return builder.ToString();
			}
		}

		[HttpPost]
		[Route(nameof(ExportPagesVersionsToXml))]
		public Task<string> ExportPagesVersionsToXml()
		{
			return Task.FromResult("");
		}

		[HttpPost]
		[Route(nameof(ExportAsSql))]
		public Task<string> ExportAsSql()
		{
			return Task.FromResult("");
		}

		[HttpPost]
		[Route(nameof(ExportAttachments))]
		public Task ExportAttachments()
		{
			return Task.CompletedTask;
		}
	}
}
