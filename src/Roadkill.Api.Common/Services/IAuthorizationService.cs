using System.Threading.Tasks;
using Refit;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Common.Services
{
	public interface IAuthorizationService
	{
		[Post("/authorization/authenticate")]
		Task<string> Authenticate([Body] AuthenticationModel model);
	}
}
