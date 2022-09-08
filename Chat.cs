using System;

namespace Stevebot
{
    public class Chat
    {
        public static List<Chat> Chats = new List<Chat>();
        public ulong[] users{ get; } = new ulong[10];
        List<ulong> left_users { get; } = new List<ulong>();
        public ulong guild_id { get; }
        public ulong channel_id { get; }
        public List<KeyValuePair<ulong,string>> messageHistory { get; set; }
        public bool just_listening { get; }

        public Chat(ulong user, ulong guild, ulong channel, string firstMsg)
        {
            users[0] = user;
            guild_id = guild;
            channel_id = channel;
            messageHistory = new List<KeyValuePair<ulong, string>>();
            messageHistory.Add(new KeyValuePair<ulong,string>(user,firstMsg));
        }

        public Chat(ulong user, ulong guild, ulong channel)
        {
            users[0] = user;
            guild_id = guild;
            channel_id = channel;
            messageHistory = new List<KeyValuePair<ulong, string>>();
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

            // move to class variable so that different prompts can be used
            string fullMsg = $"This is a chat log between an Artificial Intelligence, {botName} and a human, {await Bot.client.GetUserAsync(users[0])}. The current date is {DateTime.Now.ToString("MMMM d, hh:mmtt")}\n\n";
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