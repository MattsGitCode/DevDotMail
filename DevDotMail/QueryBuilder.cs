using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class QueryBuilder
    {
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public string Search { get; private set; }
        public string Recipient { get; private set; }

        public QueryBuilder(Nancy.Request Request)
        {
            Page = 1;
            if (Request.Query.page != null && Request.Query.page > 0)
                Page = Request.Query.page;

            PageSize = 10;

            Search = Request.Query.q;
            Recipient = Request.Query.to;
        }

        public TableQuery<Email> Filter(TableQuery<Email> query)
        {
            if (!string.IsNullOrWhiteSpace(Search))
            {
                bool includeRecipientInSearch = Search.Contains("@") && string.IsNullOrWhiteSpace(Recipient);
                if (includeRecipientInSearch)
                {
                    query = query.Where(
                        x => x.To.Contains(Search)
                        || x.Subject.Contains(Search)
                        || x.Body.Contains(Search));
                }
                else
                {
                    query = query.Where(
                        x => x.Subject.Contains(Search)
                        || x.Body.Contains(Search));
                }
            }

            if (!string.IsNullOrWhiteSpace(Recipient))
            {
                query = query.Where(x => x.To == Recipient);
            }

            return query;
        }

        QueryBuilder Clone()
        {
            return (QueryBuilder)MemberwiseClone();
        }

        string CreateUrl()
        {
            var query = new Dictionary<string, string>();
            if (Page != 1)
                query.Add("page", Page.ToString());
            if (!string.IsNullOrWhiteSpace(Search))
                query.Add("q", Search);
            if (!string.IsNullOrWhiteSpace(Recipient))
                query.Add("to", Recipient);

            var parts = query.Select(x =>
                string.Format("{0}={1}",
                HttpUtility.UrlEncode(x.Key),
                HttpUtility.UrlEncode(x.Value)));

            var queryString = string.Join("&", parts.ToArray());
            return "?" + queryString;
        }

        public string PageUrl(int page)
        {
            var builder = Clone();
            builder.Page = page;
            return builder.CreateUrl();
        }

        public string RecipientUrl(string recipient)
        {
            var builder = Clone();
            builder.Recipient = recipient;
            return builder.CreateUrl();
        }

        public string EmailUrl(int id)
        {
            return "/email-" + id + CreateUrl();
        }
    }
}