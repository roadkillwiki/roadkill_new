using AutoMapper;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ObjectConverters
{
	public interface IPageVersionObjectsConverter
	{
		PageVersionResponse ConvertToPageVersionResponse(PageVersion pageVersion);

		PageVersion ConvertToPageVersion(PageVersionRequest request);
	}

	public class PageVersionObjectsConverter : IPageVersionObjectsConverter
	{
		private readonly IMapper _mapper;

		public PageVersionObjectsConverter(IMapper mapper)
		{
			_mapper = mapper;
		}

		public PageVersionResponse ConvertToPageVersionResponse(PageVersion pageVersion)
		{
			return _mapper.Map<PageVersionResponse>(pageVersion);
		}

		public PageVersion ConvertToPageVersion(PageVersionRequest request)
		{
			return _mapper.Map<PageVersion>(request);
		}
	}
}
