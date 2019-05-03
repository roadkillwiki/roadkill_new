using System;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roadkill.Api.JWT;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit
{
	[ShouldlyMethods]
	public static class ShouldlyAttributeExtensions
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

			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for {methodName}");
		}

		public static void ShouldHaveRouteAttributeWithTemplate<T>(this T actual, string methodName, string routeTemplate)
			where T : class
		{
			Type attributeType = typeof(RouteAttribute);
			MethodInfo methodType = actual.GetType().GetMethod(methodName);

			var customAttributes = methodType.GetCustomAttributes(attributeType, false);
			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for {methodName}");

			RouteAttribute routeAttribute = customAttributes[0] as RouteAttribute;
			routeAttribute.Template.ShouldBe(routeTemplate);
		}

		public static void ShouldAllowAnonymous<T>(this T actual, string methodName)
			where T : class
		{
			Type attributeType = typeof(AllowAnonymousAttribute);
			actual.ShouldHaveAttribute(methodName, attributeType);
		}

		public static void ShouldAuthorizeEditors<T>(this T actual, string methodName)
			where T : class
		{
			Type attributeType = typeof(AuthorizeAttribute);
			MethodInfo methodType = actual.GetType().GetMethod(methodName);

			var customAttributes = methodType.GetCustomAttributes(attributeType, false);
			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for {methodName}");

			AuthorizeAttribute authorizeAttribute = customAttributes[0] as AuthorizeAttribute;
			authorizeAttribute.Policy.ShouldContain(PolicyNames.Editor);
		}

		public static void ShouldAuthorizeAdmins<T>(this T actual, string methodName)
			where T : class
		{
			Type attributeType = typeof(AuthorizeAttribute);
			MethodInfo methodType = actual.GetType().GetMethod(methodName);

			var customAttributes = methodType.GetCustomAttributes(attributeType, false);
			customAttributes.Length.ShouldBeGreaterThan(0, $"No {attributeType.Name} found for {methodName}");

			AuthorizeAttribute authorizeAttribute = customAttributes[0] as AuthorizeAttribute;
			authorizeAttribute.Policy.ShouldContain(PolicyNames.Admin);
		}
	}
}
