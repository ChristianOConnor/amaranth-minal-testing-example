using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using AngleSharp.Html.Dom;
using Xunit;
using TestRunProject.Data;
using Microsoft.AspNetCore.Identity;

namespace TestRunProject.Tests
{
    public class AdminControllerTests : 
        IClassFixture<CustomWebApplicationFactory<TestRunProject.Startup>>
    {
        private readonly CustomWebApplicationFactory<TestRunProject.Startup> 
            _factory;

        public AdminControllerTests(
            CustomWebApplicationFactory<TestRunProject.Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_FirsttoRegisterClaimsAdmin()
        {
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = true
                });
            var initResponse = await client.GetAsync("/Identity/Account/Register");
            var antiForgeryValues = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);

            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Register");
            postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "Email", "test@example.com" },
                { "Password", "pas3w0!rRd" },
                { "ConfirmPassword", "pas3w0!rRd" },
            };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();

            var postRequest2 = new HttpRequestMessage(HttpMethod.Post, "/Home/SetAdminToUser");
            postRequest2.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel2 = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "check1", "True" },
                { "check2", "True" },
                { "check3", "True" },
            };
            postRequest2.Content = new FormUrlEncodedContent(formModel2);
            var response2 = await client.SendAsync(postRequest2);
            response2.EnsureSuccessStatusCode();

            Assert.Equal("http://localhost/Home/ClaimAdmin", response.RequestMessage.RequestUri.ToString());
            Assert.Equal("http://localhost/Admin/MasterWalletList", response2.RequestMessage.RequestUri.ToString());

            IdentityUserRole<string>? userRl;
            IdentityUser? user;
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                user = db.Users.FirstOrDefault();
                userRl = db.UserRoles.FirstOrDefault();
            }

            Assert.NotNull(user);
            Assert.True(userRl?.UserId == user?.Id);
        }

        [Fact]
        public async Task CreateMasterWalletTest()
        {
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = true
                });

            var initResponse = await client.GetAsync("/Admin/MasterWalletList");
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Admin/CreateMasterWallet");
            var antiForgeryValues = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);
            postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "label", "testWallet1t" },
                { "isTestNet", "True" }
            };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            IdentityUserRole<string>? userRl;
            MasterWallet? wallet1;
            IdentityUser user;
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                wallet1 = db.MasterWallets.FirstOrDefault();
                userRl = db.UserRoles.FirstOrDefault();
                user = db.Users.FirstOrDefault();
            }

            Assert.Equal(user.Email, "test@example.com");
            //Assert.Equal(userRl?.UserId, user?.Id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(wallet1);
            Assert.Equal(wallet1?.Address.Substring(0, 3), "tb1");
        }

        public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Test");

                var result = AuthenticateResult.Success(ticket);

                return Task.FromResult(result);
            }
        }
    }
}