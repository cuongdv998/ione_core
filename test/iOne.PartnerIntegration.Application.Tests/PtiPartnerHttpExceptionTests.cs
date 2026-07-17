using System;
using iOne.PartnerIntegration.Pti;
using Xunit;

namespace iOne.PartnerIntegration.Tests;

public class PtiPartnerHttpExceptionTests
{
    [Fact]
    public void Constructor_Preserves_RawResponseBody_HttpStatus_And_RequestUrl()
    {
        const string body =
            """
            {
              "statusCode" : 400,
              "message" : "Thông tin phí không hợp lệ",
              "code" : "001"
            }
            """;

        var ex = new PtiPartnerHttpException(
            "Policy:Partner:PtiIssueFailed",
            400,
            body,
            friendlyMessage: "001 - Thông tin phí không hợp lệ",
            requestUrl: "https://open-gate-uat.ipas.com.vn/bep-pti-api/vehicle/create");

        Assert.Equal(400, ex.HttpStatusCode);
        Assert.Equal(body, ex.RawResponseBody);
        Assert.Equal("https://open-gate-uat.ipas.com.vn/bep-pti-api/vehicle/create", ex.RequestUrl);
        Assert.Contains("Thông tin phí không hợp lệ", ex.Message, StringComparison.Ordinal);
        Assert.Equal("Policy:Partner:PtiIssueFailed", ex.Code);
    }
}
