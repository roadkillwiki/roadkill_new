using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Api.Controllers
{
	public class MartenAspIdentityLoadTests : IClassFixture<IntegrationTestsWebFactory>
	{
		private const string _authenticatePath = "/v3/Authorization/Authenticate";
		private const string _addEditorUserPath = "/v3/Users/CreateEditor";
		private const string _addPagePath = "/v3/Pages/";
		private const string _getPagePath = "/v3/Pages/{0}";

		private readonly HttpClient _httpClient;
		private readonly IntegrationTestsWebFactory _factory;
		private readonly ITestOutputHelper _outputHelper;

		public MartenAspIdentityLoadTests(IntegrationTestsWebFactory factory, ITestOutputHelper outputHelper)
		{
			_factory = factory;
			_outputHelper = outputHelper;
			_factory.TestOutputHelper = outputHelper;

			_httpClient = _factory.CreateClient();
			_httpClient.DefaultRequestHeaders
				.Accept
				.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		[Fact]
		public async Task should_async_add_users_and_post_pages_and_get_pages()
		{
			int failedCount = 0;

			var loginData = new
			{
				email = _factory.AdminUser.Email,
				password = _factory.AdminUserPassword
			};

			var jwtResponse = await PostReturnsStatusCodeWithContent(_authenticatePath, loginData, HttpStatusCode.OK);
			string json = await jwtResponse.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<AuthorizationResponse>(json);
			_httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {response.JwtToken}");

			// Hit marten with 50 requests per second.
			// You may need to lower this number for slower hardware.
			var stopWatch = Stopwatch.StartNew();
			Parallel.For(0, 50, (i, state) =>
			{
				try
				{
					_factory.Logger.LogInformation($"Begin thread {i}.");
					AddEditorAndAddPageCheckFor200(i).ConfigureAwait(false).GetAwaiter().GetResult();
					_factory.Logger.LogInformation($"(Thread {i} success).");
				}
				catch (Exception ex)
				{
					Interlocked.Increment(ref failedCount);
					_factory.Logger.LogInformation($"Failed on thread {i}. {ex.Message}");
					state.Break();
				}
			});

			if (failedCount > 0)
			{
				Fail($"Failed on {failedCount} threads");
			}

			stopWatch.Stop();
			_outputHelper.WriteLine($"Took {stopWatch.ElapsedMilliseconds}ms to run.");
		}

		private async Task AddEditorAndAddPageCheckFor200(int uniqueId)
		{
			// Create an Editor user
			var userData = new UserRequest
			{
				Email = $"{uniqueId}@localhost",
				Password = "Passw0rd12345"
			};

			var t = await PostReturnsStatusCodeWithContent(
				_addEditorUserPath,
				userData,
				HttpStatusCode.Created);
			_factory.Logger.LogInformation($"Created {userData.Email}.");

			// Create a page in the API
			var pageRequest = new PageRequest
			{
				Title = $"page {uniqueId}",
				CreatedBy = userData.Email,
				Tags = "testing"
			};

			var response = await PostReturnsStatusCodeWithContent(
				_addPagePath,
				pageRequest,
				HttpStatusCode.Created);

			// Get the PageResponse from the API
			string json = await response.Content.ReadAsStringAsync();
			var model = JsonConvert.DeserializeObject<PageResponse>(json);

			string newPageApiUrl = string.Format(_getPagePath, model.Id);
			response = await GetReturnsStatusCode(newPageApiUrl, HttpStatusCode.OK);
			json = await response.Content.ReadAsStringAsync();
			_factory.Logger.LogInformation($"Created page {model.Id}.");

			// Check it returned a page model
			json.ShouldNotBeNullOrEmpty($"Failed on iteration {uniqueId}");
		}

		private async Task<HttpResponseMessage> GetReturnsStatusCode(string requestUri, HttpStatusCode statusCode)
		{
			var response = await _httpClient.GetAsync(requestUri);
			response.StatusCode.ShouldBe(statusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldNotBeNull();
			return response;
		}

		private async Task<HttpResponseMessage> PostReturnsStatusCodeWithContent<T>(string requestUri, T data, HttpStatusCode statusCode)
		{
			string json = JsonConvert.SerializeObject(data);
			var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(requestUri, stringContent);
			response.StatusCode.ShouldBe(statusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldNotBeNull();
			return response;
		}

		private void Fail(string message) => throw new Xunit.Sdk.XunitException(message);
	}
}
