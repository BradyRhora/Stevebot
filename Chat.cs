using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using OpenAI_API;
namespace Stevebot
{

    public class Chat
    {
        #region subclasses
        public class ChatMessage
        {
            public ulong Sender { get; }
            public string Message { get; }
            public DateTime Time { get; }

            public ChatMessage(ulong sender, string message)
            {
                Sender = sender;
                Message = message;
                Time = DateTime.Now;
            }
            public ChatMessage(ulong sender, string message, DateTime time)
            {
                Sender = sender;
                Message = message;
                Time = time;
            }
        }

        public class ChatUser
        {
            public ulong Id { get; }
            public DateTime LastMsg { get; set; }
            public bool Left { get; set; } = false;

            public ChatUser(ulong id)
            {
                Id = id;
                LastMsg = DateTime.Now;
            }
        }
        #endregion

        public static List<Chat> Chats = new List<Chat>();

        public List<ChatUser> users { get; } = new List<ChatUser>();
        public ulong channel_id { get; }
        public List<ChatMessage> messageHistory { get; set; }

        bool just_listening = false;
        int messagesUntilJoin = 0;
        DateTime lastTimeSent = DateTime.MinValue;
        int secondDelay = 0;
        const int MEMORY_LENGTH = 15;
        const ulong BOT_ID = Constants.Users.STEVEY;
        const string MIN_BOT_NAME = "steve";

        private string[] prompts = {
                                        "This is a chat log between an all-knowing but kind and humorous Artificial Intelligence, [BOT], and a human, [USER]. The current date is [DATE].",
                                        "This is a chat log between some users in a university chat room for York University, in Toronto Canada. Occasionally, an Artificial Intelligence known as [BOT] chimes in with his knowledge banks or just to have fun. The current date is [DATE]."
                                   };

        public Chat(ulong user, ulong channel, string botFirstMsg)
        {
            users.Add(new ChatUser(user));
            channel_id = channel;
            messageHistory = new List<ChatMessage>();
            messageHistory.Add(new ChatMessage(BOT_ID, botFirstMsg));
            Chats.Add(this);
        }

        public Chat(ulong user, ulong channel, bool listening = false)
        {
            users.Add(new ChatUser(user));
            channel_id = channel;
            messageHistory = new List<ChatMessage>();
            if (listening)
            {
                just_listening = true;
                messagesUntilJoin = Bot.rdm.Next(3, MEMORY_LENGTH + 1);
            }
            Chats.Add(this);
        }

        public ChatUser GetUser(ulong id) { return users.Where(x => x.Id == id).FirstOrDefault(); }

        public void Join(IUser user)
        {
            if (users.Where(x => x.Id == user.Id).Count() == 0)
            {
                users.Add(new ChatUser(user.Id));
                Console.WriteLine($"[DEBUG] {user.Username} has entered the chat.");
                messageHistory.Add(new ChatMessage(0, $"{user.Username} has entered the chat."));
            }

        }

        // returns true if there are no users remaining
        public bool Leave(IUser user)
        {
            bool found = false;
            users.Where(x => x.Id == user.Id).First().Left = true;

            Console.WriteLine($"[DEBUG] {user.Username} has left the chat. {users.Where(x => x.Left == false).Count()}/{users.Count()}");
            messageHistory.Add(new ChatMessage(0, $"{user.Username} has left the chat."));

            if (users.Where(x => x.Left == false).Count() == 0)
            {
                Chats.Remove(this);
                return true;
            }
            return false;
        }

        public async Task Update()
        {
            foreach (var user in users)
            {
                if (DateTime.Now - user.LastMsg > TimeSpan.FromMinutes(5)) Leave(await Bot.client.GetUserAsync(user.Id));
            }
        }


        public async Task<string> GetNextMessageAsync(IMessage message)
        {
            GetUser(message.Author.Id).LastMsg = DateTime.Now;
            messageHistory.Add(new ChatMessage(message.Author.Id, message.Content.Replace(Bot.COMMAND_PREFIX + "talk", "").Trim(' ')));

            bool botMentioned = message.Content.ToLower().Contains(MIN_BOT_NAME) || message.MentionedUserIds.Contains(BOT_ID);
            bool timePassed = (DateTime.Now - lastTimeSent) > new TimeSpan(0, 0, secondDelay);

            if (!botMentioned && !timePassed)
                return "";

            int activeUsers = users.Where(x => !x.Left).Count();
            int ignoreChance = (activeUsers - 1) * 7;
            bool ignore = Bot.rdm.Next(0, 100) < (ignoreChance < 100 ? ignoreChance : 100);

            if (ignore)
                return "";

            int max = (activeUsers - 1) * 7;
            secondDelay = Bot.rdm.Next(0, max); // random amount of seconds from 0 to (7 * (#ofusers - 1))
            Console.WriteLine($"[DEBUG] second delay from 0 to {max} is {secondDelay}");
            lastTimeSent = DateTime.Now;


            string botName = Bot.client.CurrentUser.Username;
            if (just_listening)
            {
                if (--messagesUntilJoin > 0) return "";
                else if (messagesUntilJoin == 0)
                {
                    var bbGen = await Bot.client.GetChannelAsync(Constants.Channels.BB_GENERAL) as IGuildChannel;
                    var gUser = await bbGen.GetUserAsync(Bot.client.CurrentUser.Id);
                    await gUser.ModifyAsync(x => x.Nickname = gUser.DisplayName.Replace(Constants.Emotes.EAR.Name, ""));
                }
            }

            using (message.Channel.EnterTypingState())
            {
                string fullMsg;
                if (just_listening) fullMsg = prompts[1];
                else fullMsg = prompts[0];
                string dnl = "\n\n"; // double newline
                //string dnl = "  ";
                fullMsg = fullMsg.Replace("[BOT]", botName).Replace("[USER]", (await Bot.client.GetUserAsync(users[0].Id)).Username).Replace("[DATE]", DateTime.Now.ToString("MMMM d, hh:mmtt")) + dnl;

                int start = messageHistory.Count() - (MEMORY_LENGTH - 1);

                for (int i = start >= 0 ? start : 0; i < messageHistory.Count(); i++)
                {
                    fullMsg += $"[{messageHistory[i].Time.ToShortTimeString()}] ";
                    if (messageHistory[i].Sender != 0)
                    {
                        if (messageHistory[i].Sender == BOT_ID) fullMsg += botName + ": \"";
                        else
                        {
                            var user = await Bot.client.GetUserAsync(messageHistory[i].Sender);
                            fullMsg += $"{user.Username}: \"";
                        }
                        fullMsg += messageHistory[i].Message + "\"" + dnl;
                    }
                    else fullMsg += messageHistory[i].Message + dnl;
                }

                fullMsg += $"[{DateTime.Now.ToShortTimeString()}] " + botName + ": \"";
                var response = await Bot.openapi.Completions.CreateCompletionAsync(fullMsg, temperature: 0.85, max_tokens: 128, stopSequences: "\"");
                messageHistory.Add(new ChatMessage(BOT_ID, response.ToString()));
                System.Threading.Thread.Sleep(response.ToString().Length * 75);
                return await ReplaceNameWithPingAsync(response.ToString());
            }
        }

        async Task<string> ReplaceNameWithPingAsync(string msg)
        {
            foreach (var u in users)
            {
                var user = await Bot.client.GetUserAsync(u.Id);
                if (msg.Contains(user.Username))
                    msg = msg.Replace(user.Username, $"<@{user.Id}>");
            }

            return msg;
        }

        public static async void ChatTimerCallBack(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var chat in Chats)
            {
                await chat.Update();
            }
        }
    }
}
