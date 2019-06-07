using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Response;
using Roadkill.Api.JWT;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[Authorize(Policy = PolicyNames.Admin)]
	public class TagsController : ControllerBase
	{
		private readonly IPageRepository _pageRepository;
		private readonly IPageObjectsConverter _pageObjectsConverter;

		public TagsController(
			IPageRepository pageRepository,
			IPageObjectsConverter pageObjectsConverter)
		{
			_pageRepository = pageRepository;
			_pageObjectsConverter = pageObjectsConverter;
		}

		[HttpGet]
		[Route(nameof(AllTags))]
		public async Task<ActionResult<IEnumerable<TagResponse>>> AllTags()
		{
			IEnumerable<string> allTags = await _pageRepository.AllTagsAsync();

			var responses = new List<TagResponse>();
			foreach (string tag in allTags)
			{
				var existingModel = responses.FirstOrDefault(x => x.Name == tag);
				if (existingModel != null)
				{
					existingModel.Count += 1;
				}
				else
				{
					responses.Add(new TagResponse() { Name = tag, Count = 1 });
				}
			}

			return Ok(responses);
		}

		[HttpGet]
		[Route(nameof(FindPageWithTag))]
		public async Task<ActionResult<IEnumerable<PageResponse>>> FindPageWithTag(string tag)
		{
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTagAsync(tag);
			IEnumerable<PageResponse> responses = pages.Select(_pageObjectsConverter.ConvertToPageResponse);

			return Ok(responses);
		}

		[HttpPut]
		[Route(nameof(Rename))]
		public async Task<ActionResult<string>> Rename(string oldTagName, string newTagName)
		{
			IEnumerable<Page> pages = await _pageRepository.FindPagesContainingTagAsync(oldTagName);

			foreach (Page page in pages)
			{
				page.Tags = Regex.Replace(page.Tags, $@"\s{oldTagName}\s", newTagName);
				await _pageRepository.UpdateExistingAsync(page);
			}

			return NoContent();
		}
	}
}
