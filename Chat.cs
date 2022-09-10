using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Stevebot
{
    public class Chat
    {
        public static List<Chat> Chats = new List<Chat>();
        public ulong[] users{ get; } = new ulong[10];
        List<ulong> left_users { get; } = new List<ulong>();
        public ulong channel_id { get; }
        public List<KeyValuePair<ulong,string>> messageHistory { get; set; }
        bool just_listening = false;
        int messagesUntilJoin = 0;

        private string[] prompts = { 
                                        "This is a chat log between an all-knowing but kind Artificial Intelligence, [BOT] and a human, [USER]. The current date is [DATE]",
                                        "This is a chat log between some users. Occasionally, an Artificial Intelligence known as [BOT] chimes in with his knowledge banks or just to have fun. The current date is [DATE]."
                                   };

        public Chat(ulong user, ulong channel, string firstMsg)
        {
            users[0] = user;
            channel_id = channel;
            messageHistory = new List<KeyValuePair<ulong, string>>();
            messageHistory.Add(new KeyValuePair<ulong,string>(user,firstMsg));
        }

        public Chat(ulong user, ulong channel, bool listening = false)
        {
            users[0] = user;
            channel_id = channel;
            messageHistory = new List<KeyValuePair<ulong, string>>();
            just_listening = listening;
            if (listening) messagesUntilJoin = Bot.rdm.Next(3, 16);
        }

        public bool Join(IUser user)
        {
            if (left_users.Contains(user.Id))
                return false;

            for (int i = 1; i < 10; i++)
                if (users[i] == 0)
                    users[i] = user.Id;

            messageHistory.Add(new KeyValuePair<ulong, string>(0, $"{user.Username} has entered the chat."));
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


                messageHistory.Add(new KeyValuePair<ulong, string>(0, $"{user.Username} has left the chat."));
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

            messageHistory.Add(new KeyValuePair<ulong, string>(message.Author.Id,message.Content));
            string botName = Bot.client.CurrentUser.Username;

            if (just_listening)
            {
                if (--messagesUntilJoin > 0)
                {
                    Console.WriteLine($"..in {messagesUntilJoin} messages..");
                    return "";
                }
                else if (messagesUntilJoin == 0) {
                    var gUser = Bot.client.CurrentUser as IGuildUser;
                    gUser.Edit(name:gUser.DisplayName.Trim(Constants.Emotes.EAR));
                }
            }

            string fullMsg;
            if (just_listening) fullMsg = prompts[1];
            else fullMsg = prompts[0];
            
            fullMsg = fullMsg.Replace("[BOT]", botName).Replace("[USER]", (await Bot.client.GetUserAsync(users[0])).Username).Replace("[DATE]", DateTime.Now.ToString("MMMM d, hh:mmtt"));

            foreach (var msg in messageHistory)
            {
                if (msg.Key != 0)
                {
                    if (msg.Key == Constants.Users.STEVEY) fullMsg += botName + ": \"";
                    else
                    {
                        var userName = await Bot.client.GetUserAsync(msg.Key);
                        fullMsg += $"{userName}: \"";
                    }
                    fullMsg += msg.Value + "\"\n\n";
                }
                else fullMsg += msg.Value + "\n\n";
            }

            fullMsg += botName + ": \"";
            var response = await Bot.openapi.Completions.CreateCompletionAsync(fullMsg, temperature: 0.85, max_tokens:128, stopSequences:"\"");
            messageHistory.Add(new KeyValuePair<ulong,string>(Constants.Users.STEVEY, response.ToString()));
            return response.ToString();
        }
    }
}