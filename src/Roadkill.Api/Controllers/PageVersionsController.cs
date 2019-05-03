using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Models;
using Roadkill.Api.ModelConverters;
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
		private readonly IPageVersionModelConverter _modelConverter;

		public PageVersionsController(IPageVersionRepository pageVersionRepository, IPageVersionModelConverter modelConverter)
		{
			_pageVersionRepository = pageVersionRepository;
			_modelConverter = modelConverter;
		}

		[HttpPost]
		public async Task<PageVersionModel> Add(int pageId, string text, string author, DateTime? dateTime = null)
		{
			PageVersion pageVersion = await _pageVersionRepository.AddNewVersionAsync(pageId, text, author, dateTime);

			return _modelConverter.ConvertToViewModel(pageVersion);
		}

		[HttpGet]
		[Route("Get")]
		public async Task<PageVersionModel> GetById(Guid id)
		{
			PageVersion pageVersion = await _pageVersionRepository.GetByIdAsync(id);

			return _modelConverter.ConvertToViewModel(pageVersion);
		}

		[HttpDelete]
		public async Task Delete(Guid id)
		{
			await _pageVersionRepository.DeleteVersionAsync(id);
		}

		[HttpPut]
		public async Task Update(PageVersionModel pageVersionModel)
		{
			PageVersion pageVersion = _modelConverter.ConvertToPageVersion(pageVersionModel);
			await _pageVersionRepository.UpdateExistingVersionAsync(pageVersion);
		}

		[HttpGet]
		[Route(nameof(AllVersions))]
		public async Task<IEnumerable<PageVersionModel>> AllVersions()
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.AllVersionsAsync();
			return pageVersions.Select(_modelConverter.ConvertToViewModel);
		}

		[HttpGet]
		[Route(nameof(FindPageVersionsByPageId))]
		public async Task<IEnumerable<PageVersionModel>> FindPageVersionsByPageId(int pageId)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByPageIdAsync(pageId);
			return pageVersions.Select(_modelConverter.ConvertToViewModel);
		}

		[HttpGet]
		[Route(nameof(FindPageVersionsByAuthor))]
		public async Task<IEnumerable<PageVersionModel>> FindPageVersionsByAuthor(string username)
		{
			IEnumerable<PageVersion> pageVersions = await _pageVersionRepository.FindPageVersionsByAuthorAsync(username);
			return pageVersions.Select(_modelConverter.ConvertToViewModel);
		}

		[HttpGet]
		[Route(nameof(GetLatestVersion))]
		public async Task<PageVersionModel> GetLatestVersion(int pageId)
		{
			PageVersion latestPageVersion = await _pageVersionRepository.GetLatestVersionAsync(pageId);
			if (latestPageVersion == null)
			{
				return null;
			}

			return _modelConverter.ConvertToViewModel(latestPageVersion);
		}
	}
}
