using System.Threading;
using System.Threading.Tasks;
using iOne.PartnerIntegration.Pti.Models;

namespace iOne.PartnerIntegration.Pti;

public interface IPtiApiClient
{
    /// <summary>
    /// Calls POST bep-pti-api/vehicle/create and returns the structured response.
    /// Throws on HTTP errors or when PTI returns a non-success response.
    /// </summary>
    Task<PtiCreateVehicleResponse> IssueVehicleAsync(
        PtiCreateVehicleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls GET bep-pti-api/vehicle/lookup-info?filter=transId_ss%3A{transId} and returns
    /// the resolved endpoint URL, raw JSON response body, and HTTP status code.
    /// Throws on HTTP errors.
    /// </summary>
    Task<(string Url, string? RawResponseJson, int HttpStatus)> LookupVehicleInfoAsync(
        string transId, CancellationToken cancellationToken = default);
}
