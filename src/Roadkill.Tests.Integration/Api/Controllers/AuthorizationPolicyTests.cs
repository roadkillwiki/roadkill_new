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
	public class AuthorizationPolicyTests : IClassFixture<IntegrationTestsWebFactory>
	{
		private const string _authenticatePath = "/v3/Authorization/Authenticate";

		private readonly HttpClient _httpClient;
		private readonly IntegrationTestsWebFactory _factory;
		private readonly ITestOutputHelper _outputHelper;

		public AuthorizationPolicyTests(IntegrationTestsWebFactory factory, ITestOutputHelper outputHelper)
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
		public async Task should_deny_editor_for_policy_she_doesnt_have()
		{
			// given
			string createEditorPath = "/v3/Users/CreateEditor";
			var userData = new UserRequest
			{
				Email = $"{Guid.NewGuid()}@localhost",
				Password = "Passw0rd12345"
			};

			await Authenticate(_factory.EditorUser.Email, _factory.EditorUserPassword);

			// when
			HttpResponseMessage responseMessage = await Post(createEditorPath, userData);

			// then
			responseMessage.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
		}

		private async Task Authenticate(string email, string password)
		{
			var loginData = new
			{
				email = email,
				password = password
			};

			var jwtResponse = await PostReturnsStatusCodeWithContent(_authenticatePath, loginData, HttpStatusCode.OK);
			string json = await jwtResponse.Content.ReadAsStringAsync();
			var response = JsonConvert.DeserializeObject<AuthorizationResponse>(json);
			_httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {response.JwtToken}");
		}

		private async Task<HttpResponseMessage> GetReturnsStatusCode(string requestUri, HttpStatusCode statusCode)
		{
			var response = await _httpClient.GetAsync(requestUri);
			response.StatusCode.ShouldBe(statusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			var content = await response.Content.ReadAsStringAsync();

			content.ShouldNotBeNull();
			return response;
		}

		private async Task<HttpResponseMessage> Post<T>(string requestUri, T data)
		{
			string json = JsonConvert.SerializeObject(data);
			var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

			return await _httpClient.PostAsync(requestUri, stringContent);
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
	}
}
