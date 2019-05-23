using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.JWT;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize(Policy = PolicyNames.Editor)]
	public class MarkdownController : ControllerBase
	{
		private readonly ITextMiddlewareBuilder _textMiddlewareBuilder;

		public MarkdownController(ITextMiddlewareBuilder textMiddlewareBuilder)
		{
			_textMiddlewareBuilder = textMiddlewareBuilder;
		}

		[HttpPost]
		[Route(nameof(ConvertToHtml))]
		[AllowAnonymous]
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
