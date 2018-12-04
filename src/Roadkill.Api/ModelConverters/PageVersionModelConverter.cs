using Roadkill.Api.Common.Models;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ModelConverters
{
	public interface IPageVersionModelConverter
	{
		PageVersionModel ConvertToViewModel(PageVersion pageVersion);

		PageVersion ConvertToPageVersion(PageVersionModel model);
	}

	public class PageVersionModelConverter : IPageVersionModelConverter
	{
		public PageVersionModel ConvertToViewModel(PageVersion pageVersion)
		{
			return new PageVersionModel()
			{
				Id = pageVersion.Id,
				Text = pageVersion.Text,
				DateTime = pageVersion.DateTime,
				Author = pageVersion.Author,
				PageId = pageVersion.PageId
			};
		}

		public PageVersion ConvertToPageVersion(PageVersionModel model)
		{
			return new PageVersion()
			{
				Id = model.Id,
				Text = model.Text,
				DateTime = model.DateTime,
				Author = model.Author,
				PageId = model.PageId
			};
		}
	}
}
