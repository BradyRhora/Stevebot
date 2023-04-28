using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SuperBlastPals;
using System.IO;

using MySql.Data;
using MySql.Data.MySqlClient;
using BradyMS;

namespace Stevebot
{
    
    public class Commands : ModuleBase
    {
        Random rdm = new Random();

        [Command("help"), Summary("Displays commands and information about topics.")]
        public async Task Help(string topic = null)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Stevey Commands"
            };

            foreach (var cmd in Bot.commands.Commands)
            {
                string commandText = Bot.COMMAND_PREFIX + cmd.Name;
                foreach (var param in cmd.Parameters) commandText += $" [{param.Name}]";
                embed.AddField(commandText, cmd.Summary);
            }

            await ReplyAsync(embed: embed.Build());
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
            else if (Chat.Chats.Where(x => x.ChannelID== Context.Channel.Id).Count() > 0)
            {
                if (input.ToLower() == "end") Chat.Chats.Remove(Chat.Chats.Where(x => x.ChannelID == Context.Channel.Id).First());
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

        [Command("series"), Summary("Display info about the specified series.")]
        public async Task Series([Remainder] string input = null)
        {
            SBPS.Series series;
            if (input == null)
                series = SBPS.Series.GetRandom();
            else
                series = SBPS.Series.Search(input);

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = series.GetName()
            };

            embed.AddField("Genre", series.GetGenre(), true);
            embed.AddField("Release Year", series.GetReleaseYear(), true);

            string gameText = "";
            var games = series.GetCharacters();
            embed.Color = games.First().GetColour();
            foreach (var game in games)
                gameText += game.GetName() + '\n';
            embed.AddField("Characters", gameText);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("character"), Summary("Display info about the specified character.")]
        public async Task Character([Remainder] string input = null)
        {
            SBPS.Character character;
            if (input == null)
                character = SBPS.Character.GetRandom();
            else 
                character = SBPS.Character.Search(input);

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = character.GetName(),
                Description = character.Blurb,
                Color = character.GetColour()
            };

            double range = character.Range;
            string sRange = range < 1 ? "short" : "far";
            if (Math.Abs(range - 1) > .5) sRange = "very " + sRange;

            double weight = character.Weight;
            string sWeight = weight < 1 ? "light" : "heavy";
            if (Math.Abs(weight - 1) > .5) sWeight = "very " + sWeight;

            double power = character.Power;
            string sPower = power < 1 ? "weak" : "strong";
            if (Math.Abs(power - 1) > .5) sPower = "very " + sPower;

            double speed = character.Speed;
            string sSpeed = speed < 1 ? "slow" : "fast";
            if (Math.Abs(speed - 1) > .5) sSpeed = "very " + sSpeed;

            double wepSize = character.Weapon_Length;
            string sWep = wepSize < 1 ? wepSize < -0.5 ? "none" : "short" : "long";
            if (Math.Abs(wepSize - 1) > .5) sWep = "very " + sWep;

            double sexy = character.Sexiness;
            string sSexy = sexy < 1 ? "ugly" : "sexy";
            if (Math.Abs(sexy - 1) > .5) sSexy = "very " + sSexy;

            double style = character.Style;
            string sStyle = style < 1 ? "lame" : "stylish";
            if (Math.Abs(style - 1) > .5) sStyle = "very " + sStyle;

            embed.AddField("Series", character.GetSeries().GetName());

            embed.AddField("Range", sRange, true);
            embed.AddField("Weight", sWeight, true);
            embed.AddField("Power", sPower, true);
            embed.AddField("Speed", sSpeed, true);
            embed.AddField("Weapon Size", sWep, true);
            embed.AddField("Sexiness", sSexy, true);
            embed.AddField("Style", sStyle, true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("player"), Summary("Display info about the specified player.")]
        public async Task Player([Remainder] string input = null)
        {
            SBPS.Player player;
            if (input == null)
                player = SBPS.Player.GetRandom();
            else
                player = SBPS.Player.Search(input);

            if (player.ID == -1)
            {
                await ReplyAsync("Player not found.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = player.Tag,
                Description = "Real name: " + player.Name,
                Color = player.GetMain().GetColour()
            };

            double weight = player.Weight;
            string sWeight = weight < 1 ? "light" : "heavy";
            if (Math.Abs(weight - 1) > .5) sWeight = "very " + sWeight;

            double charm = player.Charm;
            string sCharm = charm < 1 ? "weird" : "charming";
            if (Math.Abs(charm - 1) > .5) sCharm = "very " + sCharm;

            double anger = player.Anger;
            string sAnger = anger < 1 ? "gentle" : "angry";
            if (Math.Abs(anger - 1) > .5) sAnger = "very " + sAnger;

            double depression = player.Depression;
            string sDepress = depression < 1 ? "happy" : "depressed";
            if (Math.Abs(depression - 1) > .5) sDepress = "very " + sDepress;

            double highness = player.Highness;
            string sHigh = highness < 1 ? "sober" : "blazed";
            if (Math.Abs(highness - 1) > .5) sHigh = "very " + sHigh;

            double coordination = player.Coordination;
            string sCoordination = coordination < 1 ? "clumsy" : "coordinated";
            if (Math.Abs(coordination - 1) > .5) sCoordination = "very " + sCoordination;

            double intelligence = player.Intelligence;
            string sIntel = intelligence < 1 ? "dumb" : "smart";
            if (Math.Abs(intelligence - 1) > .5) sIntel = "very " + sIntel;

            double tech = player.Tech_Knowledge;
            string sTech = tech < 1 ? "non-technical" : "technical";
            if (Math.Abs(tech - 1) > .5) sTech = "very " + sTech;

            double stink = player.Stink;
            string sStink = stink < 1 ? "smelly" : "clean";
            if (Math.Abs(stink - 1) > .5) sStink = "very " + sStink;

            string sFinger = player.Finger_Count.ToString();


            embed.AddField("Main", player.GetMain().GetName());
            var sec = player.GetSecondary();
            if (sec.ID != -1)
                embed.AddField("Secondary", sec.GetName());

            embed.AddField("Weight", sWeight, true);
            embed.AddField("Charm", sCharm, true);
            embed.AddField("Anger", sAnger, true);
            embed.AddField("Mood", sDepress, true);
            embed.AddField("Highness", sHigh, true);
            embed.AddField("Coordination", sCoordination, true);
            embed.AddField("Intelligence", sIntel, true);
            embed.AddField("Tech Knowledge", sTech, true);
            embed.AddField("Stink", sStink, true);
            embed.AddField("Finger Count", sFinger, true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bracket"), Summary("Display the current tournament bracket.")]
        public async Task Bracket()
        {
            await ReplyAsync($"**__{SBPS.Tournament.Current.GetName()}__**```" + SBPS.Tournament.Current.BuildChart() +"```");
        }

        [Command("bms"), Summary("Get and set information for BradyMS2!")]
        public async Task BMS(string charName)
        {
            BradyMS.BMS.Character chr = new BMS.Character(charName);

            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = $"{chr.Name} {(chr.Gender == BradyMS.BMS.Character.CharacterGender.Male ? "♂️" : "♀️")} (Lvl. {chr.Level} {chr.GetJobNiche()})",
                Description = $"💰 {chr.Mesos:n0} mesos\nCurrent HP: {chr.HP/chr.MaxHP*100}%\nCurrent MP: {chr.MP/chr.MaxMP*100}%",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Joined {chr.CreateDate.ToShortDateString()}"
                }
            };
            

            await ReplyAsync(embed: embed.Build());
        }

    }

}