using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TestRunProject.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task DLoginTest()
        {
            {
                var application = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        // ... Configure test services
                    });

                var client = application.CreateClient();

                // -- REGISTER --

                var registerResponse = await client.GetAsync("/Identity/Account/Register");
                registerResponse.EnsureSuccessStatusCode();
                string registerResponseContent = await registerResponse.Content.ReadAsStringAsync();

                var requestVerificationToken = AntiForgeryTokenExtractor.ExtractAntiForgeryToken(registerResponseContent);
                
                var formModel = new Dictionary<string, string>
                {
                    { "Input.Email", "test2@example.com" },
                    { "Input.Password", "pas3w02!rRd" },
                    { "Input.ConfirmPassword", "pas3w02!rRd" },
                    { "__RequestVerificationToken", requestVerificationToken },
                };

                var postRequest2 = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Register");
                postRequest2.Content = new FormUrlEncodedContent(formModel);
                var registerResponse2 = await client.SendAsync(postRequest2);
                registerResponse2.EnsureSuccessStatusCode();

                // -- LOGIN --

                var loginResponse = await client.GetAsync("/Identity/Account/Login");
                loginResponse.EnsureSuccessStatusCode();
                string loginResponseContent = await registerResponse.Content.ReadAsStringAsync();

                requestVerificationToken = AntiForgeryTokenExtractor.ExtractAntiForgeryToken(loginResponseContent);

                formModel = new Dictionary<string, string>
                {
                    { "Input.Email", "test2@example.com" },
                    { "Input.Password", "pas3w02!rRd" },
                    { "__RequestVerificationToken", requestVerificationToken },
                };

                var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/Identity/Account/Login");
                loginRequest.Content = new FormUrlEncodedContent(formModel);
                var loginResponse2 = await client.SendAsync(loginRequest);
                loginResponse2.EnsureSuccessStatusCode();

                // -- LOGOUT --

                var logoutRequest = new StringContent("");
                logoutRequest.Headers.Add("RequestVerificationToken", requestVerificationToken);

                var logoutResponse = await client.PostAsync("/Identity/Account/Logout", logoutRequest);

                logoutResponse.EnsureSuccessStatusCode();
            }
        }
    }
}