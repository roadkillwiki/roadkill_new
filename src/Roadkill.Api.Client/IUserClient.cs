using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;

namespace Roadkill.Api.Client
{
	public interface IUserClient
	{
		Task<bool> ActivateUser(string activationKey);

		Task<bool> AddUser(string email, string username, string password, bool isAdmin, bool isEditor);

		Task<bool> DeleteUser(string email);

		Task<UserResponse> GetUserById(Guid id, bool? isActivated = null);

		Task<UserResponse> GetUser(string email, bool? isActivated = null);

		Task<UserResponse> GetUserByResetKey(string resetKey);

		Task<bool> IsAdmin(string cookieValue);

		Task<bool> IsEditor(string cookieValue);

		Task<IEnumerable<UserResponse>> ListAdmins();

		Task<IEnumerable<UserResponse>> ListEditors();

		Task<string> Signup(UserRequest request, Action completed);

		Task ToggleAdmin(string email);

		Task ToggleEditor(string email);

		Task<bool> UpdateUser(UserRequest request);

		Task<bool> UserExists(string email);

		Task<bool> UserNameExists(string username);

		Task<string> GetLoggedInUserName();

		Task<UserRequest> GetLoggedInUser(string cookieValue);
	}
}
