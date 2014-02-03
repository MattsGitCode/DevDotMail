using Nancy;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DevDotMail
{
    public class DevDotMailModule : NancyModule
    {
        public DevDotMailModule(SQLiteConnection db, IRootPathProvider rootPathProvider)
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

                if (Request.Url.Query == "?original")
                {
                    string safeSubject = Regex.Replace(email.Subject, "[^\\w-_0-9]", " ");
                    safeSubject = safeSubject.Replace("  ", " ");
                    string fileName = safeSubject + ".eml";

                    var stream = File.OpenRead(Path.Combine(rootPathProvider.GetRootPath(), "App_Data", email.OriginalMailFileId));
                    return Response.FromStream(stream, "message/rfc822").AsAttachment(fileName);
                }

                if (email.HasAttachments)
                {
                    email.Attachments = db.Table<EmailAttachment>()
                        .Where(x => x.EmailId == email.Id)
                        .Where(x => x.IsAttachment == true)
                        .ToList();
                }

                return View[email];
            };

            Get["/cid-{id}"] = _ =>
            {
                string id = _.id;
                var attachment = db.Table<EmailAttachment>()
                    .Where(x => x.ContentId == id)
                    .SingleOrDefault();

                if (attachment == null)
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);

                var stream = File.OpenRead(Path.Combine(rootPathProvider.GetRootPath(), "App_Data", attachment.FileId));
                return Response.FromStream(stream, attachment.ContentType);
            };

            Get["/attachment-{id}"] = _ =>
            {
                int id = _.id;
                var attachment = db.Table<EmailAttachment>()
                    .Where(x => x.Id == id)
                    .SingleOrDefault();

                if (attachment == null)
                    return Negotiate.WithStatusCode(HttpStatusCode.NotFound);

                var stream = File.OpenRead(Path.Combine(rootPathProvider.GetRootPath(), "App_Data", attachment.FileId));
                return Response.FromStream(stream, attachment.ContentType);
            };
        }
    }
}