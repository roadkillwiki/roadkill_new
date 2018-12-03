using System.Threading.Tasks;

namespace Roadkill.Api.Interfaces
{
	public interface IMarkdownService
	{
		Task<string> ConvertToHtml(string markDown);

		Task UpdateLinksToPage(string oldTitle, string newTitle);
	}
}