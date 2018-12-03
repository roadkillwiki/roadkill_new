using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Interfaces;
using Roadkill.Api.Models;
using Roadkill.Core.Models;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class TagsController : Controller, ITagsService
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
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTag(oldTagName);

			foreach (Page page in pages)
			{
				page.Tags = Regex.Replace(page.Tags, $@"\s{oldTagName}\s", newTagName);
				await _pageRepository.UpdateExisting(page);
			}
		}

		[HttpGet]
		[Route(nameof(AllTags))]
		public async Task<IEnumerable<TagModel>> AllTags()
		{
			IEnumerable<string> allTags = await _pageRepository.AllTags();

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
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTag(tag);
			return pages.Select(_pageModelConverter.ConvertToViewModel);
		}
	}
}