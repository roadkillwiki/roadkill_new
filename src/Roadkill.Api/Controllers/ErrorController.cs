using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Roadkill.Api.Controllers
{
	[Route("[controller]")]
	public class ErrorController : Controller
	{
		public IActionResult Index()
		{
			var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
			return Json(feature.Error.ToString());
		}
	}
}
