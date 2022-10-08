﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            if (input.ToLower().StartsWith("remember"))
            {
                

                Dictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "remember","" },
                    { "you're", "Stevey is" },
                    { "yours", "Steveys" },
                    { "your", "Steveys" },
                    { "you", "Stevey" },
                    { "i", Context.User.Username },
                    { "me", Context.User.Username },
                    { "my", Context.User.Username + 's' },
                    { "mine", Context.User.Username + 's'},
                    { "were", "was" },
                    { "we're", $"Stevey and {Context.User.Username} are" },
                    { "we've", $"Stevey and {Context.User.Username} have" },
                    { "we", $"Stevey and {Context.User.Username}" }
                    
                };

                input = input.ToLower();

                foreach (var replace in replacements)
                {
                    input = Regex.Replace(input, $"([^a-z]|^)({replace.Key})([^a-z]|$)", $"$1{replace.Value}$3");
                }

                input = input.Trim(' ','.','?','!',',');
                
                if (Properties.Settings.Default.Memory == null) Properties.Settings.Default.Memory = "";
                Properties.Settings.Default.Memory += input + ". ";
                Properties.Settings.Default.Save();

                await ReplyAsync("Okay, I'll remember that.");

            }
            else if (Chat.Chats.Where(x => x.channel_id == Context.Channel.Id).Count() > 0)
            {
                if (input.ToLower() == "end") Chat.Chats.Remove(Chat.Chats.Where(x => x.channel_id == Context.Channel.Id).First());
                else await Context.Channel.SendMessageAsync("We're already chatting here.");
            }
            else
            {
                await Context.Message.AddReactionAsync(Emoji.Parse("💬"));
                Chat newChat = null;
                if (input == "" || input == " ")
                {
                    var firstMsg = await Bot.openapi.Completions.CreateCompletionAsync("Say a greeting for a conversation:\n", max_tokens: 128, temperature: 0.8);

                    var trimmed = firstMsg.ToString().Trim('"', ' ', '"', '\n');
                    await Context.Channel.SendMessageAsync(trimmed);
                    newChat = new Chat(Context.User.Id, Context.Channel.Id, trimmed);
                }
                else
                {
                    newChat = new Chat(Context.User.Id, Context.Channel.Id);
                    await Context.Channel.SendMessageAsync(await newChat.GetNextMessageAsync(Context.Message));
                }
                await Context.Message.RemoveReactionAsync(Emoji.Parse("💬"), Bot.client.CurrentUser);
            }
        }

        
        
    }

}