using VatBe.Models;

namespace VatBe.Vies;

/// <summary>Abstraction over the VIES API — mockable for tests.</summary>
public interface IViesClient
{
    Task<ViesResult> ValidateAsync(VatNumber vatNumber, CancellationToken cancellationToken = default);
    Task<ViesResult> ValidateAsync(string countryCode, string vatNumber, CancellationToken cancellationToken = default);
}
