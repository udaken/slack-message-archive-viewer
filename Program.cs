using System;
using System.Collections.Generic;
using System.Linq;

namespace slack_message_archive_viewer
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            var exeName = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: " + exeName + " ARCHIVE-PATH [channel]");
                return 2;
            }

            var basePath = args[0];
            var channel = args.Length > 1 ?  args[1] : null;
            try
            {
                var archive = SlackArchive.FromExportFolder(basePath);
                foreach (var channelName in archive.Channels.Where(channelName => (channel != null) ? channelName == channel : true))
                {
                    Console.WriteLine("<<#" + channelName + ">>\n");

                    foreach (var msg in archive.GetMessage(channelName))
                    {
                        Console.WriteLine(msg.Timestamp.ToString("s") + " " + (msg.user != null ? archive.GetUserFromId(msg.user).name : "????") + "|" + msg.ToDisplayText(archive.Users) + "\n");
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }

        }
    }
}
