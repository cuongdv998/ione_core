namespace iOne.Policy.Permissions;

public static class PolicyPermissions
{
    public const string GroupName = "PolicyPolicy";

    public const string Default = GroupName;
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
    public const string View = Default + ".View";

    /// <summary>
    /// Policy request approval (policy creation request approval screen).
    /// </summary>
    public static class RequestApproval
    {
        public const string Default = GroupName + ".RequestApproval";
        public const string View = Default + ".View";
        public const string Approve = Default + ".Approve";
    }

    /// <summary>
    /// Policy terminate request approval (policy termination request approval screen).
    /// </summary>
    public static class TerminateApproval
    {
        public const string Default = GroupName + ".TerminateApproval";
        public const string View = Default + ".View";
        public const string Approve = Default + ".Approve";
    }

    public static class EndorsementApproval
    {
        public const string Default = GroupName + ".EndorsementApproval";
        public const string View = Default + ".View";
        public const string Approve = Default + ".Approve";
    }
}
