using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace slack_message_archive_viewer
{
    /// <summary>
    /// https://api.slack.com/events/message
    /// </summary>
    [DataContract]
    class SlackMessage
    {
        [DataMember]
        internal string user { get; set; }

        [DataMember]
        internal string type { get; set; }

        [DataMember]
        internal string subtype { get; set; }

        [DataMember]
        internal string text { get; set; }

        [DataMember]
        internal double ts { get; set; }

        internal DateTime Timestamp
        {
            get
            {
                // 小数点以下はチャンネル内のユニークID
                return UnixTime.FromUnixTime((long)ts);
            }
        }

        public override string ToString()
        {
            return text;
        }
        private static Regex usersRegex = new Regex(@"\<@([a-zA-Z][a-zA-Z0-9]*)\>", RegexOptions.Multiline);
        private static Regex htmlEscaped = new Regex(@"(&amp;|&lt;|&gt;|&quot;)", RegexOptions.Multiline);
        public string ToDisplayText(IEnumerable<SlackUser> users)
        {
            var unescapedText = htmlEscaped.Replace( text,
                m => m.Groups[1].Value == "&amp;" ? "&"
                    : m.Groups[1].Value == "&lt;" ? "<"
                    : m.Groups[1].Value == "&gt;" ? ">"
                    : m.Groups[1].Value == "&quot;" ? "\""
                    : "" );

            var matches = usersRegex.Matches(unescapedText);
            if (matches.Count > 0)
            {
                return usersRegex.Replace(unescapedText, (m) => 
                    {
                        var foundUser = users.Where(u => u.id ==  m.Groups[1].Value) ;
                        return foundUser.Any() ? "@" + foundUser.Single().name : m.Value;
                    });
            }
            else
                return unescapedText;
        }
    }

    [DataContract]
    class SlackUser
    {
        [DataMember]
        internal string id { get; set; }

        [DataMember]
        internal string name { get; set; }
    }

    public static class UnixTime
    {
        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /*===========================================================================*/
        /**
         * 現在時刻からUnixTimeを計算する.
         *
         * @return UnixTime.
         */
        public static long Now()
        {
            return (FromDateTime(DateTime.UtcNow));
        }

        /*===========================================================================*/
        /**
         * UnixTimeからDateTimeに変換.
         *
         * @param [in] unixTime 変換したいUnixTime.
         * @return 引数時間のDateTime.
         */
        public static DateTime FromUnixTime(long unixTime)
        {
            return UNIX_EPOCH.AddSeconds(unixTime).ToLocalTime();
        }

        /*===========================================================================*/
        /**
         * 指定時間をUnixTimeに変換する.
         *
         * @param [in] dateTime DateTimeオブジェクト.
         * @return UnixTime.
         */
        public static long FromDateTime(DateTime dateTime)
        {
            double nowTicks = (dateTime.ToUniversalTime() - UNIX_EPOCH).TotalSeconds;
            return (long)nowTicks;
        }
    }
}
