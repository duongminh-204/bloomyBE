namespace Bloomy.Models.Enums
{
    public enum UserRole
    {
        Customer = 0,
        ShopOwner = 1,
        Admin = 2
    }

    public enum OrderStatus
    {
        PendingConfirmation = 0,
        WaitingDeposit = 1,
        Confirmed = 2,
        Preparing = 3,
        Transporting = 4,
        SettingUp = 5,
        Completed = 6,
        Cancelled = 7,
        Rejected = 8,
        CancelRequested = 9
    }
}