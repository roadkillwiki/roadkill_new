using System.Threading.Tasks;

namespace Roadkill.Api.Client
{
	public interface IAuthenticationClient
	{
		Task ChangePassword(string email, string newPassword);

		Task<bool> ChangePassword(string email, string oldPassword, string newPassword);

		Task<string> ResetPassword(string email);
	}
}
