using System.Threading.Tasks;

namespace Roadkill.Api.Client
{
	public interface IMarkdownClient
	{
		Task<string> ConvertToHtml(string markDown);

		Task UpdateLinksToPage(string oldTitle, string newTitle);
	}
}
