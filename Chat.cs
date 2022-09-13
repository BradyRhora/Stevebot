using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Stevebot
{
    public class Chat
    {
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


        public static List<Chat> Chats = new List<Chat>();
        public ulong[] users{ get; } = new ulong[10];
        List<ulong> left_users { get; } = new List<ulong>();
        public ulong channel_id { get; }
        public List<ChatMessage> messageHistory { get; set; }
        //public List<KeyValuePair<ulong,string>> messageHistory { get; set; }
        bool just_listening = false;
        int messagesUntilJoin = 0;

        private string[] prompts = {
                                        "This is a chat log between an all-knowing but kind and humorous Artificial Intelligence, [BOT], and a human, [USER]. The current date is [DATE].",
                                        "This is a chat log between some users. Occasionally, an Artificial Intelligence known as [BOT] chimes in with his knowledge banks or just to have fun. The current date is [DATE]."
                                   };

        public Chat(ulong user, ulong channel, string botFirstMsg)
        {
            users[0] = user;
            channel_id = channel;
            messageHistory = new List<ChatMessage>();
            messageHistory.Add(new ChatMessage(Constants.Users.STEVEY,botFirstMsg));
            Chats.Add(this);
        }

        public Chat(ulong user, ulong channel, bool listening = false)
        {
            users[0] = user;
            channel_id = channel;
            messageHistory = new List<ChatMessage>();
            if (listening) {
                just_listening = true;
                messagesUntilJoin = Bot.rdm.Next(3, 16);
            }
            Chats.Add(this);
        }

        public bool Join(IUser user)
        {
            if (left_users.Contains(user.Id))
                return false;

            for (int i = 1; i < 10; i++)
                if (users[i] == 0)
                    users[i] = user.Id;

            messageHistory.Add(new ChatMessage(0, $"{user.Username} has entered the chat."));
            return true;
        }

        // returns true if there are no users remaining
        public bool Leave(IUser user)
        {
            bool found = false;
            for (int i = 0; i < 10; i++)
            {
                if (users[i] == user.Id)
                {
                    users[i] = 0;
                    found = true;
                }
                else if (found)
                    users[i - 1] = users[i];


                messageHistory.Add(new ChatMessage(0, $"{user.Username} has left the chat."));
            }
            left_users.Add(user.Id);
            if (users.Where(x => x != 0).Count() == 0)
            {
                Chats.Remove(this);
                return true;
            }
            return false;
        }


        public async Task<string> GetNextMessageAsync(IMessage message)
        {

            messageHistory.Add(new ChatMessage(message.Author.Id,message.Content.Replace(">talk","").Trim(' ')));
            string botName = Bot.client.CurrentUser.Username;

            if (just_listening)
            {
                if (--messagesUntilJoin > 0)
                {
                    Console.WriteLine($"..in {messagesUntilJoin} messages..");
                    return "";
                }
                else if (messagesUntilJoin == 0) {
                    var bbGen = await Bot.client.GetChannelAsync(Constants.Channels.BB_GENERAL) as IGuildChannel;
                    var gUser = await bbGen.GetUserAsync(Bot.client.CurrentUser.Id);
                    await gUser.ModifyAsync(x => x.Nickname = gUser.DisplayName.Replace(Constants.Emotes.EAR.Name,""));
                }
            }
            using (message.Channel.EnterTypingState())
            {
                string fullMsg;
                if (just_listening) fullMsg = prompts[1];
                else fullMsg = prompts[0];

                fullMsg = fullMsg.Replace("[BOT]", botName).Replace("[USER]", (await Bot.client.GetUserAsync(users[0])).Username).Replace("[DATE]", DateTime.Now.ToString("MMMM d, hh:mmtt")) + "\n\n";

                foreach (var msg in messageHistory)
                {
                    fullMsg += $"[{msg.Time.ToShortTimeString()}] ";
                    if (msg.Sender != 0)
                    {
                        if (msg.Sender == Constants.Users.STEVEY) fullMsg += botName + ": \"";
                        else
                        {
                            var user = await Bot.client.GetUserAsync(msg.Sender);
                            fullMsg += $"{user.Username}: \"";
                        }
                        fullMsg += msg.Message + "\"\n\n";
                    }
                    else fullMsg += msg.Message + "\n\n";
                }

                fullMsg += $"[{DateTime.Now.ToShortTimeString()}] " + botName + ": \"";
                var response = await Bot.openapi.Completions.CreateCompletionAsync(fullMsg, temperature: 0.85, max_tokens: 128, stopSequences: "\"");
                messageHistory.Add(new ChatMessage(Constants.Users.STEVEY, response.ToString()));

                return response.ToString();
            }
        }
    }
}