using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Api.Controllers
{
    public class AuthorizationControllerTests : IClassFixture<IntegrationTestsWebFactory>
    {
        private const string _authenticatePath = "/v3/Authorization/Authenticate";
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestsWebFactory _factory;

        public AuthorizationControllerTests(IntegrationTestsWebFactory factory, ITestOutputHelper outputHelper)
        {
	        _factory = factory;
	        _factory.TestOutputHelper = outputHelper;

	        _httpClient = _factory.CreateClient();
	        _httpClient.DefaultRequestHeaders
		        .Accept
		        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task should_retrieve_HTTP_OK_with_admin_user()
        {
	        var data = new
	        {
		        email = _factory.AdminUser.Email,
		        password = _factory.AdminUserPassword
	        };

            await PostReturnsStatusCodeWithContent(_authenticatePath, data, HttpStatusCode.OK);
        }

        private async Task<HttpResponseMessage> PostReturnsStatusCodeWithContent<T>(string requestUri, T data, HttpStatusCode statusCode)
        {
	        string json = JsonConvert.SerializeObject(data);
	        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

	        using (var response = await _httpClient.PostAsync(requestUri, stringContent))
            {
                response.StatusCode.ShouldBe(statusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                var content = await response.Content.ReadAsStringAsync();

                content.ShouldNotBeNull();
                return response;
            }
        }
    }
}
