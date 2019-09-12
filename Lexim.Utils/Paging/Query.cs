using System.ComponentModel;

namespace Lexim.Utils.Paging
{
    public class Query
    {
        public string SortColumn { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public string SearchText { get; set; }

        public int StartIndex => (Page - 1) * PageSize;
        public int EndIndex => StartIndex + PageSize;
    }
}