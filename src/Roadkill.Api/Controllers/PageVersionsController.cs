using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Policies;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[AuthorizeWithBearer]
	public class PageVersionsController : ControllerBase
	{
		private readonly IPageVersionRepository _pageVersionRepository;
		private readonly IPageVersionObjectsConverter _objectsConverter;

		public PageVersionsController(
			IPageVersionRepository pageVersionRepository,
			IPageVersionObjectsConverter objectsConverter)
		{
			_pageVersionRepository = pageVersionRepository;
			_objectsConverter = objectsConverter;
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("{id}")]
		public async Task<ActionResult<PageVersionResponse>> Get(Guid id)
		{
			PageVersion pageVersion = await _pageVersionRepository.GetByIdAsync(id);
			PageVersionResponse responses = _objectsConverter.ConvertToPageVersionResponse(pageVersion);

			return Ok(responses);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(AllVersions))]
		public async Task<ActionResult<IEnumerable<PageVersionResponse>>> AllVersions()
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.AllVersionsAsync();
			IEnumerable<PageVersionResponse> responses = pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);

			return Ok(responses);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(FindPageVersionsByPageId))]
		public async Task<ActionResult<IEnumerable<PageVersionResponse>>> FindPageVersionsByPageId(int pageId)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByPageIdAsync(pageId);
			IEnumerable<PageVersionResponse> responses = pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);

			return Ok(responses);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(FindPageVersionsByAuthor))]
		public async Task<ActionResult<IEnumerable<PageVersionResponse>>> FindPageVersionsByAuthor(string username)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByAuthorAsync(username);
			IEnumerable<PageVersionResponse> responses = pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);

			return Ok(responses);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(GetLatestVersion))]
		public async Task<ActionResult<PageVersionResponse>> GetLatestVersion(int pageId)
		{
			PageVersion latestPageVersion = await _pageVersionRepository.GetLatestVersionAsync(pageId);
			if (latestPageVersion == null)
			{
				return null;
			}

			PageVersionResponse response = _objectsConverter.ConvertToPageVersionResponse(latestPageVersion);
			return Ok(response);
		}

		[HttpPost]
		[Authorize(Policy = PolicyNames.AddPageVersion)]
		public async Task<ActionResult<PageVersionResponse>> Add(int pageId, string text, string author, DateTime? dateTime = null)
		{
			PageVersion pageVersion = await _pageVersionRepository.AddNewVersionAsync(pageId, text, author, dateTime);
			PageVersionResponse response = _objectsConverter.ConvertToPageVersionResponse(pageVersion);

			return CreatedAtAction(nameof(Add), response);
		}

		[HttpDelete]
		[Authorize(Policy = PolicyNames.DeletePageVersion)]
		public async Task<ActionResult<string>> Delete(Guid id)
		{
			await _pageVersionRepository.DeleteVersionAsync(id);
			return NoContent();
		}

		[HttpPut]
		[Authorize(Policy = PolicyNames.UpdatePageVersion)]
		public async Task<ActionResult<string>> Update(PageVersionRequest pageVersionRequest)
		{
			// doesn't add a new version
			PageVersion pageVersion = _objectsConverter.ConvertToPageVersion(pageVersionRequest);
			await _pageVersionRepository.UpdateExistingVersionAsync(pageVersion);

			return NoContent();
		}
	}
}
