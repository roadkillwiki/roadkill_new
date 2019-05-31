using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Api.JWT;
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
		private Fixture _fixture;

		public MarkdownControllerTests()
		{
			_fixture = new Fixture();

			_textMiddlewareMock = Substitute.For<ITextMiddlewareBuilder>();
			_markdownController = new MarkdownController(_textMiddlewareMock);
		}

		[Fact]
		public void Controller_should_require_editor_access()
		{
			Type attributeType = typeof(AuthorizeAttribute);

			var customAttributes = typeof(MarkdownController).GetCustomAttributes(attributeType, false);
			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for MarkdownController");

			AuthorizeAttribute authorizeAttribute = customAttributes[0] as AuthorizeAttribute;
			authorizeAttribute?.Policy.ShouldNotBeNullOrEmpty("No AuthorizeAttribute policy string specified for MarkdownController");
			authorizeAttribute?.Policy.ShouldContain(PolicyNames.Editor);
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
		public void UpdateLinksToPage_should_be_HttpPost()
		{
			string methodName = nameof(MarkdownController.UpdateLinksToPage);
			Type attributeType = typeof(HttpPostAttribute);

			_markdownController.ShouldHaveAttribute(methodName, attributeType);
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
			string actualHtml = await _markdownController.ConvertToHtml(markdown);

			// then
			actualHtml.ShouldBe(expectedHtml);
		}
	}
}
