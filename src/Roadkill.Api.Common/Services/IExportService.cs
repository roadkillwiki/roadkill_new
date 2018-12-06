using System.Threading.Tasks;

namespace Roadkill.Api.Common.Services
{
	public interface IExportService
	{
		Task<string> ExportPagesToXml();

		Task<string> ExportPagesVersionsToXml();

		Task<string> ExportAsSql();

		Task ExportAttachments();
	}
}
