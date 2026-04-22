using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace VeraPdf.NET.SampleApi.Tests;

public class ValidationApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ValidationApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }


    private static async Task<HttpResponseMessage> WaitForResultAsync(HttpClient client, string jobId)
    {
        var timeoutAt = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < timeoutAt)
        {
            var statusResponse = await client.GetAsync($"/api/validation/jobs/{jobId}");
            if (!statusResponse.IsSuccessStatusCode)
            {
                await Task.Delay(250);
                continue;
            }

            var statusPayload = await statusResponse.Content.ReadFromJsonAsync<JobStatusResponse>();
            if (statusPayload is not null && (statusPayload.Status == "Completed" || statusPayload.Status == "Failed"))
            {
                return await client.GetAsync($"/api/validation/jobs/{jobId}/result");
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Validation job did not complete within timeout window.");
    }

    private static byte[] BuildInvalidPdfBytes()
        => "not-a-pdf"u8.ToArray();

    private sealed class JobSubmissionResponse
    {
        public string JobId { get; set; } = string.Empty;
    }

    private sealed class JobStatusResponse
    {
        public string Status { get; set; } = string.Empty;
    }
}
