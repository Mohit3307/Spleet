namespace Spleet.Models
{
    public enum SplitType
    {
        Equal = 0,
        Custom = 1
    }

    public enum GroupRole
    {
        Member = 0,
        Admin = 1
    }

    public enum SettlementStatus
    {
        Pending = 0,
        Completed = 1,
        Disputed = 2
    }

    public enum ActivityType
    {
        ExpenseAdded = 0,
        ExpenseEdited = 1,
        ExpenseDeleted = 2,
        MemberJoined = 3,
        MemberLeft = 4,
        SettlementRequested = 5,
        SettlementConfirmed = 6,
        SettlementDisputed = 7,
        GroupCreated = 8
    }
}