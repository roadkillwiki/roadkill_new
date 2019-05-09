using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Authorization;

namespace Roadkill.Api.ObjectConverters
{
	public interface IUserObjectsConverter
	{
		UserResponse ConvertToUserResponse(RoadkillUser roadkillUser);

		RoadkillUser ConvertToRoadkillUser(UserRequest userRequest);
	}

	public class UserObjectsConverter : IUserObjectsConverter
	{
		public UserResponse ConvertToUserResponse(RoadkillUser roadkillUser)
		{
			return new UserResponse()
			{
				Id = roadkillUser.Id,
				Email = roadkillUser.Email,
				UserName = roadkillUser.UserName,
				LockoutEnd = roadkillUser.LockoutEnd,
				RoleClaims = roadkillUser.RoleClaims,
				PhoneNumber = roadkillUser.PhoneNumber,
				PasswordHash = roadkillUser.PasswordHash,
				SecurityStamp = roadkillUser.SecurityStamp,
				EmailConfirmed = roadkillUser.EmailConfirmed,
				LockoutEnabled = roadkillUser.LockoutEnabled,
				NormalizedEmail = roadkillUser.NormalizedEmail,
				ConcurrencyStamp = roadkillUser.ConcurrencyStamp,
				TwoFactorEnabled = roadkillUser.TwoFactorEnabled,
				AccessFailedCount = roadkillUser.AccessFailedCount,
				NormalizedUserName = roadkillUser.NormalizedUserName,
				PhoneNumberConfirmed = roadkillUser.PhoneNumberConfirmed
			};
		}

		public RoadkillUser ConvertToRoadkillUser(UserRequest userRequest)
		{
			return new RoadkillUser()
			{
				UserName = userRequest.Email,
				Email = userRequest.Email,
				EmailConfirmed = true
			};
		}
	}
}
