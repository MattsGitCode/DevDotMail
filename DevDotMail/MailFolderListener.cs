using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace DevDotMail
{
    public class MailFolderListener : IDisposable
    {
        IEnumerable<string> folders;
        IEnumerable<FileSystemWatcher> fileSystemWatchers;
        Action<string, Stream> emailFoundAction;

        public MailFolderListener(IEnumerable<string> folders, Action<string, Stream> emailFoundAction, bool listen)
        {
            this.folders = folders;
            this.emailFoundAction = emailFoundAction;

            if (listen)
            {
                fileSystemWatchers = folders.Select(f =>
                {
                    var watcher = new FileSystemWatcher(f);

                    watcher.Created += FileWatcherEvent;
                    watcher.Changed += FileWatcherEvent;

                    watcher.EnableRaisingEvents = true;

                    return watcher;
                });
            }
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

        public void CheckFoldersNow()
        {
            folders.ToList().ForEach(folder =>
            {
                Directory.GetFiles(folder).ToList().ForEach(file =>
                {
                    TryFile(folder, file);
                });
            });
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