using System;
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
                Description = character.GetBlurb(),
                Color = character.GetColour()
            };

            double range = character.GetRange();
            string sRange = range < 0 ? "short" : "far";
            if (Math.Abs(range) > .5) sRange = "very " + sRange;

            double weight = character.GetWeight();
            string sWeight = weight < 0 ? "light" : "heavy";
            if (Math.Abs(weight) > .5) sWeight = "very " + sWeight;

            double power = character.GetPower();
            string sPower = power < 0 ? "weak" : "strong";
            if (Math.Abs(power) > .5) sPower = "very " + sPower;

            double speed = character.GetSpeed();
            string sSpeed = speed < 0 ? "slow" : "fast";
            if (Math.Abs(speed) > .5) sSpeed = "very " + sSpeed;

            double wepSize = character.GetWeaponLength();
            string sWep = wepSize < 0 ? wepSize < -0.5 ? "none" : "short" : "long";
            if (Math.Abs(wepSize) > .5) sWep = "very " + sWep;

            double sexy = character.GetSexiness();
            string sSexy = sexy < 0 ? "ugly" : "sexy";
            if (Math.Abs(sexy) > .5) sSexy = "very " + sSexy;

            double style = character.GetStyle();
            string sStyle = style < 0 ? "lame" : "stylish";
            if (Math.Abs(style) > .5) sStyle = "very " + sStyle;

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

            EmbedBuilder embed = new EmbedBuilder
            {
                Title = player.GetTag(),
                Description = "Real name: " + player.GetName(),
                Color = player.GetMain().GetColour()
            };

            double weight = player.GetWeight();
            string sWeight = weight < 0 ? "light" : "heavy";
            if (Math.Abs(weight) > .5) sWeight = "very " + sWeight;

            double charm = player.GetCharm();
            string sCharm = charm < 0 ? "weird" : "charming";
            if (Math.Abs(charm) > .5) sCharm = "very " + sCharm;

            double anger = player.GetAnger();
            string sAnger = anger < 0 ? "gentle" : "angry";
            if (Math.Abs(anger) > .5) sAnger = "very " + sAnger;

            double depression = player.GetDepression();
            string sDepress = depression < 0 ? "happy" : "depressed";
            if (Math.Abs(depression) > .5) sDepress = "very " + sDepress;

            double highness = player.GetHighness();
            string sHigh = highness < 0 ? "sober" : "blazed";
            if (Math.Abs(highness) > .5) sHigh = "very " + sHigh;

            double coordination = player.GetCoordination();
            string sCoordination = coordination < 0 ? "clumsy" : "coordinated";
            if (Math.Abs(coordination) > .5) sCoordination = "very " + sCoordination;

            double intelligence = player.GetIntelligence();
            string sIntel = intelligence < 0 ? "dumb" : "smart";
            if (Math.Abs(intelligence) > .5) sIntel = "very " + sIntel;

            double tech = player.GetTechKnowledge();
            string sTech = tech < 0 ? "non-technical" : "technical";
            if (Math.Abs(tech) > .5) sTech = "very " + sTech;

            double stink = player.GetStink();
            string sStink = stink < 0 ? "smelly" : "clean";
            if (Math.Abs(stink) > .5) sStink = "very " + sStink;

            int fingerCount = rdm.Next(100) < 95 ? 10 : 10 + rdm.Next(-2, 2);
            string sFinger = fingerCount.ToString();


            embed.AddField("Main", player.GetMain().GetName());
            if (player.GetSecondary().ID != -1)
                embed.AddField("Secondary", player.GetSecondary().GetName());

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

    }

}