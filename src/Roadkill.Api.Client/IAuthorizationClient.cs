using System.Threading.Tasks;
using Refit;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Client
{
	public interface IAuthorizationClient
	{
		[Post("/authorization/authenticate")]
		Task<string> Authenticate([Body] AuthenticationModel model);
	}
}
