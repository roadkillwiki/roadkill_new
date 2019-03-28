using System.Linq;
using Roadkill.Core.Search.Parsers;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Core.Search.Parsers
{
	public class SearchQueryParserTests
	{
		[Fact]
		public void should_parse_free_text()
		{
			// given
			string text = "ducks yellow green blue";
			SearchQueryParser searchQueryParser = CreateParser();

			// when
			ParsedQueryResult queryResult = searchQueryParser.ParseQuery(text);

			// then
			queryResult.OriginalText.ShouldBe(text);
			queryResult.TextWithoutFields.ShouldBe(text);
			queryResult.Fields.Count().ShouldBe(0);
		}

		[Fact]
		public void should_parse_multiple_fields()
		{
			// given
			string text = "author:donald date:now title:headache some text";
			SearchQueryParser searchQueryParser = CreateParser();

			// when
			ParsedQueryResult queryResult = searchQueryParser.ParseQuery(text);

			// then
			queryResult.OriginalText.ShouldBe(text);
			queryResult.TextWithoutFields.ShouldBe("some text");
			queryResult.Fields.Count().ShouldBe(3);

			var tagsField = queryResult.Fields.First(x => x.Name == "title");
			tagsField.ShouldNotBeNull();
			tagsField.Value.ShouldBe("headache");
		}

		[Fact]
		public void should_parse_fields_with_quotes()
		{
			// given
			string text = "some text tags:\"make,brexit,great,again\"";
			SearchQueryParser searchQueryParser = CreateParser();

			// when
			ParsedQueryResult queryResult = searchQueryParser.ParseQuery(text);

			// then
			queryResult.OriginalText.ShouldBe(text);
			queryResult.TextWithoutFields.ShouldBe("some text");
			queryResult.Fields.Count().ShouldBe(1);

			var tagsField = queryResult.Fields.First(x => x.Name == "tags");
			tagsField.ShouldNotBeNull();
			tagsField.Value.ShouldBe("\"make,brexit,great,again\"");
		}

		[Fact]
		public void should_parse_free_text_and_fields()
		{
			// given
			string text = "ducks yellow author:chorizo green date:2018 blue";
			SearchQueryParser searchQueryParser = CreateParser();

			// when
			ParsedQueryResult queryResult = searchQueryParser.ParseQuery(text);

			// then
			queryResult.OriginalText.ShouldBe(text);
			queryResult.TextWithoutFields.ShouldBe("ducks yellow green blue");
			queryResult.Fields.Count().ShouldBe(2);
		}

		private SearchQueryParser CreateParser()
		{
			return new SearchQueryParser();
		}
	}
}
