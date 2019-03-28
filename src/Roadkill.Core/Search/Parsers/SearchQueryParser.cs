using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;

namespace Roadkill.Core.Search.Parsers
{
	public interface ISearchQueryParser
	{
		ParsedQueryResult ParseQuery(string queryText);
	}

	public class SearchQueryParser : ISearchQueryParser
	{
		// Gold and ANTLR parsers to integrate in future:
		// https://github.com/yetanotherchris/spruce/blob/master/Spruce.Core/Search/Grammar/spruce.grm
		// https://github.com/lucastorri/query-parser/blob/master/antlr4/Query.g4
		private static readonly Regex _parserRegex = new Regex(@"(?<name>\w+?):(?<value>""[^""]+""|[\w]+)", RegexOptions.Compiled);

		public ParsedQueryResult ParseQuery(string queryText)
		{
			var searchQuery = new ParsedQueryResult
			{
				OriginalText = queryText,
				TextWithoutFields = queryText,
				Fields = Enumerable.Empty<FieldDefinition>()
			};

			if (_parserRegex.IsMatch(queryText))
			{
				MatchCollection matches = _parserRegex.Matches(queryText);

				var definitions = new List<FieldDefinition>();
				foreach (Match match in matches)
				{
					definitions.Add(new FieldDefinition()
					{
						Name = match.Groups["name"]?.Value,
						Value = match.Groups["value"]?.Value,
					});
				}

				searchQuery.Fields = definitions;

				foreach (var definition in definitions)
				{
					string pair = $"{definition.Name}:{definition.Value}";

					// Remove the field/value definition and surrounding spaces
					searchQuery.TextWithoutFields = Regex.Replace(searchQuery.TextWithoutFields, $@"(\s{{2,}})*{pair}(\s{{2,}})*", "");
				}

				// Remove excess spacing, including multiple spaces into a single space
				searchQuery.TextWithoutFields = Regex.Replace(searchQuery.TextWithoutFields, @"\s{2,}", " ");
				searchQuery.TextWithoutFields = searchQuery.TextWithoutFields.Trim();
			}

			return searchQuery;
		}
	}
}
