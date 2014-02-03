using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevDotMail
{
    public static class Helpers
    {
        public static string ToReadableSizeString(this long size)
        {
            var s = new[] { "bytes", "kb", "mb", "gb" };
            int i = 0;
            decimal n = size;

            while (n > 1024 && i < s.Length) {
                n = n / 1024;
                i = i + 1;
            }
            return string.Format("{0:0.##} {1}", n, s[i]);
        }

        public static IHtmlString ToCrudeSafeHtml(this string html)
        {
            var match = Regex.Match(html, @".*(?is)<body(?:\s?[^>]*)>(?<body>.*?)(?:</\s*body\s*>|</\s*html\s*>|$).*");

            string body;
            if (match.Success)
            {
                body = match.Groups["body"].Value;
            }
            else
            {
                body = html;
            }

            string safeBody = Regex.Replace(body, @"<script(?:\s?[^>]*)>(.*?)(?:</\s*script\s*>|$)", string.Empty);

            string bodyWithEmbeddedImages = Regex.Replace(safeBody, "src=(?:\"?)cid:([^\\s/>\"]*)(\"?)", "src=\"/cid-$1\"");

            return new NonEncodedHtmlString(bodyWithEmbeddedImages);
        }
    }
}