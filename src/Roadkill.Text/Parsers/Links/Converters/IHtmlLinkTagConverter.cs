namespace Roadkill.Text.Parsers.Links.Converters
{
	public interface IHtmlLinkTagConverter
	{
		bool IsMatch(HtmlLinkTag htmlLinkTag);

		HtmlLinkTag Convert(HtmlLinkTag htmlLinkTag);
	}
}
