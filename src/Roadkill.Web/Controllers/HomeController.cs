using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;
using Roadkill.Api.Common.Services;

namespace Roadkill.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPagesService _pageService;

        public HomeController(IPagesService pageService)
        {
            _pageService = pageService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
