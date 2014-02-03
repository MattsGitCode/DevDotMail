using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class EmailListModel
    {
        public List<Email> Emails { get; private set; }
        public int TotalRecordCount { get; private set; }
        public QueryBuilder Builder { get; private set; }

        public int CurrentPage { get; private set; }
        public int PageStartRecord { get; private set; }
        public int PageLastRecord { get; private set; }

        public int TotalPages { get; private set; }
        public IEnumerable<int> PagesToLink { get; private set; }
        public bool ShowFirstPageLink { get; private set; }
        

        public EmailListModel(List<Email> emails, int total, QueryBuilder builder)
        {
            Emails = emails;
            TotalRecordCount = total;
            Builder = builder;

            CurrentPage = builder.Page;

            PageStartRecord = (CurrentPage - 1) * builder.PageSize + 1;
            PageLastRecord = PageStartRecord + Emails.Count - 1;

            TotalPages = total / builder.PageSize + (((total % builder.PageSize) == 0) ? 0 : 1);

            int pagesToShow = 5;

            if (pagesToShow > TotalPages)
            {
                PagesToLink = Enumerable.Range(1, TotalPages);
            }
            else
            {
                int pagesBefore = pagesToShow / 2;
                int startPage;
                if ((CurrentPage - 1) < pagesBefore)
                    startPage = 1;
                else if (CurrentPage > (TotalPages - pagesToShow) + pagesBefore)
                    startPage = TotalPages - pagesToShow + 1;
                else
                    startPage = CurrentPage - pagesBefore;

                PagesToLink = Enumerable.Range(startPage, pagesToShow);

                ShowFirstPageLink = startPage > 1;
            }
        }
    }
}