using OpaqueMail.Net;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class MailParser : IDisposable
    {
        IDictionary<string, string> folderToNames;
        MailFolderListener mailFolderListener;
        SQLiteConnection db;

        public MailParser(IDictionary<string, string> folderToNames, SQLiteConnection db)
        {
            this.db = db;
            this.folderToNames = folderToNames;
            mailFolderListener = new MailFolderListener(folderToNames.Keys, ParseMail, false);
            mailFolderListener.CheckFoldersNow();
        }

        public void Dispose()
        {
            if (mailFolderListener != null)
            {
                mailFolderListener.Dispose();
                mailFolderListener = null;
            }
        }

        void ParseMail(string folder, Stream fileStream)
        {
            string fileContent;
            using (var reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd();
            }

            using (var msg = new ReadOnlyMailMessage(fileContent))
            {
                var email = new Email
                {
                    Date = msg.Date,
                    From = msg.From.Address,
                    To = msg.To.ToString(),
                    Subject = msg.Subject.ToString(),
                    Body = msg.Body,
                    IsBodyHtml = msg.IsBodyHtml,
                };
                db.Insert(email);
            }
        }

        public void CheckAndParse()
        {
            mailFolderListener.CheckFoldersNow();
        }
    }
}