using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize(Policy = PolicyNames.Admin)]
	public class TagsController : ControllerBase
	{
		private readonly IPageRepository _pageRepository;
		private readonly IPageModelConverter _pageModelConverter;

		public TagsController(IPageRepository pageRepository, IPageModelConverter pageModelConverter)
		{
			_pageRepository = pageRepository;
			_pageModelConverter = pageModelConverter;
		}

		[HttpPost]
		[Route(nameof(Rename))]
		public async Task Rename(string oldTagName, string newTagName)
		{
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTagAsync(oldTagName);

			foreach (Page page in pages)
			{
				page.Tags = Regex.Replace(page.Tags, $@"\s{oldTagName}\s", newTagName);
				await _pageRepository.UpdateExistingAsync(page);
			}
		}

		[HttpGet]
		[Route(nameof(AllTags))]
		public async Task<IEnumerable<TagModel>> AllTags()
		{
			IEnumerable<string> allTags = await _pageRepository.AllTagsAsync();

			var viewModels = new List<TagModel>();
			foreach (string tag in allTags)
			{
				var existingModel = viewModels.FirstOrDefault(x => x.Name == tag);
				if (existingModel != null)
				{
					existingModel.Count += 1;
				}
				else
				{
					viewModels.Add(new TagModel() { Name = tag, Count = 1 });
				}
			}

			return viewModels;
		}

		[HttpGet]
		[Route(nameof(FindPageWithTag))]
		public async Task<IEnumerable<PageModel>> FindPageWithTag(string tag)
		{
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTagAsync(tag);
			return pages.Select(_pageModelConverter.ConvertToViewModel);
		}
	}
}
