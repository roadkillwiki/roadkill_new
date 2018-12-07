using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace Roadkill.Tests.Unit
{
	public static class AssertExtensions
	{
		public static void Equivalent(object expected, object actual)
		{
			string expectedJson = JsonConvert.SerializeObject(expected);
			string actualJson = JsonConvert.SerializeObject(actual);

			Assert.Equal(expectedJson, actualJson);
		}

		public static void ContainsItem<T>(IEnumerable<T> items, object itemToFind)
		{
			string jsonToCompare = JsonConvert.SerializeObject(itemToFind);

			items.ToList().ForEach(x =>
			{
				string itemJson = JsonConvert.SerializeObject(x);

				if (itemJson == jsonToCompare)
				{
					Assert.True(true);
				}
			});

			Assert.False(true, "Could not find item in the collection");
		}
	}
}
