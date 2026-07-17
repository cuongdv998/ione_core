using System.Collections.Generic;
using iOne.PartnerIntegration;
using iOne.PartnerIntegration.Pti;
using Xunit;

namespace iOne.PartnerIntegration.Tests;

public class PtiVehicleLookupInfoParserTests
{
    [Fact]
    public void TryExtractLookupInfo_InsurerPolicyNo_From_TaskId_Ignores_LookUpCode()
    {
        const string json = """
            {"content":[{"taskId":999,"contractNumber":"HN001","certificateInfos":[{"lookUpCode":"LC","certificateCode":"GCN-1","url":"https://x/a"}]}]}
            """;

        var list = new List<PtiCertificateInfoSnapshot>();
        PtiVehicleLookupInfoParser.TryExtractLookupInfo(json, out var insurerNo, out var contract, list);

        Assert.Equal("999", insurerNo);
        Assert.Equal("HN001", contract);
        Assert.Single(list);
        Assert.Equal("GCN-1", list[0].CertificateCode);
        Assert.Equal("https://x/a", list[0].Url);
    }

    [Fact]
    public void TryExtractLookupInfo_No_TaskId_Leaves_InsurerPolicyNo_Null_Even_With_LookUpCode_Or_Contract()
    {
        const string json = """
            {"content":[{"contractNumber":"ONLY_CONTRACT","certificateInfos":[{"lookUpCode":"SHOULD_NOT_SET_INSURER","certificateCode":"A"}]}]}
            """;

        var list = new List<PtiCertificateInfoSnapshot>();
        PtiVehicleLookupInfoParser.TryExtractLookupInfo(json, out var insurerNo, out var contract, list);

        Assert.Null(insurerNo);
        Assert.Equal("ONLY_CONTRACT", contract);
        Assert.Single(list);
        Assert.Equal("A", list[0].CertificateCode);
    }

    [Fact]
    public void TryExtractLookupInfo_Parses_String_TaskId()
    {
        const string json = """{"content":[{"taskId":"42","certificateInfos":[]}]}""";

        var list = new List<PtiCertificateInfoSnapshot>();
        PtiVehicleLookupInfoParser.TryExtractLookupInfo(json, out var insurerNo, out _, list);

        Assert.Equal("42", insurerNo);
        Assert.Empty(list);
    }
}
