using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace iOne.Payment.VnPay;

[Route("api/payment/vnpay")]
public class VnPayController : AbpControllerBase
{
    private readonly IVnPayAppService _vnPayAppService;

    public VnPayController(IVnPayAppService vnPayAppService)
    {
        _vnPayAppService = vnPayAppService;
    }

    [HttpPost("create-url")]
    public async Task<IActionResult> CreatePaymentUrl([FromBody] CreateVnPayRequestDto input)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ip) || ip == "::1")
        {
            ip = "127.0.0.1";
        }
        input.IpAddress = ip;
        
        if (string.IsNullOrEmpty(input.OrderId))
        {
            input.OrderId = System.DateTime.Now.Ticks.ToString();
        }

        var url = await _vnPayAppService.CreatePaymentUrlAsync(input);
        return Ok(new { Url = url });
    }

    [HttpGet("ipn")]
    public async Task<IActionResult> Ipn()
    {
        var parameters = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

        var result = await _vnPayAppService.HandleIpnAsync(parameters);

        if (!result.IsSuccess)
        {
            return Ok(new { RspCode = "97", Message = result.Message });
        }

        // Logic to update database status for Order goes here
        // e.g. using result.Data.TxnRef to get OrderId

        return Ok(new { RspCode = result.Data?.ResponseCode ?? "00", Message = result.Message });
    }

    [HttpGet("return-url")]
    public async Task<IActionResult> ReturnUrl()
    {
        var parameters = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());

        var result = await _vnPayAppService.HandleReturnAsync(parameters);

        return Ok(result);
    }

    [HttpPost("query-transaction")]
    public async Task<IActionResult> QueryTransaction([FromBody] VnPayQueryDrRequestDto input)
    {
        if (string.IsNullOrWhiteSpace(input.IpAddress))
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            input.IpAddress = string.IsNullOrWhiteSpace(ip) || ip == "::1" ? "127.0.0.1" : ip;
        }

        var result = await _vnPayAppService.QueryTransactionAsync(input);
        return Ok(result);
    }

}
