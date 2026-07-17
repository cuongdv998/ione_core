using System;
using System.Linq;
using System.Threading.Tasks;
using iOne.Claim.Claims;
using iOne.Claim.Permissions;
using iOne.SystemEventNotifies;
using iOne.ResCustomers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace iOne.Claim.Claims;

[Authorize(ClaimPermissions.Default)]
public class ClaimSentMessageAppService : ApplicationService, IClaimSentMessageAppService
{
    protected IRepository<SystemEventNotify, Guid> SystemEventNotifyRepository { get; }
    protected IRepository<ResCustomer, Guid> CustomerRepository { get; }

    public ClaimSentMessageAppService(
        IRepository<SystemEventNotify, Guid> systemEventNotifyRepository,
        IRepository<ResCustomer, Guid> customerRepository)
    {
        SystemEventNotifyRepository = systemEventNotifyRepository;
        CustomerRepository = customerRepository;
    }

    public virtual async Task<PagedResultDto<ClaimSentMessageDto>> GetListAsync(GetClaimSentMessagesInput input)
    {
        if (input.ClaimId == Guid.Empty)
        {
            throw new UserFriendlyException(L["Claim:ClaimIdRequired"]);
        }

        var notifyQuery = await SystemEventNotifyRepository.GetQueryableAsync();

        // Filter by recipient_type = 'cus'
        notifyQuery = notifyQuery.Where(x => x.RecipientType == "cus");

        // Optional: filter by ClaimId inside payload if có lưu claimId trong payload JSON
        var claimIdText = input.ClaimId.ToString("D");
        notifyQuery = notifyQuery.Where(x => x.Payload != null && x.Payload.Contains(claimIdText));

        notifyQuery = notifyQuery.OrderByDescending(x => x.CreationTime);

        var totalCount = await AsyncExecuter.CountAsync(notifyQuery);

        var items = await AsyncExecuter.ToListAsync(
            notifyQuery.Skip(input.SkipCount).Take(input.MaxResultCount > 0 ? input.MaxResultCount : 50));

        var dtos = items.Select(x => new ClaimSentMessageDto
        {
            Id = x.Id,
            CreationTime = x.CreationTime,
            SentAt = x.SentAt,
            Recipient = x.Recipient ?? string.Empty,
            ChannelName = null,
            Title = x.Title,
            Body = x.Body,
            Status = x.Status
        }).ToList();

        return new PagedResultDto<ClaimSentMessageDto>(totalCount, dtos);
    }
}

