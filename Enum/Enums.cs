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
        Pending = 0,      
        Confirmed = 1,    
        Preparing = 2,   
        Transporting = 3, 
        Setup = 4,        
        Completed = 5,    
        Cancelled = 6,    
        Rejected = 7      
    }
}