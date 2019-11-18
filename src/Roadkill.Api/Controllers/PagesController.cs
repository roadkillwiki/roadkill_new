using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	/// <summary>
	/// Actions related to pages in the wiki, such as creation and retrieval.
	/// </summary>
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[AuthorizeWithBearer]
	public class PagesController : ControllerBase
	{
		// [ApiController] adds [FromBody] by default and model validation
		private readonly IPageRepository _pageRepository;
		private readonly IPageObjectsConverter _pageObjectsConverter;

		public PagesController(
			IPageRepository pageRepository,
			IPageObjectsConverter pageObjectsConverter)
		{
			_pageRepository = pageRepository;
			_pageObjectsConverter = pageObjectsConverter;
		}

		/// <summary>
		/// Gets a single page by its ID.
		/// </summary>
		/// <param name="id">The unique ID of the page to retrieve.</param>
		/// <returns>The page meta information, or a 404 not found if the page cannot be found.
		/// No page text is returned, use PageVersions to get this information.</returns>
		[HttpGet]
		[AllowAnonymous]
		[Route("{id}")]
		public async Task<ActionResult<PageResponse>> Get(int id)
		{
			Page page = await _pageRepository.GetPageByIdAsync(id);
			if (page == null)
			{
				return NotFound();
			}

			return _pageObjectsConverter.ConvertToPageResponse(page);
		}

		/// <summary>
		/// Retrieves all pages in the Roadkill database.
		/// </summary>
		/// <returns>Meta information for all the pages in the database.
		/// No page text is returned, use PageVersions to get this information.</returns>
		[HttpGet]
		[Route(nameof(AllPages))]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<PageResponse>>> AllPages()
		{
			IEnumerable<Page> allpages = await _pageRepository.AllPagesAsync();
			return Ok(allpages.Select(_pageObjectsConverter.ConvertToPageResponse));
		}

		/// <summary>
		/// Retrieves all pages created by a particular user.
		/// </summary>
		/// <param name="username">The username (typically an email address) of the user that created
		/// the the pages.</param>
		/// <returns>Meta information for all the pages created by the user in the database.
		/// No page text is returned, use PageVersions to get this information.</returns>
		[HttpGet]
		[Route(nameof(AllPagesCreatedBy))]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<PageResponse>>> AllPagesCreatedBy(string username)
		{
			IEnumerable<Page> pagesCreatedBy = await _pageRepository.FindPagesCreatedByAsync(username);

			IEnumerable<PageResponse> models = pagesCreatedBy.Select(_pageObjectsConverter.ConvertToPageResponse);
			return Ok(models);
		}

		/// <summary>
		/// Finds the first page in the database with the "homepage" tag.
		/// </summary>
		/// <returns>The page meta information, or a 404 not found if the page cannot be found.
		/// No page text is returned, use PageVersions to get this information.</returns>
		[HttpGet]
		[Route(nameof(FindHomePage))]
		[AllowAnonymous]
		public async Task<ActionResult<PageResponse>> FindHomePage()
		{
			IEnumerable<Page> pagesWithHomePageTag = await _pageRepository.FindPagesContainingTagAsync("homepage");

			if (!pagesWithHomePageTag.Any())
			{
				return NotFound();
			}

			Page firstResult = pagesWithHomePageTag.First();
			PageResponse response = _pageObjectsConverter.ConvertToPageResponse(firstResult);
			return Ok(response);
		}

		/// <summary>
		/// Finds the first page matching the given page title.
		/// </summary>
		/// <param name="title">The title of the page to search for (case-insensitive).</param>
		/// <returns>The page meta information, or a 404 not found if the page cannot be found.
		/// No page text is returned, use PageVersions to get this information.</returns>
		[HttpGet]
		[Route(nameof(FindByTitle))]
		[AllowAnonymous]
		public async Task<ActionResult<PageResponse>> FindByTitle(string title)
		{
			Page page = await _pageRepository.GetPageByTitleAsync(title);
			if (page == null)
			{
				return NotFound();
			}

			PageResponse response = _pageObjectsConverter.ConvertToPageResponse(page);
			return Ok(response);
		}

		/// <summary>
		/// Add a page to the database using the provided meta information. This will only add
		/// the meta information not the page text, use PageVersions to add text for a page.
		/// </summary>
		/// <param name="pageRequest">The page information to add.</param>
		/// <returns>A 202 HTTP status with the newly created page, with its generated ID populated.</returns>
		[HttpPost]
		[Authorize(Policy = PolicyNames.AddPage)]
		public async Task<ActionResult<PageResponse>> Add([FromBody] PageRequest pageRequest)
		{
			// TODO: add base62 ID, as Id in Marten is Hilo and starts at 1000 as the lo
			// TODO: fill createdon property
			// TODO: validate
			// http://www.anotherchris.net/csharp/friendly-unique-id-generation-part-2/
			Page page = _pageObjectsConverter.ConvertToPage(pageRequest);
			if (page == null)
			{
				return NotFound();
			}

			Page newPage = await _pageRepository.AddNewPageAsync(page);
			PageResponse response = _pageObjectsConverter.ConvertToPageResponse(newPage);

			return CreatedAtAction(nameof(Add), nameof(PagesController), response);
		}

		/// <summary>
		/// Updates an existing page in the database.
		/// </summary>
		/// <param name="pageRequest">The page details to update, which should include the page id.</param>
		/// <returns>The update page details, or a 404 not found if the existing page cannot be found</returns>
		[HttpPut]
		[Authorize(Policy = PolicyNames.UpdatePage)]
		public async Task<ActionResult<PageResponse>> Update(PageRequest pageRequest)
		{
			Page page = _pageObjectsConverter.ConvertToPage(pageRequest);
			if (page == null)
			{
				return NotFound();
			}

			await _pageRepository.UpdateExistingAsync(page);
			return NoContent();
		}

		/// <summary>
		/// Deletes an existing page from the database. This is an administrator-only action.
		/// </summary>
		/// <param name="pageId">The id of the page to remove.</param>
		/// <returns>A 204 if the page successfully deleted.</returns>
		[HttpDelete]
		[Authorize(Policy = PolicyNames.DeletePage)]
		public async Task<ActionResult<string>> Delete(int pageId)
		{
			await _pageRepository.DeletePageAsync(pageId);
			return NoContent();
		}
	}
}
