using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ObjectConverters
{
	public interface IPageObjectsConverter
	{
		PageResponse ConvertToPageResponse(Page page);

		Page ConvertToPage(PageRequest pageRequest);
	}

	public class PageObjectsConverter : IPageObjectsConverter
	{
		private readonly IMapper _mapper;

		public PageObjectsConverter(IMapper mapper)
		{
			_mapper = mapper;
		}

		public PageResponse ConvertToPageResponse(Page page)
		{
			var pageResponse = _mapper.Map<PageResponse>(page);
			pageResponse.SeoFriendlyTitle = CreateSeoFriendlyPageTitle(page.Title);
			pageResponse.TagList = TagsToList(page.Tags);

			return pageResponse;
		}

		public Page ConvertToPage(PageRequest pageRequest)
		{
			return _mapper.Map<Page>(pageRequest);
		}

		private static IEnumerable<string> TagsToList(string csvTags)
		{
			List<string> tagList = new List<string>();
			char delimiter = ',';

			if (!string.IsNullOrEmpty(csvTags))
			{
				// For the legacy tag seperator format
				if (csvTags.IndexOf(";", StringComparison.Ordinal) != -1)
				{
					delimiter = ';';
				}

				if (csvTags.IndexOf(delimiter, StringComparison.Ordinal) != -1)
				{
					string[] parts = csvTags.Split(delimiter);
					foreach (string item in parts)
					{
						if (item != "," && !string.IsNullOrWhiteSpace(item))
						{
							tagList.Add(item.Trim());
						}
					}
				}
				else
				{
					tagList.Add(csvTags.TrimEnd());
				}
			}

			return tagList;
		}

		private static string CreateSeoFriendlyPageTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return title;
			}

			// Search engine friendly slug routine with help from http://www.intrepidstudios.com/blog/2009/2/10/function-to-generate-a-url-friendly-string.aspx

			// remove invalid characters
			title = Regex.Replace(title, @"[^\w\d\s-]", "");  // this is unicode safe, but may need to revert back to 'a-zA-Z0-9', need to check spec

			// convert multiple spaces/hyphens into one space
			title = Regex.Replace(title, @"[\s-]+", " ").Trim();

			// If it's over 30 chars, take the first 30.
			title = title.Substring(0, title.Length <= 75 ? title.Length : 75).Trim();

			// hyphenate spaces
			title = Regex.Replace(title, @"\s", "-");

			return title;
		}
	}
}
