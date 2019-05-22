using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Api.JWT;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
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
		public async Task<PageVersionResponse> Get(Guid id)
		{
			PageVersion pageVersion = await _pageVersionRepository.GetByIdAsync(id);

			return _objectsConverter.ConvertToPageVersionResponse(pageVersion);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(AllVersions))]
		public async Task<IEnumerable<PageVersionResponse>> AllVersions()
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.AllVersionsAsync();
			return pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(FindPageVersionsByPageId))]
		public async Task<IEnumerable<PageVersionResponse>> FindPageVersionsByPageId(int pageId)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByPageIdAsync(pageId);
			return pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(FindPageVersionsByAuthor))]
		public async Task<IEnumerable<PageVersionResponse>> FindPageVersionsByAuthor(string username)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByAuthorAsync(username);
			return pageVersions.Select(_objectsConverter.ConvertToPageVersionResponse);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route(nameof(GetLatestVersion))]
		public async Task<PageVersionResponse> GetLatestVersion(int pageId)
		{
			PageVersion latestPageVersion = await _pageVersionRepository.GetLatestVersionAsync(pageId);
			if (latestPageVersion == null)
			{
				return null;
			}

			return _objectsConverter.ConvertToPageVersionResponse(latestPageVersion);
		}

		[HttpPost]
		[Authorize(Policy = PolicyNames.Editor)]
		public async Task<PageVersionResponse> Add(int pageId, string text, string author, DateTime? dateTime = null)
		{
			PageVersion pageVersion = await _pageVersionRepository.AddNewVersionAsync(pageId, text, author, dateTime);

			return _objectsConverter.ConvertToPageVersionResponse(pageVersion);
		}

		[HttpDelete]
		[Authorize(Policy = PolicyNames.Admin)]
		public async Task Delete(Guid id)
		{
			await _pageVersionRepository.DeleteVersionAsync(id);
		}

		[HttpPut]
		[Authorize(Policy = PolicyNames.Admin)]
		public async Task Update(PageVersionRequest pageVersionRequest)
		{
			// doesn't add a new version
			PageVersion pageVersion = _objectsConverter.ConvertToPageVersion(pageVersionRequest);
			await _pageVersionRepository.UpdateExistingVersionAsync(pageVersion);
		}
	}
}
