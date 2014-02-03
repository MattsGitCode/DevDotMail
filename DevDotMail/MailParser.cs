using MimeKit;
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
        Func<string, Stream> getAttachmentStreamForSaving;

        public MailParser(IDictionary<string, string> folderToNames, SQLiteConnection db, Func<string, Stream> getAttachmentStreamForSaving)
        {
            this.db = db;
            this.folderToNames = folderToNames;
            this.getAttachmentStreamForSaving = getAttachmentStreamForSaving;
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
            var msg = MimeMessage.Load(fileStream);
            var email = new Email();

            ExtractMimeMessageIntoEmail(msg, email);

            db.Insert(email);

            foreach (var attachment in email.Attachments)
            {
                attachment.EmailId = email.Id;
                db.Insert(attachment);
            }
        }

        void ExtractMimeMessageIntoEmail(MimeMessage msg, Email email)
        {
            email.Date = msg.Date.Date;
            email.To = msg.To.ToString();
            email.Subject = msg.Subject;

            if (msg.From.Count != 0)
            {
                email.From = msg.From.ToString();
            }
            else if (msg.Headers.Contains("From"))
            {
                email.From = msg.Headers["From"];
            }

            ExtractMimeEntityIntoEmail(msg.Body, email);
        }

        void ExtractMimeEntityIntoEmail(MimeEntity entity, Email email)
        {
            if (entity is MessagePart)
            {
                // TODO: implement attached emails
            }
            else if (entity is Multipart)
            {
                foreach (var part in (Multipart)entity)
                {
                    ExtractMimeEntityIntoEmail(part, email);
                }
            }
            else if (entity is TextPart)
            {
                var text = (TextPart)entity;
                bool isHtml = text.ContentType.Matches("text", "html");
                if (isHtml || string.IsNullOrEmpty(email.Body))
                {
                    email.Body = text.Text;
                    email.IsBodyHtml = isHtml;
                }
            }
            else
            {
                email.HasAttachments = true;
                var part = (MimePart)entity;

                string fileId = Guid.NewGuid().ToString();

                long size;
                using (var attachmentStream = getAttachmentStreamForSaving(fileId))
                {
                    part.ContentObject.DecodeTo(attachmentStream);
                    size = attachmentStream.Length;
                }

                bool isImage = part.ContentType.Matches("image", "*");

                var attachment = new EmailAttachment
                {
                    FileId = fileId,
                    IsAttachment = part.IsAttachment,
                    IsInlineImage = !part.IsAttachment && isImage,
                    ContentType = string.Format("{0}/{1}", part.ContentType.MediaType, part.ContentType.MediaSubtype),
                    FileName = part.FileName,
                    FileSize = size,
                    ContentId = part.ContentId,
                };

                email.Attachments.Add(attachment);
            }
        }

        public void CheckAndParse()
        {
            db.BeginTransaction();
            mailFolderListener.CheckFoldersNow();
            db.Commit();
        }
    }
}