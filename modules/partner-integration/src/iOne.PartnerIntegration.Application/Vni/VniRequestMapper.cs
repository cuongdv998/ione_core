using System;
using System.Globalization;
using iOne.Policies;
using Volo.Abp.DependencyInjection;

namespace iOne.PartnerIntegration.Vni;

public class VniRequestMapper : ITransientDependency
{
    private const string VniDateFormat = "dd/MM/yyyy HH:mm";
    private static readonly CultureInfo VniCulture = CultureInfo.InvariantCulture;

    public VniCarTndsIssueRequest ToCarTndsRequest(
        Policy policy,
        PolicyVersion version,
        PolicyProduct product,
        PolicyRiskMotor? motor)
    {
        var ngayHt = DateTime.UtcNow.ToString(VniDateFormat, VniCulture);
        var ngayHl = version.EffectDate.ToString(VniDateFormat, VniCulture);
        var ngayKt = version.ExpireDate.ToString(VniDateFormat, VniCulture);

        var request = new VniCarTndsIssueRequest
        {
            PhiDt = product.Premium,
            Thue = product.Vat,
            Ttoan = product.PremiumTotal,
            NgayHt = ngayHt,
            NgayHl = ngayHl,
            NgayKt = ngayKt,
            TenKh = policy.InsuredName ?? "",
            PhoneKh = policy.InsuredPhone ?? "",
            EmailKh = policy.InsuredEmail ?? "",
            DchiKh = policy.InsuredAddress ?? policy.InsuredFullAddress ?? "",
            SoCmt = policy.InsuredIdNo ?? "",
            Ten = policy.InsuredName ?? "",
            Dchi = policy.InsuredAddress ?? policy.InsuredFullAddress ?? "",
            Phone = policy.InsuredPhone ?? "",
            Email = policy.InsuredEmail ?? "",
            PhiTnds = product.PremiumTotal
        };

        if (motor != null)
        {
            request.BienXe = motor.CarPlate ?? "";
            request.SoKhung = motor.CarVin ?? "";
            request.SoMay = motor.CarEngineNumber ?? "";
            request.HangXe = motor.CarBrandCode ?? "";
            request.HieuXe = motor.CarModelCode ?? "";
            request.SoCn = (int)(motor.CarSeatNumber ?? 5);
            if (motor.CarProductionYear.HasValue)
            {
                request.TgianSx = motor.CarProductionYear.Value.ToString("MM/yyyy", VniCulture);
            }
        }

        return request;
    }

    public VniCarVcxIssueRequest ToCarVcxRequest(
        Policy policy,
        PolicyVersion version,
        PolicyProduct product,
        PolicyRiskMotor? motor)
    {
        var ngayHt = DateTime.UtcNow.ToString(VniDateFormat, VniCulture);
        var ngayHl = version.EffectDate.ToString(VniDateFormat, VniCulture);
        var ngayKt = version.ExpireDate.ToString(VniDateFormat, VniCulture);

        var request = new VniCarVcxIssueRequest
        {
            PhiDt = product.Premium,
            Thue = product.Vat,
            Ttoan = product.PremiumTotal,
            NgayHt = ngayHt,
            NgayHl = ngayHl,
            NgayKt = ngayKt,
            TenKh = policy.InsuredName ?? "",
            PhoneKh = policy.InsuredPhone ?? "",
            EmailKh = policy.InsuredEmail ?? "",
            DchiKh = policy.InsuredAddress ?? policy.InsuredFullAddress ?? "",
            SoCmt = policy.InsuredIdNo ?? "",
            Ten = policy.InsuredName ?? "",
            Dchi = policy.InsuredAddress ?? policy.InsuredFullAddress ?? "",
            Phone = policy.InsuredPhone ?? "",
            Email = policy.InsuredEmail ?? "",
            PhiTnds = 0,
            PhiTv = product.Premium,
            GiaXeTk = "0",
            GiaXe = "0",
            TienBh = "0",
            MucKtru = 0
        };

        if (motor != null)
        {
            request.BienXe = motor.CarPlate ?? "";
            request.SoKhung = motor.CarVin ?? "";
            request.SoMay = motor.CarEngineNumber ?? "";
            request.HangXe = motor.CarBrandCode ?? "";
            request.HieuXe = motor.CarModelCode ?? "";
            request.SoCn = (int)(motor.CarSeatNumber ?? 5);
            if (motor.CarProductionYear.HasValue)
            {
                request.TgianSx = motor.CarProductionYear.Value.ToString("MM/yyyy", VniCulture);
            }
            if (motor.RiskObjectValue.HasValue && motor.RiskObjectValue.Value > 0)
            {
                var val = (long)motor.RiskObjectValue.Value;
                request.GiaXeTk = val.ToString();
                request.GiaXe = val.ToString();
                request.TienBh = val.ToString();
            }
        }

        return request;
    }
}
