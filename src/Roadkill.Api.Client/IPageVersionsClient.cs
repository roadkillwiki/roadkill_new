using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Common.Request;

namespace Roadkill.Api.Client
{
	public interface IPageVersionsClient
	{
		Task<PageVersionRequest> Add(int pageId, string text, string author, DateTime? dateTime = null);

		Task<PageVersionRequest> GetById(Guid id);

		Task Delete(Guid id);

		Task Update(PageVersionRequest pageVersionRequest);

		Task<PageVersionRequest> GetLatestVersion(int pageId);

		Task<IEnumerable<PageVersionRequest>> AllVersions();

		Task<IEnumerable<PageVersionRequest>> FindPageVersionsByPageId(int pageId);

		Task<IEnumerable<PageVersionRequest>> FindPageVersionsByAuthor(string username);
	}
}
