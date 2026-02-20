using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VatBe.Models;

namespace VatBe.Vies;

/// <summary>
/// Client for the EU VIES (VAT Information Exchange System) REST API.
/// API docs: https://ec.europa.eu/taxation_customs/vies/#/vat-validation
/// </summary>
public sealed class ViesClient(HttpClient httpClient) : IViesClient, IDisposable
{
    private const string BaseUrl = "https://ec.europa.eu/taxation_customs/vies/rest-api";

    /// <summary>
    /// Validate a Belgian VAT number against the EU VIES service.
    /// </summary>
    public Task<ViesResult> ValidateAsync(
        VatNumber vatNumber,
        CancellationToken cancellationToken = default) =>
        ValidateAsync(vatNumber.CountryCode, vatNumber.EnterpriseNumber.Digits, cancellationToken);

    /// <summary>
    /// Validate any EU VAT number against the EU VIES service.
    /// </summary>
    public async Task<ViesResult> ValidateAsync(
        string countryCode,
        string vatNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(vatNumber);

        var url = $"{BaseUrl}/ms/{countryCode.ToUpperInvariant()}/vat/{vatNumber}";

        try
        {
            var httpResponse = await httpClient.GetAsync(url, cancellationToken);

            ViesApiResponse? response = null;
            if (httpResponse.IsSuccessStatusCode)
            {
                response = await httpResponse.Content.ReadFromJsonAsync<ViesApiResponse>(
                    cancellationToken: cancellationToken);
            }

            if (response is null)
            {
                return new ViesResult
                {
                    IsValid = false,
                    CountryCode = countryCode,
                    VatNumber = vatNumber,
                    ValidatedAt = DateTimeOffset.UtcNow,
                    Error = httpResponse.IsSuccessStatusCode
                        ? "Empty response from VIES"
                        : $"VIES service error: HTTP {(int)httpResponse.StatusCode}",
                };
            }

            // Map userError from VIES (e.g. MS_UNAVAILABLE, INVALID_INPUT, SERVICE_UNAVAILABLE)
            var userError = NullIfUnknown(response.UserError);

            return new ViesResult
            {
                IsValid = response.IsValid,
                CountryCode = response.CountryCode ?? countryCode,
                VatNumber = response.VatNumber ?? vatNumber,
                TraderName = NullIfUnknown(response.Name),
                TraderAddress = NullIfUnknown(response.Address),
                ValidatedAt = DateTimeOffset.UtcNow,
                RequestIdentifier = response.RequestIdentifier,
                Error = userError,
            };
        }
        catch (HttpRequestException ex)
        {
            return new ViesResult
            {
                IsValid = false,
                CountryCode = countryCode,
                VatNumber = vatNumber,
                ValidatedAt = DateTimeOffset.UtcNow,
                Error = $"HTTP error: {ex.Message}",
            };
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            return new ViesResult
            {
                IsValid = false,
                CountryCode = countryCode,
                VatNumber = vatNumber,
                ValidatedAt = DateTimeOffset.UtcNow,
                Error = "Request timed out",
            };
        }
    }

    public void Dispose() => httpClient.Dispose();

    private static string? NullIfUnknown(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "---" ? null : value;

    // Internal DTO matching VIES REST API response
    private sealed class ViesApiResponse
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("requestDate")]
        public string? RequestDate { get; set; }

        /// <summary>VIES user-level error code (e.g. MS_UNAVAILABLE, INVALID_INPUT, SERVICE_UNAVAILABLE).</summary>
        [JsonPropertyName("userError")]
        public string? UserError { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("requestIdentifier")]
        public string? RequestIdentifier { get; set; }

        [JsonPropertyName("vatNumber")]
        public string? VatNumber { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }
}
