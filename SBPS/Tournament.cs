using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Stevebot;
using System.Drawing;

namespace SuperBlastPals
{
    partial class SBPS
    {
        public class Tournament : IDataBaseObject
        {
            int MatchCount;
            public int Rounds;
            List<Match> Matches = new List<Match>();
            public static Tournament Current = null;
            public string Name { get; set; }
            //string location?

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Tournaments", name);
            }

            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue<T>("Tournaments", column, value);
            }

            public Tournament(int id)
            {
                ID = id;
            }

            public Tournament()
            {
                GenerateName();
                GenerateMatches(Player.GetAllShuffled());
                TEMP_UPDATE_METHOD();
                //TEMP_UPDATE_METHOD();
                Announce(image:NewChart());
                //Announce("```" +BuildChart() + "```");
            }

            public void Update()
            {
                foreach (var match in Matches)
                {
                    match.Update();
                }
            }

            public const int POST_CHANCE = 50;
            string GenerateName()
            {
                var contents = File.ReadAllLines("Files/tourney_words.txt");
                string[] pres = contents[0].Split(',');
                string[] mains = contents[1].Split(',');
                string[] posts = contents[2].Split(',');
                
                string pre = pres[Bot.rdm.Next(pres.Count())];
                string main = mains[Bot.rdm.Next(mains.Count())];
                string post = posts[Bot.rdm.Next(posts.Count())];

                int mode = Bot.rdm.Next(100);
                Name = pre;
                if (mode < 25) Name += " " + main;
                else if (mode < 50) Name += " " + post;
                else Name += " " + main + " " + post;

                var match = Regex.Match(Name, "\\[(.*)\\]");
                if (match.Success)
                {
                    string replacement;
                    switch (match.Captures[0].Value.Trim('[',']'))
                    {
                        case "year":
                            replacement = DateTime.Now.Year.ToString();
                            break;
                        default:
                            replacement = "";
                            break;
                    }
                    Name = Regex.Replace(Name, "\\[(.*)\\]", replacement);
                }

                return Name;
            }

            void GenerateMatches(Player[] entrants)
            {
                MatchCount = (entrants.Length / 2) + entrants.Length % 2;
                for (int i = 0; i < MatchCount; i++)
                {
                    Player player2;
                    if ((i * 2) + 1 >= entrants.Length) player2 = null;
                    else player2 = entrants[(i * 2) + 1];
                    Match m = new Match(entrants[i * 2], player2, this);
                    Matches.Add(m);
                }



                int amount = Matches.Count;
                int start = Matches.Count - amount;
                Rounds = 2;
                while (amount != 1)
                {
                    for (int i = start; i < start + amount; i += 2)
                    {
                        Matches.Add(new Match(Matches[i], Matches[i + 1]));
                    }

                    amount /= 2;
                    start = Matches.Count - amount;
                    Rounds++;
                }

            }

            void TEMP_UPDATE_METHOD()
            {
                for (int i = Matches.Count() - 1; i >= 0; i--)
                {
                    if (Matches[i].HasStarted() && !Matches[i].HasFinished())
                    {
                        Matches[i].PlayerA.GivePoint();
                        Matches[i].PlayerA.GivePoint();
                    }
                }
            }

            void TEMP_PRINT_ALL()
            {
                Console.WriteLine("ALL MATCHES:");
                foreach (var m in Matches)
                {
                    Console.WriteLine($"{m.PlayerA} vs. {m.PlayerB}");
                }
                Console.WriteLine("END OF MATCHES.");
            }

            void TEMP_INTERACTIVE_BRACKET()
            {
                Match current = Matches[Matches.Count - 1];
                bool flag = true;
                bool sv = true;
                while (flag)
                {
                    Console.Clear();
                    string br = "";
                    if (sv)
                        current.Build(ref br, GetMinRound());
                    else
                        current.Build(ref br);

                    Console.WriteLine(br);
                    Console.WriteLine("==================================================");

                    Console.WriteLine($"Current match: {current.PlayerA} vs. {current.PlayerB}:");

                    Console.WriteLine("\nPick an option:\n");
                    if (current.NextMatch != null)
                        Console.WriteLine($"1. Go to Next: {current.NextMatch.PlayerA} vs. {current.NextMatch.PlayerB}");
                    else
                        Console.WriteLine("No next.");

                    if (current.PrevMatchA != null)
                        Console.WriteLine($"2. Go to Previous Match A: {current.PrevMatchA.PlayerA} vs. {current.PrevMatchA.PlayerB}");
                    else
                        Console.WriteLine("No Previous Match A");

                    if (current.PrevMatchB != null)
                        Console.WriteLine($"3. Go to Previous Match B: {current.PrevMatchB.PlayerA} vs. {current.PrevMatchB.PlayerB}");
                    else
                        Console.WriteLine("No Previous Match B");

                    Console.WriteLine($"4: Toggle Short view [{(sv ? "On" : "Off")}]");
                    Console.Write("5. Quit\n> ");
                    char c = (char)Console.Read();
                    Console.WriteLine();
                    switch (c)
                    {
                        case '1':
                            current = current.NextMatch;
                            break;
                        case '2':
                            current = current.PrevMatchA;
                            break;
                        case '3':
                            current = current.PrevMatchB;
                            break;
                        case '4':
                            sv = !sv;
                            break;
                        case '5':
                            flag = false;
                            break;
                    }
                }
            }

            public string BuildChart()
            {
                string chart = "";
                Matches[Matches.Count - 1].Build(ref chart);
                return chart;
            }

            public Bitmap NewChart()
            {
                Console.WriteLine(BuildChart());
                int width = 125 * Rounds;
                int height = (DRAW_SPACING) * MatchCount;
                width *= 2;
                height *= 2;
                Bitmap bm = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bm);
                Pen pen = new Pen(Color.FromKnownColor(KnownColor.White));
                Font font = new Font(FontFamily.GenericMonospace, 20);
                PointF point = new PointF(width-200, height/2);

                var root = Matches[Matches.Count - 1];
                root.Draw(ref g, pen, font, point);








                return bm;
            }

            public IGrouping<int,Match>[] GetMatchesByRound()
            {
                return Matches.GroupBy(x => x.GetRound()).ToArray();
            }

            public static int DRAW_SPACING = 40;
            /*public Stream DrawChart()
            {
                Bitmap bitmap = new Bitmap(200 * Rounds, 20 + ((DRAW_SPACING * 3) * MatchCount));
                Graphics graphics = Graphics.FromImage(bitmap);

                Pen pen = new Pen(Color.FromKnownColor(KnownColor.White), 2);

                for (int i = 0; i < MatchCount; i++)
                    Matches[i].Draw(graphics, pen, new Point(10,10+((DRAW_SPACING*3) *i)));

                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream;
            }*/

            public int GetMinRound()
            {
                for (int i = 0; i < Matches.Count; i++)
                    if (!Matches[i].HasFinished()) 
                        return Matches[i].GetRound();
                return Rounds;
            }
        }
    }
}