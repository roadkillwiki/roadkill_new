using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Integration
{
	[ShouldlyMethods]
	public static class ShouldlyExtensions
	{
		public static void ShouldBeEquivalent<T>(this T actual, T expected)
		{
			string expectedJson = JsonConvert.SerializeObject(expected);
			string actualJson = JsonConvert.SerializeObject(actual);

			Assert.Equal(expectedJson, actualJson);
		}
	}
}
