using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class Email
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Folder { get; set; }

        public DateTime Date { get; set; }
        [MaxLength(100)]
        public string From { get; set; }
        [MaxLength(400)]
        public string To { get; set; }
        [MaxLength(100)]
        public string Subject { get; set; }
        [MaxLength(4000)]
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }

        public bool HasAttachments { get; set; }

        public bool IsArchived { get; set; }

        [Ignore]
        public List<EmailAttachment> Attachments { get; set; }

        public Email()
        {
            Attachments = new List<EmailAttachment>();
        }
    }
}