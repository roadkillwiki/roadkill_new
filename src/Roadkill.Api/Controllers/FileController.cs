using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	[AuthorizeWithBearer]
	public class FileController : ControllerBase
	{
		public FileController()
		{
		}
	}
}
