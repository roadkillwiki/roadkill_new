using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Policies;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[AuthorizeWithBearer]
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
		public Task<ActionResult<string>> ConvertToHtml(string markDown)
		{
			PageHtml result = _textMiddlewareBuilder.Execute(markDown);
			ActionResult<string> actionResult = Ok(result.Html);

			return Task.FromResult(actionResult);
		}

		[HttpPost]
		[Route(nameof(UpdateLinksToPage))]
		[Authorize(Policy = PolicyNames.MarkdownUpdateLinks)]
		public Task UpdateLinksToPage(string oldTitle, string newTitle)
		{
			throw new System.NotImplementedException();
		}
	}
}
