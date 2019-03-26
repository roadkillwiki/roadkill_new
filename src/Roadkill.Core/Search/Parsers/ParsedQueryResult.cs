using System.Collections.Generic;

namespace Roadkill.Core.Search.Parsers
{
	public class ParsedQueryResult
	{
		public string OriginalText { get; set; }

		public string TextWithoutFields { get; set; }

		public IEnumerable<FieldDefinition> Fields { get; set; }
	}
}
