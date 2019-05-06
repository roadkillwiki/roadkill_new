using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
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
		private Fixture _fixture;

		public MarkdownControllerTests()
		{
			_fixture = new Fixture();

			_textMiddlewareMock = Substitute.For<ITextMiddlewareBuilder>();
			_markdownController = new MarkdownController(_textMiddlewareMock);
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
			_textMiddlewareMock
				.Received(1)
				.Execute(markdown);
		}
	}
}
