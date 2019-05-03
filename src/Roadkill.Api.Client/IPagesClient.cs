using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Roadkill.Api.Common.Models;

namespace Roadkill.Api.Client
{
	public interface IPagesClient
	{
		[Post("/pages/")]
		[Headers("Authorization: Bearer")]
		Task<PageModel> Add([Body] PageModel model);

		[Put("/pages/")]
		[Headers("Authorization: Bearer")]
		Task<PageModel> Update([Body] PageModel model);

		[Delete("/pages/{pageId}")]
		[Headers("Authorization: Bearer")]
		Task Delete(int pageId);

		[Get("/pages/{pageId}")]
		Task<PageModel> Get(int pageId);

		[Get("/pages/allpages")]
		Task<IEnumerable<PageModel>> AllPages();

		[Get("/pages/allpagescreatedby")]
		Task<IEnumerable<PageModel>> AllPagesCreatedBy(string username);

		[Get("/pages/findhomepage")]
		Task<PageModel> FindHomePage();

		[Get("/pages/findbytitle")]
		Task<PageModel> FindByTitle(string title);
	}
}
