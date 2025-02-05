using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Duende.Bff.Tests.Headers
{
    public class ApiAndBffUseForwardedHeaders : BffIntegrationTestBase
    {
        public ApiAndBffUseForwardedHeaders(ITestOutputHelper output) : base(output)
        {
            ApiHost = new ApiHost(output.WriteLine, IdentityServerHost, "scope1", useForwardedHeaders: true);
            ApiHost.InitializeAsync().Wait();

            BffHost = new BffHost(output.WriteLine, IdentityServerHost, ApiHost, "spa", useForwardedHeaders: true);
            BffHost.InitializeAsync().Wait();
        }
        
        [Fact]
        public async Task bff_host_name_should_propagate_to_api()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            var host = apiResult.RequestHeaders["Host"].Single();
            host.ShouldBe("app");
        }
        
        [Fact]
        public async Task forwarded_host_name_without_header_forwarding_propagate_to_api()
        {
            await BffHost.InitializeAsync();
            
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("X-Forwarded-Host", "external");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            var host = apiResult.RequestHeaders["Host"].Single();
            host.ShouldBe("external");
        }
        
        [Fact]
        public async Task forwarded_host_name_with_header_forwarding_should_propagate_to_api()
        {
            await BffHost.InitializeAsync();
            
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("X-Forwarded-Host", "external");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.ShouldBeTrue();
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            var host = apiResult.RequestHeaders["Host"].Single();
            host.ShouldBe("external");
        }
    }
}