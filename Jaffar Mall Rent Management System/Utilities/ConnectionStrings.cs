namespace Jaffar_Mall_Rent_Management_System.Utilities
{
    public class ConnectionStrings
    {
        private static string? _mainConStr;

        public static void Initialize(IConfiguration configuration)
        {
           _mainConStr = configuration.GetConnectionString("DefaultConnection")!;
        }

       public static string MainConStr => _mainConStr ?? string.Empty;
    }
}
