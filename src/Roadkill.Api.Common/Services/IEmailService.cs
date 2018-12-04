using System.Threading.Tasks;
using Refit;

namespace Roadkill.Api.Common.Services
{
	public interface IEmailService
	{
		Task Send(string from, string to, string subject, string body);
	}
}
