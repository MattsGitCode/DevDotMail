using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class EmailAttachment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int EmailId { get; set; }
        public string FileId { get; set; }

        public bool IsAttachment { get; set; }
        public bool IsInlineImage { get; set; }

        public string ContentType { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}