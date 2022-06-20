using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;

namespace TestRunProject.Tests
{
    public static class AntiForgeryTokenExtractor
	{
		public static string ExtractAntiForgeryToken(string htmlBody)
		{
            var requestVerificationTokenMatch =
                Regex.Match(htmlBody, $@"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>");

            if (requestVerificationTokenMatch.Success)
                return requestVerificationTokenMatch.Groups[1].Captures[0].Value;

            throw new ArgumentException($"Anti forgery token '__RequestVerificationToken' not found in HTML", nameof(htmlBody));
        }
	}
}