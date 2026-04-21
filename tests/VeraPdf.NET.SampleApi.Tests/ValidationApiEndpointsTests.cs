using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VeraPdf.NET.SampleApi.Tests;

public class ValidationApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ValidationApiEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(_ => { });
    }

    [Fact]
    public async Task SubmitJob_Should_Return_Accepted_And_Trackable_JobId()
    {
        using var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(BuildInvalidPdfBytes());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "pdf", "invalid.pdf");
        content.Add(new StringContent("true"), "pdfA");
        content.Add(new StringContent("false"), "pdfUa");
        content.Add(new StringContent("false"), "wcag22");

        var response = await client.PostAsync("/api/validation/jobs", content);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JobSubmissionResponse>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.JobId));

        var result = await WaitForResultAsync(client, payload.JobId);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetJobStatus_Should_Return_NotFound_For_Unknown_Job()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/validation/jobs/{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
