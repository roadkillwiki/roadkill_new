using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class FileController : ControllerBase
	{
		public FileController()
		{
		}
	}
}
