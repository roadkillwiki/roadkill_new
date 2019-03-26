using System.Collections.Generic;
using System.Linq;
using Sprache;

namespace Roadkill.Core.Search.Parsers
{
	public interface ISearchQueryParser
	{
		ParsedQueryResult ParseQuery(string queryText);
	}

	public class SearchQueryParser : ISearchQueryParser
	{
		private readonly Parser<IEnumerable<FieldDefinition>> _fieldOnlyParser;
		private readonly Parser<IEnumerable<FieldDefinition>> _combinedParser;

		public SearchQueryParser()
		{
			// https://github.com/yetanotherchris/spruce/blob/master/Spruce.Core/Search/Grammar/spruce.grm
			// TODO: find out how to combine these two parsers.
			// Remembering that it's recursive, so "whitespace" will be used
			// after name, colon, value when it recurses
			_fieldOnlyParser =
			(
				from whitespace in Parse.WhiteSpace.Many()
				from name in Parse.CharExcept(new char[] { ':', ' ' }).Many().Text()
				from colon in Parse.Char(':')
				from value in Parse.CharExcept(' ').Many().Text()
				select new FieldDefinition()
				{
					Name = name,
					Value = value
				}).Many().Token();

			_combinedParser =
			(
				from anytext in Parse.CharExcept(' ').Many()
				from whitespace in Parse.Char(' ').AtLeastOnce()
				from name in Parse.CharExcept(new char[] { ':', ' ' }).Many().Text()
				from colon in Parse.Char(':')
				from value in Parse.CharExcept(' ').Many().Text()
				select new FieldDefinition()
				{
					Name = name,
					Value = value
				}).Many().Token();
		}

		public ParsedQueryResult ParseQuery(string queryText)
		{
			var searchQuery = new ParsedQueryResult
			{
				OriginalText = queryText,
				TextWithoutFields = queryText,
				Fields = Enumerable.Empty<FieldDefinition>()
			};

			IResult<IEnumerable<FieldDefinition>> results = _fieldOnlyParser.TryParse(queryText);

			if (!results.WasSuccessful || !results.Value.Any())
			{
				results = _combinedParser.TryParse(queryText);
			}

			if (results.WasSuccessful)
			{
				if (results.Value.Any())
				{
					searchQuery.Fields = results.Value;

					foreach (FieldDefinition definition in results.Value)
					{
						string pair = $"{definition.Name}:{definition.Value}";
						searchQuery.TextWithoutFields = searchQuery.TextWithoutFields.Replace(pair, "");
					}

					searchQuery.TextWithoutFields = searchQuery.TextWithoutFields.Trim();
				}
			}

			return searchQuery;
		}
	}
}
