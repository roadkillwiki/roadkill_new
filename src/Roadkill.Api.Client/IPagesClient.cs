using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Roadkill.Api.Common.Response;

namespace Roadkill.Api.Client
{
	public interface IPagesClient
	{
		[Post("/pages/")]
		[Headers("Authorization: Bearer")]
		Task<PageResponse> Add([Body] PageResponse response);

		[Put("/pages/")]
		[Headers("Authorization: Bearer")]
		Task<PageResponse> Update([Body] PageResponse response);

		[Delete("/pages/{pageId}")]
		[Headers("Authorization: Bearer")]
		Task Delete(int pageId);

		[Get("/pages/{pageId}")]
		Task<PageResponse> Get(int pageId);

		[Get("/pages/allpages")]
		Task<IEnumerable<PageResponse>> AllPages();

		[Get("/pages/allpagescreatedby")]
		Task<IEnumerable<PageResponse>> AllPagesCreatedBy(string username);

		[Get("/pages/findhomepage")]
		Task<PageResponse> FindHomePage();

		[Get("/pages/findbytitle")]
		Task<PageResponse> FindByTitle(string title);
	}
}
