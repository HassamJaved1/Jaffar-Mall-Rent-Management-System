namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class Property
    {
    
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
         
        public string? Description { get; set; }

        public string PropertyType { get; set; } = string.Empty;

        public Guid PropertyCode { get; set; }

        public string PropertyNumber { get; set; } = string.Empty;

        public int FloorNumber { get; set; }

        // New fields (match new DB schema)
        public string? Address { get; set; }
        
        public string? City { get; set; }

        public string? Country { get; set; }

        public string? TenantName { get; set; }

        //public PropertyStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    //public enum PropertyStatus
    //{
    //    Vacant = 1,
    //    Occupied = 2,
    //    Pending = 3
    //}
}
