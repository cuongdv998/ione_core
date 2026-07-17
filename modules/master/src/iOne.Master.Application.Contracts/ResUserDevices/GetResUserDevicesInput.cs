using System;
using iOne.ResUserDevices;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUserDevices;

public class GetResUserDevicesInput : PagedAndSortedResultRequestDto
{
    public string? UserName { get; set; }
    public string? DeviceUid { get; set; }
    public string? AppChannelCode { get; set; }
    public ResUserDeviceStatus? Status { get; set; }
}
