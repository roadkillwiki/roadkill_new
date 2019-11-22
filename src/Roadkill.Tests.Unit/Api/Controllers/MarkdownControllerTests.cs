using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Policies;
using Roadkill.Api.Controllers;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class MarkdownControllerTests
	{
		private readonly ITextMiddlewareBuilder _textMiddlewareMock;
		private readonly MarkdownController _markdownController;

		public MarkdownControllerTests()
		{
			_textMiddlewareMock = Substitute.For<ITextMiddlewareBuilder>();
			_markdownController = new MarkdownController(_textMiddlewareMock);
		}

		[Fact]
		public void ConvertToHtml_should_be_HttpPost_and_allowanonymous()
		{
			string methodName = nameof(MarkdownController.ConvertToHtml);
			Type attributeType = typeof(HttpPostAttribute);

			_markdownController.ShouldHaveAttribute(methodName, attributeType);
			_markdownController.ShouldAllowAnonymous(methodName);
		}

		[Fact]
		public void UpdateLinksToPage_should_be_HttpPost_and_authorize_policy()
		{
			string methodName = nameof(MarkdownController.UpdateLinksToPage);
			Type attributeType = typeof(HttpPostAttribute);

			_markdownController.ShouldHaveAttribute(methodName, attributeType);
			_markdownController.ShouldAuthorizePolicy(methodName, PolicyNames.MarkdownUpdateLinks);
		}

		[Fact]
		public async Task ConvertToHtml()
		{
			// given
			string expectedHtml = "<html>";
			string markdown = "a bit of markdown";

			_textMiddlewareMock.Execute(markdown)
				.Returns(new PageHtml(expectedHtml));

			// when
			ActionResult<string> actionResult = await _markdownController.ConvertToHtml(markdown);

			// then
			actionResult.ShouldBeOkObjectResult();

			string actualHtml = actionResult.GetOkObjectResultValue();
			actualHtml.ShouldBe(expectedHtml);
		}
	}
}
