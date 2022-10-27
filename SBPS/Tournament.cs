using System;
using System.Collections.Generic;
using System.Linq;

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

            //string tourney name? three word generation, prefix - main - suffix (suffix optional (50%?))
            //string location?

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Tournaments", name);
            }

            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue<T>("Tournaments", column, value);
            }

            public Tournament()
            {
                GenerateMatches(Player.GetAllShuffled());
            }

            public void Update()
            {
                foreach (var match in Matches)
                {
                    match.Update();
                }
            }

            void GenerateMatches(Player[] ps)
            {
                MatchCount = (ps.Length / 2) + ps.Length % 2;
                for (int i = 0; i < MatchCount; i++)
                {
                    Player player2;
                    if ((i * 2) + 1 >= ps.Length) player2 = null;
                    else player2 = ps[(i * 2) + 1];
                    Match m = new Match(ps[i * 2], player2, this);
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

                //TEMP_UPDATE_METHOD();
                //TEMP_PRINT_ALL();
                //TEMP_INTERACTIVE_BRACKET();
            }

            void TEMP_UPDATE_METHOD()
            {
                for (int i = 0; i < MatchCount; i++)
                {
                    Matches[i].GivePoint(Match.PlayerIdentifier.A);
                    Matches[i].GivePoint(Match.PlayerIdentifier.A);
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
                while (flag)
                {
                    Console.Clear();
                    string br = "";
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


                    Console.Write("4. Quit\n> ");
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

            public static int DRAW_SPACING = 15;
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
        }
    }
}