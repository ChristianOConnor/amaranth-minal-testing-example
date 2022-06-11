using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public async Task DLoginTest()
        {
            var client = _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = true
                });
            var newAntiForg = await client.GetAsync("/Identity/Account/Register");
            var antiForgeryValues = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(newAntiForg);
            var postRequest2 = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Register");
            postRequest2.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "Email", "test2@example.com" },
                { "Password", "pas3w02!rRd" },
                { "ConfirmPassword", "pas3w02!rRd" },
            };

            postRequest2.Content = new FormUrlEncodedContent(formModel);
            var response = await client.SendAsync(postRequest2);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var profResponse = await client.GetAsync("/Profile");
            //var antiForgeryValues2 = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);
            var postRequestLogout = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Logout");
            postRequestLogout.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel2 = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
            };
            postRequestLogout.Content = new FormUrlEncodedContent(formModel2);

            var logoutAnswer = await client.SendAsync(postRequestLogout);
            Console.WriteLine(logoutAnswer.StatusCode);
            logoutAnswer.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, logoutAnswer.StatusCode);

            var postRequest3 = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Login");
            postRequest3.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel3 = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "Input.Email", "test2@example.com" },
                { "Input.Password", "pas3w02!rRd" }
            };
            postRequest3.Content = new FormUrlEncodedContent(formModel3);
            var reLoginResponse = await client.SendAsync(postRequest3);
            reLoginResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, reLoginResponse.StatusCode);
        }
    }
}