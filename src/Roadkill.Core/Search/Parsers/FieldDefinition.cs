namespace Roadkill.Core.Search.Parsers
{
	public class FieldDefinition
	{
		public string Name { get; set; }

		public string Value { get; set; }

		public override string ToString()
		{
			return $"'{Name}'='{Value}'";
		}
	}
}
