using System.Threading.Tasks;

namespace Roadkill.Api.Client
{
	public interface IExportClient
	{
		Task<string> ExportPagesToXml();

		Task<string> ExportPagesVersionsToXml();

		Task<string> ExportAsSql();

		Task ExportAttachments();
	}
}
