using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Roadkill.Tests.Unit.Api
{
	public static class ActionResultExtensions
	{
		public static void ShouldBeOkObjectResult<T>(this ActionResult<T> actionResult)
		{
			var okResult = actionResult.Result as OkObjectResult;
			okResult.ShouldNotBeNull($"No OkObjectResult returned for {typeof(T).Name}");
		}

		public static T GetOkObjectResultValue<T>(this ActionResult<T> actionResult)
		{
			var okResult = (OkObjectResult)actionResult.Result;
			if (okResult.Value is Task<T> taskValue)
			{
				return (T)taskValue.GetAwaiter().GetResult();
			}

			return (T)okResult.Value;
		}

		public static void ShouldBeCreatedAtActionResult<T>(this ActionResult<T> actionResult)
		{
			var createdAtRouteResult = actionResult.Result as CreatedAtActionResult;
			createdAtRouteResult.ShouldNotBeNull($"No CreatedAtActionResult returned for {typeof(T).Name}");
		}

		public static T CreatedAtActionResultValue<T>(this ActionResult<T> actionResult)
		{
			var createdAtActionResult = (CreatedAtActionResult)actionResult.Result;

			if (createdAtActionResult.Value is Task<T> taskValue)
			{
				return (T)taskValue.GetAwaiter().GetResult();
			}

			return (T)createdAtActionResult.Value;
		}

		public static void ShouldBeNotFoundObjectResult<T>(this ActionResult<T> actionResult)
		{
			var createdAtRouteResult = actionResult.Result as NotFoundObjectResult;
			createdAtRouteResult.ShouldNotBeNull($"No NotFoundObjectResult returned for {typeof(T).Name}");
		}

		/// <summary>
		/// When a controller method is a DELETE, PUT, UPDATE, CREATE and the object
		/// can't be updated this will return the error message that the NotFound result contained.
		/// </summary>
		/// <param name="actionResult">The ActionResult from the controller.</param>
		/// <typeparam name="T">The type of returned when there's NoContent (an errormessage)</typeparam>
		/// <returns>The value when there's NotFoundObjectResult (an errormessage)</returns>
		public static T GetNotFoundValue<T>(this ActionResult<T> actionResult)
		{
			var notFoundResult = (NotFoundObjectResult)actionResult.Result;

			if (notFoundResult.Value is Task<T> taskValue)
			{
				return (T)taskValue.GetAwaiter().GetResult();
			}

			return (T)notFoundResult.Value;
		}

		/// <summary>
		/// When a controller method returns a type, for example a Get action, use this
		/// method to get the alternative path where the type being looked for isn't found.
		/// </summary>
		/// <param name="actionResult">The ActionResult</param>
		/// <typeparam name="TActionResult">The type returned by the ActionResult</typeparam>
		/// <typeparam name="T">The type of returned when there's NoContent (an errormessage)</typeparam>
		/// <returns>The value when there's NoContent (an errormessage)</returns>
		public static T GetNotFoundValue<TActionResult, T>(this ActionResult<TActionResult> actionResult)
		{
			var notFoundResult = actionResult.Result as NotFoundObjectResult;
			notFoundResult.ShouldNotBeNull($"No NotFoundObjectResult returned for {typeof(T).Name}");

			if (notFoundResult.Value is Task<T> taskValue)
			{
				return (T)taskValue.GetAwaiter().GetResult();
			}

			return (T)notFoundResult.Value;
		}

		public static void ShouldBeBadRequestObjectResult<T>(this ActionResult<T> actionResult)
		{
			var createdAtRouteResult = actionResult.Result as BadRequestObjectResult;
			createdAtRouteResult.ShouldNotBeNull($"No BadRequestObjectResult returned for {typeof(T).Name}");
		}

		public static T GetBadRequestValue<T>(this ActionResult<T> actionResult)
		{
			var badRequestResult = (BadRequestObjectResult)actionResult.Result;

			if (badRequestResult.Value is Task<T> taskValue)
			{
				return (T)taskValue.GetAwaiter().GetResult();
			}

			return (T)badRequestResult.Value;
		}

		public static NoContentResult ShouldBeNoContentResult<T>(this ActionResult<T> actionResult)
		{
			var noContentResult = actionResult.Result as NoContentResult;
			noContentResult.ShouldNotBeNull($"No NoContentResult returned for {typeof(T).Name}");

			return noContentResult;
		}
	}
}
