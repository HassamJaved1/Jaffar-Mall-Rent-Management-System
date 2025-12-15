namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class Property
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string PropertyType { get; set; } = string.Empty;

        public string PropertyCode { get; set; } = string.Empty;

        public PropertyStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public enum PropertyStatus
    {
        Vacant = 1,
        Occupied = 2,
        Pending = 3
    }
}
