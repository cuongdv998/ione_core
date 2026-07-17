using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Application.Services;
using iOne.AccountPaymentRequests;

namespace iOne.Payment.VnPay;

public class VnPayAppService : ApplicationService, IVnPayAppService
{
    private readonly VnPayOptions _options;
    private readonly IRepository<AccountPaymentRequest, Guid> _accountPaymentRequestRepository;

    public VnPayAppService(
        IOptions<VnPayOptions> options,
        IRepository<AccountPaymentRequest, Guid> accountPaymentRequestRepository)
    {
        _options = options.Value;
        _accountPaymentRequestRepository = accountPaymentRequestRepository;
    }

    public Task<string> CreatePaymentUrlAsync(CreateVnPayRequestDto input)
    {
        var vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", _options.Version);
        vnpay.AddRequestData("vnp_Command", _options.Command);
        vnpay.AddRequestData("vnp_TmnCode", _options.TmnCode);

        var amount = (long)(input.Amount * 100);
        vnpay.AddRequestData("vnp_Amount", amount.ToString());
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", string.IsNullOrEmpty(input.IpAddress) ? "127.0.0.1" : input.IpAddress);
        vnpay.AddRequestData("vnp_Locale", string.IsNullOrEmpty(input.Locale) ? "vn" : input.Locale);

        if (!string.IsNullOrEmpty(input.BankCode))
        {
            vnpay.AddRequestData("vnp_BankCode", input.BankCode);
        }

        vnpay.AddRequestData("vnp_OrderInfo", input.OrderInfo);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", _options.ReturnUrl);
        vnpay.AddRequestData("vnp_TxnRef", input.OrderId);

        var paymentUrl = vnpay.CreateRequestUrl(_options.BaseUrl, _options.HashSecret);
        Console.WriteLine(paymentUrl);

        return Task.FromResult(paymentUrl);
    }

    public Task<VnPayReturnDto> HandleReturnAsync(IDictionary<string, string> queryParameters)
    {
        var vnpay = new VnPayLibrary();
        string vnp_SecureHash = string.Empty;

        foreach (var kvp in queryParameters)
        {
            if (kvp.Key.StartsWith("vnp_"))
            {
                if (kvp.Key == "vnp_SecureHash")
                {
                    vnp_SecureHash = kvp.Value;
                }
                else
                {
                    vnpay.AddResponseData(kvp.Key, kvp.Value);
                }
            }
        }

        bool isSignatureValid = vnpay.ValidateSignature(vnp_SecureHash, _options.HashSecret);
        var txnRef = vnpay.GetResponseData("vnp_TxnRef");
        var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var transactionNo = vnpay.GetResponseData("vnp_TransactionNo");

        if (!isSignatureValid)
        {
            return Task.FromResult(new VnPayReturnDto
            {
                IsSuccess = false,
                Message = "Chữ ký không hợp lệ"
            });
        }

        if (responseCode == "00")
        {
            var rawAmount = vnpay.GetResponseData("vnp_Amount");
            var amountStr = !string.IsNullOrEmpty(rawAmount) && long.TryParse(rawAmount, out long amt)
                            ? (amt / 100).ToString() : "0";

            return Task.FromResult(new VnPayReturnDto
            {
                IsSuccess = true,
                Message = "Thanh toán thành công",
                Data = new VnPayReturnDataDto
                {
                    TxnRef = txnRef,
                    Amount = amountStr,
                    BankCode = vnpay.GetResponseData("vnp_BankCode"),
                    BankTranNo = vnpay.GetResponseData("vnp_BankTranNo"),
                    CardType = vnpay.GetResponseData("vnp_CardType"),
                    TransactionNo = vnpay.GetResponseData("vnp_TransactionNo"),
                    PayDate = vnpay.GetResponseData("vnp_PayDate"),
                    ResponseCode = responseCode
                }
            });
        }
        else
        {
            return Task.FromResult(new VnPayReturnDto
            {
                IsSuccess = false,
                Message = $"Thanh toán thất bại. Mã lỗi: {responseCode}"
            });
        }
    }

    public async Task<VnPayReturnDto> HandleIpnAsync(IDictionary<string, string> queryParameters)
    {
        var vnpay = new VnPayLibrary();
        string vnp_SecureHash = string.Empty;

        foreach (var kvp in queryParameters)
        {
            if (kvp.Key.StartsWith("vnp_"))
            {
                if (kvp.Key == "vnp_SecureHash")
                {
                    vnp_SecureHash = kvp.Value;
                }
                else
                {
                    vnpay.AddResponseData(kvp.Key, kvp.Value);
                }
            }
        }

        bool isSignatureValid = vnpay.ValidateSignature(vnp_SecureHash, _options.HashSecret);
        var txnRef = vnpay.GetResponseData("vnp_TxnRef");
        var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var transactionNo = vnpay.GetResponseData("vnp_TransactionNo");

        if (!isSignatureValid)
        {
            return new VnPayReturnDto
            {
                IsSuccess = false,
                Message = "Invalid Checksum",
                Data = new VnPayReturnDataDto { ResponseCode = "97" }
            };
        }

        if (!Guid.TryParse(txnRef, out var paymentId))
        {
            return new VnPayReturnDto
            {
                IsSuccess = false,
                Message = "Invalid payment reference",
                Data = new VnPayReturnDataDto { ResponseCode = "01", TxnRef = txnRef }
            };
        }

        var paymentRequest = await _accountPaymentRequestRepository.GetAsync(paymentId);

        if (responseCode == "00")
        {
            paymentRequest.UpdateStatus(AccountPaymentRequestStatus.Approved);
            paymentRequest.UpdateTransRef(transactionNo);
        }
        else
        {
            paymentRequest.UpdateStatus(AccountPaymentRequestStatus.Rejected);
        }

        await _accountPaymentRequestRepository.UpdateAsync(paymentRequest, autoSave: true);

        return new VnPayReturnDto
        {
            IsSuccess = true,
            Message = "Confirm Success",
            Data = new VnPayReturnDataDto { ResponseCode = "00", TxnRef = txnRef }
        };
    }

    public async Task<VnPayQueryDrResponseDto> QueryTransactionAsync(VnPayQueryDrRequestDto input)
    {
        var requestId = string.IsNullOrWhiteSpace(input.RequestId)
            ? Guid.NewGuid().ToString("N")[..32]
            : input.RequestId.Trim();
        var ipAddress = string.IsNullOrWhiteSpace(input.IpAddress) ? "127.0.0.1" : input.IpAddress.Trim();
        var createDate = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");

        var requestPayload = new Dictionary<string, string>
        {
            ["vnp_RequestId"] = requestId,
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = "querydr",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_TxnRef"] = input.TxnRef.Trim(),
            ["vnp_OrderInfo"] = input.OrderInfo.Trim(),
            ["vnp_TransactionDate"] = input.TransactionDate.Trim(),
            ["vnp_CreateDate"] = createDate,
            ["vnp_IpAddr"] = ipAddress
        };

        if (!string.IsNullOrWhiteSpace(input.TransactionNo))
        {
            requestPayload["vnp_TransactionNo"] = input.TransactionNo.Trim();
        }

        var signData =
            $"{requestPayload["vnp_RequestId"]}|{requestPayload["vnp_Version"]}|{requestPayload["vnp_Command"]}|{requestPayload["vnp_TmnCode"]}|{requestPayload["vnp_TxnRef"]}|{requestPayload["vnp_TransactionDate"]}|{requestPayload["vnp_CreateDate"]}|{requestPayload["vnp_IpAddr"]}|{requestPayload["vnp_OrderInfo"]}";
        requestPayload["vnp_SecureHash"] = HmacSha512(_options.HashSecret, signData);

        var json = JsonSerializer.Serialize(requestPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var response = await client.PostAsync(_options.QueryApiUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new VnPayQueryDrResponseDto
            {
                IsSuccess = false,
                Message = $"VNPAY query failed with HTTP {(int)response.StatusCode}: {responseBody}",
                IsSignatureValid = false
            };
        }

        var responsePayload = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Dictionary<string, string>();

        responsePayload.TryGetValue("vnp_SecureHash", out var responseSecureHash);
        var responseDataToSign =
            $"{GetValue(responsePayload, "vnp_ResponseId")}|{GetValue(responsePayload, "vnp_Command")}|{GetValue(responsePayload, "vnp_ResponseCode")}|{GetValue(responsePayload, "vnp_Message")}|{GetValue(responsePayload, "vnp_TmnCode")}|{GetValue(responsePayload, "vnp_TxnRef")}|{GetValue(responsePayload, "vnp_Amount")}|{GetValue(responsePayload, "vnp_BankCode")}|{GetValue(responsePayload, "vnp_PayDate")}|{GetValue(responsePayload, "vnp_TransactionNo")}|{GetValue(responsePayload, "vnp_TransactionType")}|{GetValue(responsePayload, "vnp_TransactionStatus")}|{GetValue(responsePayload, "vnp_OrderInfo")}|{GetValue(responsePayload, "vnp_PromotionCode")}|{GetValue(responsePayload, "vnp_PromotionAmount")}";
        var expectedHash = HmacSha512(_options.HashSecret, responseDataToSign);
        var isSignatureValid = !string.IsNullOrWhiteSpace(responseSecureHash)
            && string.Equals(responseSecureHash, expectedHash, StringComparison.OrdinalIgnoreCase);

        var apiResponseCode = GetValue(responsePayload, "vnp_ResponseCode");

        return new VnPayQueryDrResponseDto
        {
            IsSuccess = apiResponseCode == "00" && isSignatureValid,
            Message = isSignatureValid
                ? GetValue(responsePayload, "vnp_Message")
                : "Invalid response checksum from VNPAY",
            IsSignatureValid = isSignatureValid,
            ResponseId = GetValue(responsePayload, "vnp_ResponseId"),
            Command = GetValue(responsePayload, "vnp_Command"),
            TmnCode = GetValue(responsePayload, "vnp_TmnCode"),
            TxnRef = GetValue(responsePayload, "vnp_TxnRef"),
            Amount = GetValue(responsePayload, "vnp_Amount"),
            OrderInfo = GetValue(responsePayload, "vnp_OrderInfo"),
            ResponseCode = apiResponseCode,
            VnpMessage = GetValue(responsePayload, "vnp_Message"),
            BankCode = GetValue(responsePayload, "vnp_BankCode"),
            PayDate = GetValue(responsePayload, "vnp_PayDate"),
            TransactionNo = GetValue(responsePayload, "vnp_TransactionNo"),
            TransactionType = GetValue(responsePayload, "vnp_TransactionType"),
            TransactionStatus = GetValue(responsePayload, "vnp_TransactionStatus"),
            PromotionCode = GetValue(responsePayload, "vnp_PromotionCode"),
            PromotionAmount = GetValue(responsePayload, "vnp_PromotionAmount")
        };
    }

    private static string GetValue(IDictionary<string, string> source, string key)
    {
        return source.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
    }

    private static string HmacSha512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);
        foreach (var theByte in hashValue)
        {
            hash.Append(theByte.ToString("x2"));
        }

        return hash.ToString();
    }
}
