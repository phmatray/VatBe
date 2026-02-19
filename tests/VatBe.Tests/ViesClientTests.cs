using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Moq;
using VatBe.Models;
using VatBe.Vies;
using Xunit;

namespace VatBe.Tests;

/// <summary>
/// Tests ViesClient using a fake HTTP message handler (no real HTTP calls).
/// </summary>
public sealed class ViesClientTests
{
    private static ViesClient CreateClient(HttpResponseMessage response)
    {
        var handler = new FakeHttpHandler(response);
        var httpClient = new HttpClient(handler);
        return new ViesClient(httpClient);
    }

    [Fact]
    public async Task ValidateAsync_VatNumber_ValidResponse_ReturnsValid()
    {
        const string json = """
            {
                "isValid": true,
                "countryCode": "BE",
                "vatNumber": "0402206045",
                "name": "Delhaize Group",
                "address": "Square Marie Curie 40, 1070 Brussels",
                "requestIdentifier": "REQ-001"
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        using var client = CreateClient(response);
        var vat = VatNumber.Parse("BE0402206045");

        var result = await client.ValidateAsync(vat);

        result.IsValid.Should().BeTrue();
        result.CountryCode.Should().Be("BE");
        result.VatNumber.Should().Be("0402206045");
        result.TraderName.Should().Be("Delhaize Group");
        result.TraderAddress.Should().Contain("Brussels");
        result.RequestIdentifier.Should().Be("REQ-001");
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_InvalidVatNumber_ReturnsInvalid()
    {
        const string json = """
            {
                "isValid": false,
                "countryCode": "BE",
                "vatNumber": "9999999999",
                "name": "---",
                "address": "---"
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        using var client = CreateClient(response);

        var result = await client.ValidateAsync("BE", "9999999999");

        result.IsValid.Should().BeFalse();
        result.TraderName.Should().BeNull();      // "---" → null
        result.TraderAddress.Should().BeNull();   // "---" → null
    }

    [Fact]
    public async Task ValidateAsync_NetworkError_ReturnsErrorResult()
    {
        var handler = new FakeHttpHandler(null!, throwNetworkError: true);
        var httpClient = new HttpClient(handler);
        using var client = new ViesClient(httpClient);

        var result = await client.ValidateAsync("BE", "0402206045");

        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        result.Error.Should().Contain("HTTP error");
    }

    [Fact]
    public async Task ValidateAsync_ServiceUnavailable_ReturnsErrorResult()
    {
        var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        using var client = CreateClient(response);

        var result = await client.ValidateAsync("BE", "0402206045");

        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_NullsTraderName()
    {
        const string json = """
            {
                "isValid": true,
                "countryCode": "BE",
                "vatNumber": "0402206045",
                "name": "",
                "address": "   "
            }
            """;

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        using var client = CreateClient(response);

        var result = await client.ValidateAsync("BE", "0402206045");

        result.TraderName.Should().BeNull();
        result.TraderAddress.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_ValidatedAt_IsSetToUtcNow()
    {
        const string json = """{ "isValid": true, "countryCode": "BE", "vatNumber": "0402206045" }""";

        var before = DateTimeOffset.UtcNow;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        using var client = CreateClient(response);
        var result = await client.ValidateAsync("BE", "0402206045");
        var after = DateTimeOffset.UtcNow;

        result.ValidatedAt.Should().BeOnOrAfter(before);
        result.ValidatedAt.Should().BeOnOrBefore(after);
    }

    // --- Helpers ---

    private sealed class FakeHttpHandler(HttpResponseMessage response, bool throwNetworkError = false)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (throwNetworkError)
                throw new HttpRequestException("Simulated network error");

            return Task.FromResult(response);
        }
    }
}
