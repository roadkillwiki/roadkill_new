using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoFixture;
using Moq;
using Roadkill.Api.Controllers;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class ExportControllerTests : IDisposable
	{
		private Mock<IPageRepository> _pageRepositoryMock;
		private ExportController _exportController;
		private Fixture _fixture;

		public ExportControllerTests()
		{
			_fixture = new Fixture();

			_pageRepositoryMock = new Mock<IPageRepository>();
			_exportController = new ExportController(_pageRepositoryMock.Object);
		}

		[Fact]
		public async Task ExportToXml()
		{
			// given
			List<Page> actualPages = _fixture.CreateMany<Page>().ToList();

			_pageRepositoryMock.Setup(x => x.AllPages())
				.ReturnsAsync(actualPages);

			XmlSerializer serializer = new XmlSerializer(typeof(List<Page>));

			// when
			string actualXml = await _exportController.ExportPagesToXml();

			// then
			var deserializedPages = serializer.Deserialize(new StringReader(actualXml)) as List<Page>;
			deserializedPages.Count.ShouldBe(actualPages.Count());
		}

		public void Dispose()
		{
			_exportController?.Dispose();
		}
	}
}
