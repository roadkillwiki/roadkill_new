using System;
using System.Reflection;
using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.Extensions;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.ObjectConverters
{
	public class PageVersionObjectsConverterTests
	{
		private PageVersionObjectsConverter _converter;
		private Fixture _fixture;

		public PageVersionObjectsConverterTests()
		{
			_fixture = new Fixture();

			// Use the DI to generate an Automapper IMapper
			var services = new ServiceCollection();
			services.AddAutoMapperForApi();
			var provider = services.BuildServiceProvider();
			var mapper = provider.GetService<IMapper>();

			_converter = new PageVersionObjectsConverter(mapper);
		}

		[Fact]
		public void ConvertToPageVersionResponse()
		{
			var pageVersion = _fixture.Create<PageVersion>();
			PageVersionResponse versionResponse = _converter.ConvertToPageVersionResponse(pageVersion);

			versionResponse.ShouldHaveSamePropertyValuesAs(pageVersion);
		}

		[Fact]
		public void ConvertToPageVersion()
		{
			var pageVersionRequest = _fixture.Create<PageVersionRequest>();
			PageVersion pageVersion = _converter.ConvertToPageVersion(pageVersionRequest);

			pageVersion.ShouldHaveSamePropertyValuesAs(pageVersionRequest);
		}
	}
}
