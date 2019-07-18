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
using Roadkill.Core.Entities.Authorization;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.ObjectConverters
{
	public class UserObjectsConverterTests
	{
		private UserObjectsConverter _converter;
		private Fixture _fixture;

		public UserObjectsConverterTests()
		{
			_fixture = new Fixture();

			// Use the DI to generate an Automapper IMapper
			var services = new ServiceCollection();
			services.AddAutoMapperForApi();
			var provider = services.BuildServiceProvider();
			var mapper = provider.GetService<IMapper>();

			_converter = new UserObjectsConverter(mapper);
		}

		[Fact]
		public void ConvertToUserResponse()
		{
			var roadkillUser = _fixture.Create<RoadkillIdentityUser>();
			UserResponse userResponse = _converter.ConvertToUserResponse(roadkillUser);

			userResponse.ShouldHaveSamePropertyValuesAs(roadkillUser);
		}
	}
}
