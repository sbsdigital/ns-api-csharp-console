using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Netstream.Nws.Client.Daemon;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json").Build();
var config = configuration.GetRequiredSection("AzureAd").Get<AzureConfig>();
var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
    .WithClientSecret(config.ClientSecret)
    .WithAuthority(new Uri(config.Authority))
    .Build();
var resourceId = "api://dedb5554-7c2d-4cc1-9fb4-514315a4387c";
var scopes = new[] { $"{resourceId}/.default" };

AuthenticationResult? result;
try
{
    result = await app.AcquireTokenForClient(scopes)
        .ExecuteAsync();
}
catch (MsalUiRequiredException ex)
{
    // The application doesn't have sufficient permissions.
    // - Did you declare enough app permissions during app creation?
    // - Did the tenant admin grant permissions to the application?
    return;
}
catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
{
    // Invalid scope. The scope has to be in the form "https://resourceurl/.default"
    // Mitigation: Change the scope to be as expected.
    return;
}

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
var apiUri = "https://sit.sbs.digital/api/cim17/9/MeterReadings2/get";
// Call the web API.
var payload =
    @"{""reading"":[{""reason"":""billing"",""reasonSpecified"":false,""timePeriod"":{""end"":""2022-06-02T00:00:00"",""endSpecified"":true,""start"":""2022-05-26T00:00:00"",""startSpecified"":true}}],""readingQuality"":[],""readingType"":[{""mRID"":""0.0.5.4.1.1.12.0.0.0.0.0.0.0.0.3.72.0""}],""usagePoint"":[{""mRID"":""43d01f47-12fe-4bf1-baaa-11780d4e2bec""}]}";
HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
HttpResponseMessage response = await httpClient.PostAsync(apiUri, content);
Console.Out.WriteLine("result is " + response);
if (response.IsSuccessStatusCode)
{
    var readings = await response.Content.ReadAsStringAsync();
    Console.Out.WriteLine($"readings: {readings}");
}