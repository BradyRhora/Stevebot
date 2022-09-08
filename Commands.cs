using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using OpenAI_API;

namespace Stevebot
{
    public class Commands : ModuleBase
    {
        Random rdm = new Random();

        [Command("help"), Summary("Displays commands and information about topics.")]
        public async Task Help(string topic = null)
        {
            await Context.Channel.SendMessageAsync("ja mate!");
        }

        [Command("be me"), Summary("Generate a 4chan greentext story")]
        public async Task Greentext([Remainder]string input)
        {
            var response = await Bot.openapi.Completions.CreateCompletionAsync("Write a 4chan greentext story.\n\n> be me\n" + input, max_tokens: 256, temperature: 0.6);
            await Context.Channel.SendMessageAsync(response.ToString().Replace(">","\\>"));
        }

        [Command("talk"), Summary("Chat time with Stevey.")]
        public async Task Talk([Remainder]string input = "")
        {
            if (Chat.Chats.Where(x => x.users.Contains(Context.User.Id)).Count() > 0) await Context.Channel.SendMessageAsync("We're already chatting somewhere else.");
            else
            {
                await Context.Message.AddReactionAsync(Emoji.Parse("💬"));
                Chat newChat = null;
                if (input == "" || input == " ")
                {
                    var firstMsg = await Bot.openapi.Completions.CreateCompletionAsync("Say the first line in a conversation:\n", max_tokens: 128, temperature: 0.8);

                    var trimmed = firstMsg.ToString().Trim('"', ' ', '"');
                    await Context.Channel.SendMessageAsync(trimmed);
                    newChat = new Chat(Context.User.Id, Context.Guild.Id, Context.Channel.Id, trimmed);
                }
                else
                {
                    newChat = new Chat(Context.User.Id, Context.Guild.Id, Context.Channel.Id);
                    await Context.Channel.SendMessageAsync(await newChat.GetNextMessageAsync(Context.Message));
                }
                Chat.Chats.Add(newChat);
                await Context.Message.RemoveReactionAsync(Emoji.Parse("💬"), Bot.client.CurrentUser);
            }
        }

        public class Chat
        {
            public static List<Chat> Chats = new List<Chat>();
            public ulong[] users{ get; } = new ulong[10];
            List<ulong> left_users { get; } = new List<ulong>();
            public ulong guild_id { get; }
            public ulong channel_id { get; }
            public List<KeyValuePair<ulong,string>> messageHistory { get; set; }

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

}