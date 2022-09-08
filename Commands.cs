using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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

        
        
    }

}