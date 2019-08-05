using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace slack_message_archive_viewer
{
    internal class SlackArchive
    {
        private static T Deserialize<T>(Stream stream) where T : class
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            return ser.ReadObject(stream) as T;
        }

        private static DataContractJsonSerializer SlackUsersSerializer = new DataContractJsonSerializer(typeof(SlackUser[]));
        private static SlackUser[] DeserializeUsers(Stream stream)
        {
            return SlackUsersSerializer.ReadObject(stream) as SlackUser[];
        }
        private static DataContractJsonSerializer SlackMessagesSerializer = new DataContractJsonSerializer(typeof(SlackMessage[]));
        private static SlackMessage[] DeserializeMessages(Stream stream)
        {
            return SlackMessagesSerializer.ReadObject(stream) as SlackMessage[];
        }
        public static SlackArchive FromExportFolder(string basePath)
        {
            SlackUser[] users = { };
            using (var stream = File.OpenRead(Path.Combine(basePath, "users.json")))
            {
                users = DeserializeUsers(stream);
            }

            var channels = new Dictionary<string, List<SlackMessage>>();
            foreach (var channelDir in Directory.GetDirectories(basePath))
            {
                var allMessages = new List<SlackMessage>();

                foreach (var file in Directory.GetFiles(channelDir))
                {
                    using (var stream = File.OpenRead(file))
                    {
                        var messagesOfDay = DeserializeMessages(stream);
                        allMessages.AddRange(messagesOfDay);
                    }
                }
                channels.Add(Path.GetFileName(channelDir), allMessages);

            }
            return new SlackArchive(users, channels);

        }
        private Dictionary<string, List<SlackMessage>> channels = new Dictionary<string, List<SlackMessage>>();
        private List<SlackUser> users;

        public SlackArchive(IEnumerable<SlackUser> users, Dictionary<string, List<SlackMessage>> channels)
        {
            this.channels = new Dictionary<string, List<SlackMessage>>(channels);
            this.users = new List<SlackUser>(users);
        }
        public SlackUser GetUserFromId(string id)
        {
            return users.SingleOrDefault(user => user.id == id) ?? SlackUser.Default;
        }

        public IEnumerable<SlackUser> Users
        {
            get
            {
                return users.AsReadOnly();
            }
        }
        public IEnumerable<String> Channels
        {
            get
            {
                return channels.Keys;
            }
        }
        public IEnumerable<SlackMessage> GetMessage(string channel)
        {
            return channels[channel];
        }

    }
}