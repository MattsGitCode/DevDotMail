using Nancy;
using SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        public static MailParser mailParser;

        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var db = new SQLiteConnection(@"C:\Users\Matt\Documents\visual studio 2013\Projects\DevDotMail\DevDotMail\bin\devdotmail.db", SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            container.Register(db);

            db.CreateTable<Email>();


            var folderToName = ConfigurationManager.AppSettings.Keys
                .Cast<string>()
                .Where(x => x.StartsWith("mailFolder:"))
                .ToDictionary(
                    key => ConfigurationManager.AppSettings[key],
                    key => key.Substring(12));

            if (mailParser == null)
            {
                mailParser = new MailParser(folderToName, db);
            }

            container.Register(mailParser);
        }
    }
}