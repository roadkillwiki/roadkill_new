using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Services;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class FileController : Controller, IFileService
	{
		public FileController()
		{
		}
	}
}
