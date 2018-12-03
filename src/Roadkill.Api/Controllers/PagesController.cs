using System.Collections.Generic;
using System.Linq;
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
    //[ApiController]
    public class PagesController : ControllerBase, IPagesService
    {
        private readonly IPageRepository _pageRepository;
        private readonly IPageModelConverter _pageModelConverter;

        public PagesController(IPageRepository pageRepository, IPageModelConverter pageModelConverter)
        {
            _pageRepository = pageRepository;
            _pageModelConverter = pageModelConverter;
        }

        [HttpPost]
        [Authorize(Policy = "Editors")]
        [Route(nameof(Add))]
        public async Task<PageModel> Add([FromBody] PageModel model)
        {
            // TODO: fill createdon property
            
            Page page = _pageModelConverter.ConvertToPage(model);
            if (page == null)
                return null;

            Page newPage = await _pageRepository.AddNewPage(page);
            return _pageModelConverter.ConvertToViewModel(newPage);
        }

        [HttpPut]
        [Authorize(Policy = "Editors")]
        public async Task<PageModel> Update(PageModel model)
        {
            Page page = _pageModelConverter.ConvertToPage(model);
            if (page == null)
                return null;

            Page newPage = await _pageRepository.UpdateExisting(page);
            return _pageModelConverter.ConvertToViewModel(newPage);
        }

        [HttpDelete]
        [Authorize(Policy = "Admins")]
        public async Task Delete(int pageId)
        {
            await _pageRepository.DeletePage(pageId);
        }

        [HttpGet]
        [Route("Get")]
		[AllowAnonymous]
        public async Task<PageModel> GetById(int id)
        {
            Page page = await _pageRepository.GetPageById(id);
            if (page == null)
                return null;

            return _pageModelConverter.ConvertToViewModel(page);
        }

        [HttpGet]
        [Route(nameof(AllPages))]
        [AllowAnonymous]
        public async Task<IEnumerable<PageModel>> AllPages()
        {
            IEnumerable<Page> allpages = await _pageRepository.AllPages();
            return allpages.Select(_pageModelConverter.ConvertToViewModel);
        }

        [HttpGet]
        [Route(nameof(AllPagesCreatedBy))]
        [AllowAnonymous]
        public async Task<IEnumerable<PageModel>> AllPagesCreatedBy(string username)
        {
            IEnumerable<Page> pagesCreatedBy = await _pageRepository.FindPagesCreatedBy(username);
            return pagesCreatedBy.Select(_pageModelConverter.ConvertToViewModel);
        }

        [HttpGet]
        [Route(nameof(FindHomePage))]
        [AllowAnonymous]
        public async Task<PageModel> FindHomePage()
        {
            IEnumerable<Page> pagesWithHomePageTag = await _pageRepository.FindPagesContainingTag("homepage");

            if (!pagesWithHomePageTag.Any())
                return null;

            Page firstResult = pagesWithHomePageTag.First();
            return _pageModelConverter.ConvertToViewModel(firstResult);
        }

        [HttpGet]
        [Route(nameof(FindByTitle))]
        [AllowAnonymous]
        public async Task<PageModel> FindByTitle(string title)
        {
            Page page = await _pageRepository.GetPageByTitle(title);
            if (page == null)
                return null;

            return _pageModelConverter.ConvertToViewModel(page);
        }
    }
}