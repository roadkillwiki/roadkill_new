using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Models;
using Roadkill.Api.JWT;
using Roadkill.Api.ModelConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[ApiController] // [ApiController] adds [FromBody] by default and model validation
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	public class PagesController : ControllerBase
	{
		private readonly IPageRepository _pageRepository;

		private readonly IPageModelConverter _pageModelConverter;

		public PagesController(IPageRepository pageRepository, IPageModelConverter pageModelConverter)
		{
			_pageRepository = pageRepository;
			_pageModelConverter = pageModelConverter;
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("{id}")]
		public async Task<ActionResult<PageModel>> Get(int id)
		{
			Page page = await _pageRepository.GetPageById(id);
			if (page == null)
			{
				return NotFound();
			}

			return _pageModelConverter.ConvertToViewModel(page);
		}

		[HttpPost]
		[Authorize(Policy = PolicyNames.Editor)]
		public async Task<ActionResult<PageModel>> Add([FromBody] PageModel model)
		{
			// TODO: add base62 ID, as Id is Hilo
			// http://www.anotherchris.net/csharp/friendly-unique-id-generation-part-2/
			// TODO: fill createdon property
			Page page = _pageModelConverter.ConvertToPage(model);
			if (page == null)
			{
				return NotFound();
			}

			Page newPage = await _pageRepository.AddNewPage(page);
			PageModel newModel = _pageModelConverter.ConvertToViewModel(newPage);

			return CreatedAtAction(nameof(Add), nameof(PagesController), newModel);
		}

		[HttpPut]
		[Authorize(Policy = PolicyNames.Editor)]
		public async Task<ActionResult<PageModel>> Update(PageModel model)
		{
			Page page = _pageModelConverter.ConvertToPage(model);
			if (page == null)
			{
				return NotFound();
			}

			Page newPage = await _pageRepository.UpdateExisting(page);
			return _pageModelConverter.ConvertToViewModel(newPage);
		}

		[HttpDelete]
		[Authorize(Policy = PolicyNames.Admin)]
		public async Task Delete(int pageId)
		{
			await _pageRepository.DeletePage(pageId);
		}

		[HttpGet]
		[Route(nameof(AllPages))]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<PageModel>>> AllPages()
		{
			IEnumerable<Page> allpages = await _pageRepository.AllPages();
			return Ok(allpages.Select(_pageModelConverter.ConvertToViewModel));
		}

		[HttpGet]
		[Route(nameof(AllPagesCreatedBy))]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<PageModel>>> AllPagesCreatedBy(string username)
		{
			IEnumerable<Page> pagesCreatedBy = await _pageRepository.FindPagesCreatedBy(username);

			IEnumerable<PageModel> models = pagesCreatedBy.Select(_pageModelConverter.ConvertToViewModel);
			return Ok(models);
		}

		[HttpGet]
		[Route(nameof(FindHomePage))]
		[AllowAnonymous]
		public async Task<ActionResult<PageModel>> FindHomePage()
		{
			IEnumerable<Page> pagesWithHomePageTag = await _pageRepository.FindPagesContainingTag("homepage");

			if (!pagesWithHomePageTag.Any())
			{
				return NotFound();
			}

			Page firstResult = pagesWithHomePageTag.First();
			return _pageModelConverter.ConvertToViewModel(firstResult);
		}

		[HttpGet]
		[Route(nameof(FindByTitle))]
		[AllowAnonymous]
		public async Task<ActionResult<PageModel>> FindByTitle(string title)
		{
			Page page = await _pageRepository.GetPageByTitle(title);
			if (page == null)
			{
				return NotFound();
			}

			return _pageModelConverter.ConvertToViewModel(page);
		}
	}
}
