using System.Threading.Tasks;

namespace Roadkill.Api.Common.Services
{
	public interface IMarkdownService
	{
		Task<string> ConvertToHtml(string markDown);

		Task UpdateLinksToPage(string oldTitle, string newTitle);
	}
}