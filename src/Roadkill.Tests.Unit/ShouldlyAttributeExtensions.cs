using System;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roadkill.Api.JWT;
using Serilog.Parsing;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit
{
	[ShouldlyMethods]
	public static class ShouldlyAttributeExtensions
	{
		public static void ShouldHaveSamePropertyValuesAs<TFrom, TTo>(this TFrom fromInstance, TTo toInstance)
			where TFrom : class
			where TTo : class
		{
			Type fromType = fromInstance.GetType();
			Type toType = toInstance.GetType();

			var fromProperties = fromType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo fromProperty in fromProperties)
			{
				PropertyInfo toProperty = toType.GetProperty(fromProperty.Name);

				if (toProperty != null)
				{
					object fromValue = fromProperty.GetValue(fromInstance);
					object toValue = toProperty.GetValue(toInstance);

					string errorMessage = $"{fromType.Name}.{fromProperty.Name} value '{fromValue}" +
										  " does not match " +
										  $"{toType.Name}.{toProperty.Name} value '{toValue}";

					fromValue.ShouldBe(toValue, errorMessage);
				}
			}
		}

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
