using System.Threading.Tasks;

namespace Roadkill.Api.Common.Services
{
	public interface IAuthenticationService
	{
		Task ChangePassword(string email, string newPassword);

		Task<bool> ChangePassword(string email, string oldPassword, string newPassword);

		Task<string> ResetPassword(string email);
	}
}
