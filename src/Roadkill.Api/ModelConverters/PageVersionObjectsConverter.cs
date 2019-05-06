using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ModelConverters
{
	public interface IPageVersionObjectsConverter
	{
		PageVersionResponse ConvertToPageVersionResponse(PageVersion pageVersion);

		PageVersion ConvertToPageVersion(PageVersionRequest request);
	}

	public class PageVersionObjectsConverter : IPageVersionObjectsConverter
	{
		public PageVersionResponse ConvertToPageVersionResponse(PageVersion pageVersion)
		{
			return new PageVersionResponse()
			{
				Id = pageVersion.Id,
				Text = pageVersion.Text,
				DateTime = pageVersion.DateTime,
				Author = pageVersion.Author,
				PageId = pageVersion.PageId
			};
		}

		public PageVersion ConvertToPageVersion(PageVersionRequest request)
		{
			return new PageVersion()
			{
				Id = request.Id,
				Text = request.Text,
				DateTime = request.DateTime,
				Author = request.Author,
				PageId = request.PageId
			};
		}
	}
}
