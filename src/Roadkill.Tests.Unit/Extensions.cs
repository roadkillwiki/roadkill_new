using System;
using System.Reflection;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit
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

		public static void ShouldHaveAttribute<T>(this T actual, string methodName, Type attributeType)
			where T : class
		{
			MethodInfo methodType = actual.GetType().GetMethod(methodName);

			var customAttributes = methodType.GetCustomAttributes(attributeType, false);

			Assert.NotEmpty(customAttributes);
		}
	}
}
