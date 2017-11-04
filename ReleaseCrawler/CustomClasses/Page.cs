using ReleaseCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReleaseCrowler.CustomClasses
{
    public class Paginator<T>
    {
        public int PageSize { get; private set; }
        public int CurrentPage { get; set; }
        
        public IEnumerable<T> Items { get; private set; }

        public Paginator(IEnumerable<T> items)
        {
            Items = items;
            PageSize = 20;
            CurrentPage = 1;
        }
        public Paginator(IEnumerable<T> releases, int pageSize):this(releases)
        {
            PageSize = pageSize;
        }
        
        public IEnumerable<T> GetPage(int pageNumber)
        {
            int startIndex = PageSize * (pageNumber - 1);

            if (Items == null || startIndex > Items.Count())
            {
                return null;
            }

            return Items.Skip(startIndex).Take(PageSize);
        }
    }
}