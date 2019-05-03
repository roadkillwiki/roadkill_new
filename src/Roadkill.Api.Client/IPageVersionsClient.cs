using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Client
{
	public interface IPageVersionsClient
	{
		Task<PageVersionModel> Add(int pageId, string text, string author, DateTime? dateTime = null);

		Task<PageVersionModel> GetById(Guid id);

		Task Delete(Guid id);

		Task Update(PageVersionModel pageVersionModel);

		Task<PageVersionModel> GetLatestVersion(int pageId);

		Task<IEnumerable<PageVersionModel>> AllVersions();

		Task<IEnumerable<PageVersionModel>> FindPageVersionsByPageId(int pageId);

		Task<IEnumerable<PageVersionModel>> FindPageVersionsByAuthor(string username);
	}
}
