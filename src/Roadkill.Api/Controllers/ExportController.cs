using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Services;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class ExportController : Controller, IExportService
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
		public async Task<string> ExportPagesVersionsToXml()
		{
			throw new System.NotImplementedException();
		}

		[HttpPost]
		[Route(nameof(ExportAsSql))]
		public async Task<string> ExportAsSql()
		{
			throw new System.NotImplementedException();
		}

		[HttpPost]
		[Route(nameof(ExportAttachments))]
		public async Task ExportAttachments()
		{
			throw new System.NotImplementedException();
		}
	}
}
