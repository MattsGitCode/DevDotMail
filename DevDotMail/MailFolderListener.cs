﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class MailFolderListener : IDisposable
    {
        IEnumerable<FileSystemWatcher> fileSystemWatchers;
        Action<string, Stream> emailFoundAction;

        public MailFolderListener(IEnumerable<string> folders, Action<string, Stream> emailFoundAction)
        {
            this.emailFoundAction = emailFoundAction;

            fileSystemWatchers = folders.Select(f =>
            {
                var watcher = new FileSystemWatcher(f)
                {
                    EnableRaisingEvents = true,
                };

                watcher.Created += FileWatcherEvent;
                watcher.Changed += FileWatcherEvent;

                return watcher;
            });

            folders.ToList().ForEach(folder =>
            {
                Directory.GetFiles(folder).ToList().ForEach(file =>
                {
                    TryFile(folder, file);
                });
            });
        }

        public void Dispose()
        {
            if (fileSystemWatchers != null)
            {
                foreach(var watcher in fileSystemWatchers)
                {
                    watcher.Dispose();
                }
                fileSystemWatchers = null;
            }
        }

        void FileWatcherEvent(object sender, FileSystemEventArgs e)
        {
            string folder = ((FileSystemWatcher)sender).Path;
            TryFile(folder, e.FullPath);
        }

        void TryFile(string folder, string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose))
                {
                    emailFoundAction(folder, stream);
                }
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }
    }
}