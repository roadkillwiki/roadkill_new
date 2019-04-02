using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Roadkill.Api.Common.Models;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ModelConverters
{
	public interface IPageModelConverter
	{
		PageModel ConvertToViewModel(Page page);

		Page ConvertToPage(PageModel model);
	}

	public class PageModelConverter : IPageModelConverter
	{
		public PageModel ConvertToViewModel(Page page)
		{
			return new PageModel()
			{
				Id = page.Id,
				Title = page.Title,
				SeoFriendlyTitle = CreateSeoFriendlyPageTitle(page.Title),
				TagsAsCsv = page.Tags,
				TagList = TagsToList(page.Tags),
				LastModifiedBy = page.LastModifiedBy,
				LastModifiedOn = page.LastModifiedOn,
				CreatedBy = page.CreatedBy,
				CreatedOn = page.CreatedOn,
				IsLocked = page.IsLocked
			};
		}

		public Page ConvertToPage(PageModel model)
		{
			return new Page()
			{
				Id = model.Id,
				Title = model.Title,
				LastModifiedBy = model.LastModifiedBy,
				LastModifiedOn = model.LastModifiedOn,
				CreatedBy = model.CreatedBy,
				CreatedOn = model.CreatedOn,
				IsLocked = model.IsLocked,
				Tags = model.TagsAsCsv
			};
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
