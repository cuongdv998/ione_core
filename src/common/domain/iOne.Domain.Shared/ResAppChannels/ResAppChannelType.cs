using System;
using System.ComponentModel;

namespace iOne.ResAppChannels;

public enum ResAppChannelType
{
    [Description("Email")]
    Email = 1,

    [Description("Mobile")]
    Mobile = 2,

    [Description("Web")]
    Web = 3,

    [Description("Phone")]
    Phone = 4,

    [Description("Other")]
    Other = 5
}
