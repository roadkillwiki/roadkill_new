using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Roadkill.Api.Client;

namespace Roadkill.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly IPagesClient _pageClient;

		public HomeController(IPagesClient pageClient)
		{
			_pageClient = pageClient;
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
