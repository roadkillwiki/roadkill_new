using System;
using System.Collections.Generic;
using System.Linq;

namespace Roadkill.Core.Search.Parsers
{
	public class ParsedQueryResult
	{
		public string OriginalText { get; set; }

		public string TextWithoutFields { get; set; }

		public IEnumerable<FieldDefinition> Fields { get; set; }

		public string GetFieldValue(string name)
		{
			return Fields.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
		}
	}
}
