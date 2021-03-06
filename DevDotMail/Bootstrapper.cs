﻿using Nancy;
using SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using Nancy.Conventions;

namespace DevDotMail
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            pipelines.BeforeRequest += ctx =>
            {
                var mailParser = container.Resolve<MailParser>();
                mailParser.CheckAndParse();

                return null;
            };
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            string databaseDir = Path.Combine(RootPathProvider.GetRootPath(), "App_Data");
            string databaseFile = Path.Combine(databaseDir, "devdotmail.db");

            if (!Directory.Exists(databaseDir))
                Directory.CreateDirectory(databaseDir);

            var db = new SQLiteConnection(databaseFile, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
            container.Register(db);

            db.CreateTable<Email>();
            db.CreateTable<EmailAttachment>();


            var folderToName = ConfigurationManager.AppSettings.Keys
                .Cast<string>()
                .Where(x => x.StartsWith("mailFolder:"))
                .ToDictionary(
                    key => ConfigurationManager.AppSettings[key],
                    key => key.Substring(12));

            Func<string, Stream> attachmentFileStreamGetter = fileId => File.Create(Path.Combine(databaseDir, fileId));

            var mailParser = new MailParser(folderToName, db, attachmentFileStreamGetter);

            container.Register(mailParser);
        }

        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("fonts"));
        }
    }
}