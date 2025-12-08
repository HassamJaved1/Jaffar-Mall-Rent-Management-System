namespace Jaffar_Mall_Rent_Management_System.Models
{
    public class BackendResponse<T>
    {
        public T Data { get; set; } = default!;

        public string Message { get; set; } = string.Empty;

        public int Code { get; set; }

    }
}
