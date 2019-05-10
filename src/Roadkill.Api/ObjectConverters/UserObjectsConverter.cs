using AutoMapper;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Authorization;

namespace Roadkill.Api.ObjectConverters
{
	public interface IUserObjectsConverter
	{
		UserResponse ConvertToUserResponse(RoadkillIdentityUser roadkillIdentityUser);

		// Skip conversions to RoadkillIdentityUser, as it's really an IdentityUser,
		// with custom logic to its properties (not a simple shallow copy).
	}

	public class UserObjectsConverter : IUserObjectsConverter
	{
		private readonly IMapper _mapper;

		public UserObjectsConverter(IMapper mapper)
		{
			_mapper = mapper;
		}

		public UserResponse ConvertToUserResponse(RoadkillIdentityUser roadkillIdentityUser)
		{
			return _mapper.Map<UserResponse>(roadkillIdentityUser);
		}
	}
}
