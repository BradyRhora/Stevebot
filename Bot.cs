using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Reflection;
using OpenAI_API;
using Pastel;
using Windows.Media.SpeechRecognition;

namespace Stevebot
{
    class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        public static Random rdm = new Random();

        public static DiscordSocketClient client;
        public static CommandService commands;
        public static OpenAIAPI openapi;

        public static System.Timers.Timer sbpsTimer;
        public async Task Run()
        {
            /*await Task.Run(async()=>
                {
                try
                {
                    var speech = new SpeechRecognizer();
                    await speech.CompileConstraintsAsync();
                    SpeechRecognitionResult result = await speech.RecognizeAsync();
                    Console.WriteLine(result.Text);
                }
                catch { };
            });


            await Task.Delay(-1);*/

            try
            {
                DiscordSocketConfig config = new DiscordSocketConfig() { MessageCacheSize = 1000 };
                Console.WriteLine("Welcome. Initializing Bot...");
                client = new DiscordSocketClient(config);
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in...");
                if (!File.Exists("bottoken"))
                {
                    File.WriteAllText("bottoken", "");
                    Console.WriteLine("Created bottoken file, you will need to put the token in this file.");
                }
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("bottoken"));

                Console.WriteLine("Successfully logged in! Connecting to OpenAI API...");
                Engine td2 = new Engine("text-davinci-002");
                openapi = new OpenAI_API.OpenAIAPI(new APIAuthentication(File.ReadAllText("openaitoken")), engine: td2);

                await client.StartAsync();
                Console.WriteLine($"Connected.\nBot successfully initialized.");

                /*/BuildSBPS();
                Console.WriteLine("Activating SBPS Update Timer..");
                sbpsTimer = new System.Timers.Timer(1000 * 60);
                sbpsTimer.Elapsed += new System.Timers.ElapsedEventHandler(SBPSUpdate);*/

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.Read();
            }
        }
        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.Ready += Ready;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
        }

        private async Task Ready()
        {
            var bbGen = await client.GetChannelAsync(Constants.Channels.BB_GENERAL) as IGuildChannel;
            var gUser = await bbGen.GetUserAsync(client.CurrentUser.Id);
            await gUser.ModifyAsync(x => x.Nickname = gUser.DisplayName.Replace(Constants.Emotes.EAR.Name, ""));
        }

        public static void SBPSUpdate(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (SBPS.isInit())
            {
                SBPS.Update();
            }
        }

        const int LISTEN_CHANCE = 5; //%
        public async Task HandleCommand(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == client.CurrentUser.Id) return; //doesn't allow the bot to respond to itself

            int argPos = 0;
            //detect and execute commands
            if (message.HasCharPrefix('>', ref argPos))
            {
                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services: null);

                if (!result.IsSuccess)
                {
                    if (result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine(result.ErrorReason);
                        await message.Channel.SendMessageAsync("ERROR:\n" + result.ErrorReason);
                    }
                }
            }
            else if (message.MentionedUsers.Select(x=>x.Id).Contains(client.CurrentUser.Id))
            {
                var typingEmote = Emote.Parse("<a:typing:550208612962664448>");
                await message.AddReactionAsync(typingEmote);
                
                string input = message.Content.Replace($"<@{client.CurrentUser.Id}>", "");
                input = Regex.Replace(input, "<a?(:[a-zA-Z_-]+:)[0-9]+>", "$1");

                try
                {
                    var result = await openapi.Completions.CreateCompletionAsync(input, temperature: 0.7, max_tokens: 256);
                    await message.Channel.SendMessageAsync(result.ToString());
                    await message.RemoveReactionAsync(typingEmote, client.CurrentUser);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (Chat.Chats.Select(x => x.channel_id == message.Channel.Id).Count() != 0)
            {
                Chat chat = Chat.Chats.Where(x => x.channel_id == message.Channel.Id).First();

                bool exited = false;
                if (!chat.users.Contains(message.Author.Id))
                    exited = !chat.Join(message.Author);

                if (!exited)
                {
                    var response = await chat.GetNextMessageAsync(message);
                    await message.Channel.SendMessageAsync(response);
                    if (message.Content.ToLower().Contains("goodbye") || message.Content.ToLower().Contains("seeya"))
                        if (chat.Leave(message.Author))
                            await message.Channel.SendMessageAsync("👋");
                }
            }
            else
            {
                if (lastChatCheck < DateTime.Now - new TimeSpan(0,10,0))
                {
                    lastChatCheck = DateTime.Now;
                    int chance = rdm.Next(1000);
                    Console.WriteLine(chance);
                    if (chance <= LISTEN_CHANCE * 10)
                    {
                        var gUser = (message.Channel as SocketGuildChannel).GetUser(client.CurrentUser.Id);
                        await gUser.ModifyAsync(x => x.Nickname = gUser.DisplayName + Constants.Emotes.EAR.Name);
                        Console.WriteLine("I want to join the chat..");
                        Chat chat = new Chat(message.Author.Id,message.Channel.Id, true);
                    }
                }

                
            } 
        }
        public static DateTime lastChatCheck = new DateTime(0);

        async void BuildSBPS()
        {
            /* SERIES GENERATOR   
            Console.WriteLine("Building SBPS Series...");
            int seriesCount = rdm.Next(20, 36);
            string[] genres = {"Fighting","Survival","Horror","RPG","MMORPG","Sandbox","Strategy","RTS","Mobile","Tower Defense","Puzzle","Racing","FPS","Rhythm","Platformer","Education","Exercise"};
            for (int i = 0; i < seriesCount; i++)
            {
                int releaseYear = rdm.Next(1979, 2025);
                string genre = genres[rdm.Next(genres.Count())];
                Console.WriteLine($"{genre} game released in {releaseYear}: ");
                string name = Console.ReadLine();
                var series = new SBPS.Series(name, releaseYear, genre);
                Console.WriteLine("Success. ID: " + series.ID);
            }*/


            /* CHARACTER GENERATOR
            Console.WriteLine("Building SBPS Characters...");
            int charCount = rdm.Next(30, 40);
            for (int i = 0; i < charCount; i++)
            {
                double range = Math.Round((rdm.NextDouble() * 2) - 1,3);
                string sRange = range < 0 ? "Low range" : "High range";
                if (Math.Abs(range) > .5) sRange = "very " + sRange;

                double weight = Math.Round((rdm.NextDouble() * 2) - 1,3);
                string sWeight = weight < 0 ? "Light" : "Heavy";
                if (Math.Abs(weight) > .5) sWeight = "very " + sWeight;

                double power = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sPower = power < 0 ? "Weak" : "Strong";
                if (Math.Abs(power) > .5) sPower = "very " + sPower;

                double speed = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sSpeed = speed < 0 ? "Slow" : "Fast";
                if (Math.Abs(speed) > .5) sSpeed = "very " + sSpeed;

                double wepSize = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sWep = wepSize < 0 ? "Tiny/No Weaponed" : "Big Weaponed";
                if (Math.Abs(wepSize) > .5) sWep = "very " + sWep;

                double sexy = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sSexy = sexy < 0 ? "Ugly" : "Sexy";
                if (Math.Abs(sexy) > .5) sSexy = "very " + sSexy;

                double style = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sStyle = style < 0 ? "Lame" : "Stylish";
                if (Math.Abs(style) > .5) sStyle = "very " + sStyle;

                Color color = new Color(rdm.Next(255), rdm.Next(255), rdm.Next(255));
                SBPS.Series series = SBPS.Series.GetRandom();
                Console.WriteLine($"Coming straight from {series.GetName()}, the {series.GetReleaseYear()} {series.GetGenre()} game.".Pastel(color));
                Console.WriteLine($"RANGE: {range} WEIGHT: {weight} POWER: {power} SPEED: {speed} WEAPON_SIZE: {wepSize} SEXINESS: {sexy} STYLE: {style}".Pastel(color));
                Console.WriteLine($"This {sRange}, {sWeight}, {sPower}, {sSpeed}, {sWep}, {sSexy}, {sStyle} individuals name is:".Pastel(color));
                Console.Write("> ");
                string name = Console.ReadLine();
                if (name != "x")
                {
                    Console.Write("Blurb: ");
                    string blurb = Console.ReadLine();
                    var character = new SBPS.Character(name, range, weight, power, speed, wepSize, sexy, style, color, blurb, series.ID);
                }
                else i--;
            }*/

            //colour fix (disgusting)
            /*
            var chars = SBPS.Character.GetAll();
            foreach(var c in chars)
            {
                var clr = c.GetDBValScalar<string>("colour_hex");
                if (!clr.StartsWith("#"))
                {
                    int r = 0, g = 0, b = 0;
                    int eCount = 0;
                    for (int i = 0; i < clr.Length; i++)
                    {
                        if (clr[i] == '=')
                        {
                            eCount++;

                            if (eCount == 2)
                            {
                                for (int o = i + 1; o < clr.Length; o++)
                                {
                                    if (clr[o] == ',' || clr[o] == ']')
                                    {
                                        var rs = clr.Substring(i + 1, (o - i) - 1);
                                        r = Convert.ToInt32(clr.Substring(i + 1, (o - i) - 1));
                                        break;
                                    }
                                }
                            }
                            else if (eCount == 3)
                            {
                                for (int o = i + 1; o < clr.Length; o++)
                                {
                                    if (clr[o] == ',' || clr[o] == ']')
                                    {
                                        var gs = clr.Substring(i + 1, (o - i) - 1);
                                        g = Convert.ToInt32(clr.Substring(i + 1, (o - i) - 1));
                                        break;
                                    }
                                }
                            }
                            else if (eCount == 4)
                            {
                                for (int o = i + 1; o < clr.Length; o++)
                                {
                                    if (clr[o] == ',' || clr[o] == ']')
                                    {
                                        var bs = clr.Substring(i + 1, (o - i) - 1);
                                        b = Convert.ToInt32(clr.Substring(i + 1, (o - i) - 1));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    string hex = System.Drawing.ColorTranslator.ToHtml(new Color(r, g, b));
                    c.SetDBValue<string>("colour_hex", hex);
                }
            }*/

            /*
            Console.WriteLine("Building SBPS Players...");
            int playerCount = rdm.Next(30, 40);
            for (int i = 0; i < playerCount; i++)
            {
                double weight = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sWeight = weight < 0 ? "Light" : "Heavy";
                if (Math.Abs(weight) > .5) sWeight = "very " + sWeight;

                double charm = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sCharm = charm < 0 ? "Weird" : "Charming";
                if (Math.Abs(charm) > .5) sCharm = "very " + sCharm;

                double anger = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sAnger = anger < 0 ? "Gentle" : "Angry";
                if (Math.Abs(anger) > .5) sAnger = "very " + sAnger;

                double depression = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sDepress = depression < 0 ? "Happy" : "Sad";
                if (Math.Abs(depression) > .5) sDepress = "very " + sDepress;

                double highness = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sHigh = highness < 0 ? "Sober" : "Blazed";
                if (Math.Abs(highness) > .5) sHigh = "very " + sHigh;

                double coordination = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sCoordination = coordination < 0 ? "Clumsy" : "Coordinated";
                if (Math.Abs(coordination) > .5) sCoordination = "very " + sCoordination;

                double intelligence = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sIntel = intelligence < 0 ? "Dumb" : "Smart";
                if (Math.Abs(intelligence) > .5) sIntel = "very " + sIntel;

                double tech = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sTech = tech < 0 ? "non-technical" : "Technical";
                if (Math.Abs(tech) > .5) sTech = "very " + sTech;

                double stink = Math.Round((rdm.NextDouble() * 2) - 1, 3);
                string sStink = stink < 0 ? "Smelly" : "Clean";
                if (Math.Abs(stink) > .5) sStink = "very " + sStink;

                int fingerCount = rdm.Next(100) < 95 ? 10 : 10 + rdm.Next(-2, 2);
                string sFinger = fingerCount + " fingered";

                SBPS.Character main = SBPS.Character.GetRandom();
                SBPS.Character secondary = null;
                if (rdm.Next(100) < 25) secondary = SBPS.Character.GetRandom();

                var color = main.GetColour();
                Console.WriteLine($"WEIGHT: {weight} CHARM: {charm} ANGER: {anger} DEPRESSION: {depression} HIGHNESS: {highness} COORDINATION: {coordination} INTELLIGENCE: {intelligence} TECH_KNOWLEDGE: {tech} STINK: {stink} FINGER COUNT: {fingerCount}".Pastel(color));
                Console.WriteLine($"MAIN: { main.GetName()}".Pastel(color));
                if (secondary != null) Console.WriteLine(("SECONDARY:" + secondary.GetName()).Pastel(secondary.GetColour()));
                Console.WriteLine($"This {sWeight}, {sCharm}, {sAnger}, {sDepress}, {sHigh}, {sCoordination}, {sIntel}, {sTech}, {sStink}, {sFinger} individuals tag is:".Pastel(color));
                Console.Write("> ");
                string tag = Console.ReadLine();
                if (tag != "x")
                {
                    Console.Write("Name: ");
                    string name = Console.ReadLine();
                    var player = new SBPS.Player(name, tag, main.ID, secondary == null ? -1 : secondary.ID, weight, charm, anger, depression, highness, fingerCount, coordination, intelligence, tech, stink);
                }
                else i--;
            }
            

            //*SBPS.Tournament t = new SBPS.Tournament();
            //string bracket = t.BuildChart();
            //Console.WriteLine(bracket.Length);
            var channel = await client.GetChannelAsync(Constants.Channels.HALO_REACH_FRIENDS) as ITextChannel;
            var bracket = t.BuildChart();
            await channel.SendMessageAsync("```"+bracket+"```");
            //await channel.SendMessageAsync("```"+ bracket + "```");*/
        }
    }
}
