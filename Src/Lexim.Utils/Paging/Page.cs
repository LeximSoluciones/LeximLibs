using System;
using System.Collections.Generic;

namespace Lexim.Utils.Paging
{
    public class Page<T>
    {
        public int Count { get; set; }
        public List<T> List { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        public int StartIndex => (PageIndex - 1) * PageSize;
        public int EndIndex => Math.Min(Count, StartIndex + PageSize);
    }
}