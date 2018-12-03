using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Interfaces;
using Roadkill.Text;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;

namespace Roadkill.Api.Controllers
{
	[Route("markdown")]
	public class MarkdownController : Controller, IMarkdownService
	{
		private readonly ITextMiddlewareBuilder _textMiddlewareBuilder;

		public MarkdownController(ITextMiddlewareBuilder textMiddlewareBuilder)
		{
			_textMiddlewareBuilder = textMiddlewareBuilder;
		}

		[HttpPost]
		[Route(nameof(ConvertToHtml))]
		public Task<string> ConvertToHtml(string markDown)
		{
			PageHtml result = _textMiddlewareBuilder.Execute(markDown);
			return Task.FromResult(result.Html);
		}

		[HttpPost]
		[Route(nameof(UpdateLinksToPage))]
		public Task UpdateLinksToPage(string oldTitle, string newTitle)
		{
			throw new System.NotImplementedException();
		}
	}
}