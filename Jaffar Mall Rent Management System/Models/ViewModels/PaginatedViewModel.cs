
namespace Jaffar_Mall_Rent_Management_System.Models.ViewModels
{
    public class PaginatedViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => (PageIndex > 1);
        public bool HasNextPage => (PageIndex < TotalPages);

        public PaginatedViewModel(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }
    }
}
