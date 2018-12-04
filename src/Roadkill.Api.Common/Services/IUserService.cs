using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Common.Services
{
	public interface IUserService
	{
		Task<bool> ActivateUser(string activationKey);

		Task<bool> AddUser(string email, string username, string password, bool isAdmin, bool isEditor);

		Task<bool> Authenticate(string email, string password);

		Task ChangePassword(string email, string newPassword);

		Task<bool> ChangePassword(string email, string oldPassword, string newPassword);

		Task<bool> DeleteUser(string email);

		Task<UserModel> GetUserById(Guid id, bool? isActivated = null);

		Task<UserModel> GetUser(string email, bool? isActivated = null);

		Task<UserModel> GetUserByResetKey(string resetKey);

		Task<bool> IsAdmin(string cookieValue);

		Task<bool> IsEditor(string cookieValue);

		Task<IEnumerable<UserModel>> ListAdmins();

		Task<IEnumerable<UserModel>> ListEditors();

		Task Logout();

		Task<string> ResetPassword(string email);

		Task<string> Signup(UserModel model, Action completed);

		Task ToggleAdmin(string email);

		Task ToggleEditor(string email);

		Task<bool> UpdateUser(UserModel model);

		Task<bool> UserExists(string email);

		Task<bool> UserNameExists(string username);

		Task<string> GetLoggedInUserName();

		Task<UserModel> GetLoggedInUser(string cookieValue);
	}
}
