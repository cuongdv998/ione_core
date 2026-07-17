using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using iOne.Customer.ResCustomers;
using iOne.Hr.HrEmployees;
using iOne.HrEmployees;
using iOne.Master;
using iOne.Master.Localization;
using iOne.Master.Permissions;
using iOne.Master.ResEvents;
using iOne.Master.SystemEventNotifies;
using iOne.ResAppChannels;
using iOne.ResEvents;
using iOne.ResUserDevices;
using iOne.SystemEventNotifies;
using iOne.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RazorLight;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace iOne.Master.SystemEventNotifies;

[RemoteService(Name = MasterRemoteServiceConsts.RemoteServiceName)]
[Authorize(SystemEventNotifyPermissions.Default)]
public class SystemEventNotifyAppService : CrudAppService<
    SystemEventNotify,
    SystemEventNotifyDto,
    Guid,
    GetSystemEventNotifiesInput,
    CreateSystemEventNotifyDto,
    UpdateSystemEventNotifyDto>,
    ISystemEventNotifyAppService
{
    protected SystemEventNotifyManager Manager { get; }
    protected ISystemEventNotifyRepository NotifyRepository { get; }
    protected IResEventNotifyTemplateAppService EventNotifyTemplateAppService { get; }
    protected IHrEmployeeAppService EmployeeAppService { get; }
    protected IResCustomerAppService CustomerAppService { get; }
    protected IRepository<ResAppChannel, Guid> AppChannelRepository { get; }
    protected IRepository<ResUserDevice, Guid> UserDeviceRepository { get; }
    protected IRepository<ResEvent, Guid> ResEventRepository { get; }
    protected IRepository<ResEventNotifyTemplate, Guid> ResEventNotifyTemplateRepository { get; }
    protected IElsaWorkflowService ElsaWorkflowService { get; }
    protected IRepository<HrEmployee, Guid> HrEmployeeRepository { get; }
    protected IRepository<IdentityUser, Guid> UserRepository { get; }
    protected INotificationHubService NotificationHubService { get; }
    private readonly RazorLightEngine _razorEngine;

    public SystemEventNotifyAppService(
        ISystemEventNotifyRepository repository,
        SystemEventNotifyManager manager,
        IResEventNotifyTemplateAppService eventNotifyTemplateAppService,
        IHrEmployeeAppService employeeAppService,
        IResCustomerAppService customerAppService,
        IRepository<ResAppChannel, Guid> appChannelRepository,
        IRepository<ResUserDevice, Guid> userDeviceRepository,
        IRepository<ResEvent, Guid> resEventRepository,
        IRepository<ResEventNotifyTemplate, Guid> resEventNotifyTemplateRepository,
        IElsaWorkflowService elsaWorkflowService,
        IRepository<HrEmployee, Guid> hrEmployeeRepository,
        IRepository<IdentityUser, Guid> userRepository,
        INotificationHubService notificationHubService)
        : base(repository)
    {
        Manager = manager;
        NotifyRepository = repository;
        EventNotifyTemplateAppService = eventNotifyTemplateAppService;
        EmployeeAppService = employeeAppService;
        CustomerAppService = customerAppService;
        AppChannelRepository = appChannelRepository;
        UserDeviceRepository = userDeviceRepository;
        ResEventRepository = resEventRepository;
        ResEventNotifyTemplateRepository = resEventNotifyTemplateRepository;
        ElsaWorkflowService = elsaWorkflowService;
        HrEmployeeRepository = hrEmployeeRepository;
        UserRepository = userRepository;
        NotificationHubService = notificationHubService;
        LocalizationResource = typeof(MasterResource);
        GetPolicyName = SystemEventNotifyPermissions.View;
        GetListPolicyName = SystemEventNotifyPermissions.View;
        CreatePolicyName = SystemEventNotifyPermissions.Create;
        UpdatePolicyName = SystemEventNotifyPermissions.Edit;
        DeletePolicyName = SystemEventNotifyPermissions.Delete;

        _razorEngine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();
    }

    public override async Task<SystemEventNotifyDto> CreateAsync(CreateSystemEventNotifyDto input)
    {
        var entity = new SystemEventNotify(
            GuidGenerator.Create(),
            input.EventCode,
            input.AppChannelId,
            input.Title,
            input.Body,
            input.Payload,
            input.RecipientType,
            input.RecipientId,
            input.Recipient,
            input.Status,
            input.ScheduleAt,
            input.SentAt,
            input.ReadAt,
            input.ErrorMessage,
            input.RetryNumber
        );

        await Manager.CreateAsync(entity);

        return ObjectMapper.Map<SystemEventNotify, SystemEventNotifyDto>(entity);
    }

    public async Task<PagedResultDto<SystemEventNotifyDto>> GetListWebNotifyAsync(GetWebNotifiesInput input)
    {
        var notifyQuery = await ReadOnlyRepository.GetQueryableAsync();
        var channelQuery = await AppChannelRepository.GetQueryableAsync();
        var hrEmployeeQuery = await HrEmployeeRepository.GetQueryableAsync();

        var currentUserId = CurrentUser.Id;

        var query = from notify in notifyQuery
                    join channel in channelQuery on notify.AppChannelId equals channel.Id
                    join emp in hrEmployeeQuery on notify.RecipientId equals emp.Id
                    where channel.Type == ResAppChannelType.Web
                    && notify.Status != SystemEventNotifyStatus.Fail
                    && notify.Status != SystemEventNotifyStatus.Pending
                    && notify.Status != SystemEventNotifyStatus.Deactive
                    && emp.UserId == currentUserId
                    select notify;

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.SentAt)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount > 0 ? input.MaxResultCount : 10)
        );

        // Map sang DTO
        var dtos = ObjectMapper.Map<List<SystemEventNotify>, List<SystemEventNotifyDto>>(entities);

        return new PagedResultDto<SystemEventNotifyDto>(totalCount, dtos);
    }

    public override async Task<SystemEventNotifyDto> UpdateAsync(Guid id, UpdateSystemEventNotifyDto input)
    {
        var entity = await Repository.GetAsync(id);

        // Cập nhật Status và ReadAt
        await Manager.UpdateAsync(entity, input.Status, input.ReadAt);

        await CurrentUnitOfWork!.SaveChangesAsync();

        return ObjectMapper.Map<SystemEventNotify, SystemEventNotifyDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var entity = await Repository.GetAsync(id);

        // Delete trước để trigger ABP audit log (soft delete)
        await Repository.DeleteAsync(entity);
        await CurrentUnitOfWork!.SaveChangesAsync();

        // Sau đó cập nhật status về Deactive
        entity.UpdateStatus(SystemEventNotifyStatus.Deactive);
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    protected override async Task<IQueryable<SystemEventNotify>> CreateFilteredQueryAsync(GetSystemEventNotifiesInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.EventCode))
        {
            query = query.Where(x => EF.Functions.ILike(x.EventCode, $"%{input.EventCode}%"));
        }

        if (input.AppChannelId.HasValue)
        {
            query = query.Where(x => x.AppChannelId == input.AppChannelId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.RecipientType))
        {
            query = query.Where(x => x.RecipientType == input.RecipientType);
        }

        if (input.RecipientId.HasValue)
        {
            query = query.Where(x => x.RecipientId == input.RecipientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Recipient))
        {
            query = query.Where(x => EF.Functions.ILike(x.Recipient ?? string.Empty, $"%{input.Recipient}%"));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == input.Status.Value);
        }

        if (input.ScheduleAtFrom.HasValue)
        {
            query = query.Where(x => x.ScheduleAt >= input.ScheduleAtFrom.Value);
        }

        if (input.ScheduleAtTo.HasValue)
        {
            query = query.Where(x => x.ScheduleAt <= input.ScheduleAtTo.Value);
        }

        if (input.SentAtFrom.HasValue)
        {
            query = query.Where(x => x.SentAt != null && x.SentAt >= input.SentAtFrom.Value);
        }

        if (input.SentAtTo.HasValue)
        {
            query = query.Where(x => x.SentAt != null && x.SentAt <= input.SentAtTo.Value);
        }


        return query;
    }

    public async Task SendNotify(Guid eventId, EventNotifyInfoDto infor)
    {
        if (eventId == Guid.Empty) { return; }

        ResEventNotifyTemplateDto eventTemplate = await EventNotifyTemplateAppService.GetAsync(eventId);

        if (eventTemplate == null)
        {
            throw new UserFriendlyException(L["EventNotifyTemplateNotFound", eventId]);
        }

        // Get channel information to determine channel type
        var appChannel = await AppChannelRepository.GetAsync(eventTemplate.AppChannelId);
        var channelType = appChannel.Type?.ToString().ToLowerInvariant() ?? string.Empty;

        // Build model once from parameters (shared for all recipients)
        var model = BuildModel(infor.Parameters);

        // Render Title and Body once using Razor (shared for all recipients)
        string renderedTitle = await _razorEngine.CompileRenderStringAsync(
            $"title_{eventId}", eventTemplate.Title, model);

        string renderedBody = await _razorEngine.CompileRenderStringAsync(
            $"body_{eventId}", eventTemplate.Body, model);

        var notifyEntities = new List<SystemEventNotify>();

        // Get all employees by IDs in one query
        List<HrEmployeeDto> employees = new();
        if (infor.Recipients?.EmployeeIds != null && infor.Recipients.EmployeeIds.Count > 0)
        {
            employees = await EmployeeAppService.GetListByIdsAsync(infor.Recipients.EmployeeIds);
        }

        // Get all customers by IDs in one query
        List<ResCustomerDto> customers = new();
        if (infor.Recipients?.CustomerIds != null && infor.Recipients.CustomerIds.Count > 0)
        {
            customers = await CustomerAppService.GetListByIdsAsync(infor.Recipients.CustomerIds);
        }

        // For mobile and web channel types, get device tokens by usernames
        // Join HrEmployee với IdentityUser để lấy UserName từ User
        Dictionary<string, List<string>> deviceTokensByUserName = new();
        Dictionary<Guid, string> userNameByEmployeeId = new();
        Dictionary<Guid, Guid> userIdByEmployeeId = new();

        if (channelType == "mobile" || channelType == "web")
        {
            var employeeIds = employees.Select(e => e.Id).ToList();

            if (employeeIds.Count > 0)
            {
                // Join HrEmployee với IdentityUser để lấy UserName
                var employeeQuery = await HrEmployeeRepository.GetQueryableAsync();
                var userQuery = await UserRepository.GetQueryableAsync();

                var employeeUserData = await (
                    from emp in employeeQuery
                    join user in userQuery on emp.UserId equals user.Id into userGroup
                    from user in userGroup.DefaultIfEmpty()
                    where employeeIds.Contains(emp.Id) && emp.UserId != null
                    select new { emp.Id, UserId = emp.UserId, UserName = user != null ? user.UserName : null }
                ).ToListAsync();

                // Build dictionary EmployeeId -> UserName
                userNameByEmployeeId = employeeUserData
                    .Where(x => !string.IsNullOrEmpty(x.UserName))
                    .ToDictionary(x => x.Id, x => x.UserName!);

                // Build dictionary EmployeeId -> UserId for SignalR
                userIdByEmployeeId = employeeUserData
                    .Where(x => x.UserId.HasValue)
                    .ToDictionary(x => x.Id, x => x.UserId!.Value);

                var userNames = userNameByEmployeeId.Values.Distinct().ToList();

                if (userNames.Count > 0)
                {
                    var userDevices = await (await UserDeviceRepository.GetQueryableAsync())
                        .Where(d => userNames.Contains(d.UserName)
                            && d.Status == ResUserDeviceStatus.Active
                            && (d.ExpirDate == null || d.ExpirDate >= DateTime.Now.Date))
                        .ToListAsync();

                    deviceTokensByUserName = userDevices
                        .GroupBy(d => d.UserName)
                        .ToDictionary(g => g.Key, g => g.Select(d => d.DeviceToken).ToList());
                }
            }
        }

        // Process Employee recipients
        foreach (var employee in employees)
        {
            var recipients = GetRecipientsByChannelType(channelType, employee, deviceTokensByUserName, userNameByEmployeeId);

            foreach (var recipient in recipients)
            {
                var entity = new SystemEventNotify(
                    GuidGenerator.Create(),
                    infor.EventCode,
                    eventTemplate.AppChannelId,
                    renderedTitle,
                    renderedBody,
                    null,
                    "emp",
                    employee.Id,
                    recipient,
                    SystemEventNotifyStatus.Pending,
                    infor.Schedule,
                    null,
                    null,
                    null,
                    0
                );

                notifyEntities.Add(entity);
            }
        }

        // Process Customer recipients
        foreach (var customer in customers)
        {
            var recipient = GetCustomerRecipientByChannelType(channelType, customer);

            if (!string.IsNullOrEmpty(recipient))
            {
                var entity = new SystemEventNotify(
                    GuidGenerator.Create(),
                    infor.EventCode,
                    eventTemplate.AppChannelId,
                    renderedTitle,
                    renderedBody,
                    null, // Payload
                    "cus",
                    customer.Id,
                    recipient,
                    SystemEventNotifyStatus.Pending,
                    infor.Schedule,
                    null, // SentAt
                    null, // ReadAt
                    null, // ErrorMessage
                    0 // RetryNumber
                );

                notifyEntities.Add(entity);
            }
        }

        foreach (var entity in notifyEntities)
        {
            await Manager.CreateAsync(entity);
        }

        await CurrentUnitOfWork!.SaveChangesAsync();

        if (notifyEntities.Count > 0)
        {
            var notifyIds = notifyEntities.Select(e => e.Id).ToList();
            await ElsaWorkflowService.InitSendNotifyWorkflowAsync(notifyIds);
        }
    }

    public async Task SendWebNotificationAsync(SendWebNotificationDto input)
    {
        if (input.NotificationIds == null || input.NotificationIds.Count == 0)
        {
            return;
        }

        var notifyQuery = await ReadOnlyRepository.GetQueryableAsync();
        var channelQuery = await AppChannelRepository.GetQueryableAsync();
        var hrEmployeeQuery = await HrEmployeeRepository.GetQueryableAsync();

        var notifies = await (
            from notify in notifyQuery
            join channel in channelQuery on notify.AppChannelId equals channel.Id
            join emp in hrEmployeeQuery on notify.RecipientId equals emp.Id
            where input.NotificationIds.Contains(notify.Id)
                && channel.Type == ResAppChannelType.Web
                && notify.RecipientType == "emp"
                && emp.UserId != null
            select new { notify.Id, notify.Title, notify.Body, UserId = emp.UserId!.Value }
        ).ToListAsync();

        foreach (var item in notifies)
        {
            await NotificationHubService.SendNotificationToUserAsync(
                item.UserId, item.Title, item.Body, item.Id);
        }
    }

    /// <summary>
    /// Get recipients for employee based on channel type
    /// </summary>
    private List<string> GetRecipientsByChannelType(
        string channelType,
        HrEmployeeDto employee,
        Dictionary<string, List<string>> deviceTokensByUserName,
        Dictionary<Guid, string> userNameByEmployeeId)
    {
        var recipients = new List<string>();

        switch (channelType)
        {
            case "email":
                if (!string.IsNullOrEmpty(employee.Email))
                {
                    recipients.Add(employee.Email);
                }
                break;

            case "mobile":
            case "web":
                if (userNameByEmployeeId.TryGetValue(employee.Id, out var userName) &&
                    deviceTokensByUserName.TryGetValue(userName, out var tokens))
                {
                    recipients.AddRange(tokens);
                }
                break;

            case "phone":
                // recipient = phone
                if (!string.IsNullOrEmpty(employee.Phone))
                {
                    recipients.Add(employee.Phone);
                }
                break;

            default:
                recipients.Add(employee.FullName);
                break;
        }

        return recipients;
    }

    /// <summary>
    /// Get recipient for customer based on channel type
    /// </summary>
    private string? GetCustomerRecipientByChannelType(string channelType, ResCustomerDto customer)
    {
        return channelType switch
        {
            "email" => customer.Email,
            "phone" => customer.Phone,
            "web" => customer.Name,
            "mobile" => null,
            _ => customer.Name
        };
    }

    private ExpandoObject BuildModel(Dictionary<string, object>? parameters)
    {
        var model = new ExpandoObject();
        var modelDict = (IDictionary<string, object>)model;

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                modelDict[kvp.Key] = kvp.Value;
            }
        }

        return model;
    }

    public async Task<List<SystemEventNotifyWithChannelTypeDto>> GetListByIdsWithChannelTypeAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new List<SystemEventNotifyWithChannelTypeDto>();
        }

        var notifyQuery = await ReadOnlyRepository.GetQueryableAsync();
        var channelQuery = await AppChannelRepository.GetQueryableAsync();

        var result = await (
            from notify in notifyQuery
            join channel in channelQuery on notify.AppChannelId equals channel.Id into channelGroup
            from channel in channelGroup.DefaultIfEmpty()
            where ids.Contains(notify.Id)
            select new SystemEventNotifyWithChannelTypeDto
            {
                Id = notify.Id,
                EventCode = notify.EventCode,
                AppChannelId = notify.AppChannelId,
                ChannelType = channel != null ? channel.Type.ToString().ToLowerInvariant() : null,
                Title = notify.Title,
                Body = notify.Body,
                Payload = notify.Payload,
                RecipientType = notify.RecipientType,
                RecipientId = notify.RecipientId,
                Recipient = notify.Recipient,
                Status = notify.Status,
                ScheduleAt = notify.ScheduleAt,
                SentAt = notify.SentAt,
                ReadAt = notify.ReadAt,
                ErrorMessage = notify.ErrorMessage,
                RetryNumber = notify.RetryNumber,
                CreationTime = notify.CreationTime,
                CreatorId = notify.CreatorId,
                LastModificationTime = notify.LastModificationTime,
                LastModifierId = notify.LastModifierId,
                IsDeleted = notify.IsDeleted,
                DeleterId = notify.DeleterId,
                DeletionTime = notify.DeletionTime
            }
        ).ToListAsync();

        return result;
    }

    /// <summary>
    /// Cập nhật kết quả gửi notification
    /// - Nếu gửi thành công: status = Sent, retry_number + 1, sent_at = now
    /// - Nếu gửi thất bại: status = Fail, retry_number + 1, error_message = errorMessage
    /// </summary>
    public async Task<SystemEventNotifyDto> UpdateSendResultAsync(Guid id, UpdateNotifySendResultDto input)
    {
        var entity = await Repository.GetAsync(id);

        if (input.IsSuccess)
        {
            entity.MarkAsSent();
        }
        else
        {
            entity.MarkAsFailed(input.ErrorMessage);
        }

        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork!.SaveChangesAsync();

        return ObjectMapper.Map<SystemEventNotify, SystemEventNotifyDto>(entity);
    }

    /// <summary>
    /// Lấy danh sách các bản tin gửi lỗi trong ngày có retry_number nhỏ hơn số cấu hình của bảng res_event_notify_template
    /// </summary>
    public async Task<List<SystemEventNotifyWithChannelTypeDto>> GetFailedNotifiesToRetryAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var notifyQuery = await ReadOnlyRepository.GetQueryableAsync();
        var eventQuery = await ResEventRepository.GetQueryableAsync();
        var templateQuery = await ResEventNotifyTemplateRepository.GetQueryableAsync();
        var channelQuery = await AppChannelRepository.GetQueryableAsync();

        var result = await (
            from notify in notifyQuery
            join template in templateQuery on new { notify.AppChannelId } equals new { template.AppChannelId }
            join channel in channelQuery on notify.AppChannelId equals channel.Id into channelGroup
            from channel in channelGroup.DefaultIfEmpty()
            where notify.Status == SystemEventNotifyStatus.Fail
                && notify.CreationTime >= today
                && notify.CreationTime < tomorrow
                && notify.RetryNumber < template.RetryNumber
            select new SystemEventNotifyWithChannelTypeDto
            {
                Id = notify.Id,
                EventCode = notify.EventCode,
                AppChannelId = notify.AppChannelId,
                ChannelType = channel != null ? channel.Type.ToString().ToLowerInvariant() : null,
                Title = notify.Title,
                Body = notify.Body,
                Payload = notify.Payload,
                RecipientType = notify.RecipientType,
                RecipientId = notify.RecipientId,
                Recipient = notify.Recipient,
                Status = notify.Status,
                ScheduleAt = notify.ScheduleAt,
                SentAt = notify.SentAt,
                ReadAt = notify.ReadAt,
                ErrorMessage = notify.ErrorMessage,
                RetryNumber = notify.RetryNumber,
                CreationTime = notify.CreationTime,
                CreatorId = notify.CreatorId,
                LastModificationTime = notify.LastModificationTime,
                LastModifierId = notify.LastModifierId,
                IsDeleted = notify.IsDeleted,
                DeleterId = notify.DeleterId,
                DeletionTime = notify.DeletionTime
            }
        ).ToListAsync();

        return result;
    }
}
