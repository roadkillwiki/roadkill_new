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
	public class PageObjectsConverterTests
	{
		private PageObjectsConverter _converter;
		private Fixture _fixture;

		public PageObjectsConverterTests()
		{
			_fixture = new Fixture();

			// Use the DI to generate an Automapper IMapper
			var services = new ServiceCollection();
			services.AddAutoMapperForApi();
			var provider = services.BuildServiceProvider();
			var mapper = provider.GetService<IMapper>();

			_converter = new PageObjectsConverter(mapper);
		}

		[Fact]
		public void ConvertToPageResponse()
		{
			var page = _fixture.Create<Page>();
			PageResponse pageResponse = _converter.ConvertToPageResponse(page);

			pageResponse.ShouldHaveSamePropertyValuesAs(page);
		}

		[Fact]
		public void ConvertToPage()
		{
			var pageRequest = _fixture.Create<PageRequest>();
			Page page = _converter.ConvertToPage(pageRequest);

			page.ShouldHaveSamePropertyValuesAs(pageRequest);
		}
	}
}
