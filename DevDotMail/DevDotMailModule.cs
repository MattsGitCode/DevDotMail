using Nancy;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class DevDotMailModule : NancyModule
    {
        public DevDotMailModule(SQLiteConnection db)
        {
            Get["/"] = _ =>
            {
                var table = db.Table<Email>()
                    .Where(x => x.IsArchived == false);

                var builder = new QueryBuilder(Request);

                var filtered = builder.Filter(table);
                
                var total = filtered.Count();
                
                var emails = filtered
                    .OrderByDescending(x => x.Date)
                    .Skip((builder.Page - 1) * builder.PageSize)
                    .Take(builder.PageSize)
                    .ToList();
                
                var model = new EmailListModel(emails, total, builder);
                ViewBag.Title = "Latest Emails";
                ViewBag.SearchTerm = builder.Search;

                return View[model];
            };

            Get["/email-{id}"] = _ =>
            {
                var builder = new QueryBuilder(Request);

                int id = _.id;
                var email = db.Table<Email>()
                    .Where(x => x.Id == id)
                    .SingleOrDefault();

                if (email == null)
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);

                if (email.HasAttachments)
                {
                    email.Attachments = db.Table<EmailAttachment>()
                        .Where(x => x.EmailId == email.Id)
                        .ToList();
                }

                return View[email];
            };
        }
    }
}