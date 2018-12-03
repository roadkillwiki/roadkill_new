using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Interfaces;

namespace Roadkill.Api.Controllers
{
	[Route("files")]
	public class FileController : Controller, IFileService
	{
		public FileController()
		{
		}
	}
}