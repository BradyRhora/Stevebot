using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Stevebot;

namespace SuperBlastPals
{
    partial class SBPS
    {
       
        public class Match : IDataBaseObject // probably missing overrides
        {
            int PointsA = 0;
            int PointsB = 0;
            public Player PlayerA;
            public Player PlayerB;
            public Match NextMatch;
            public Match PrevMatchA;
            public Match PrevMatchB;
            public Tournament Tournament;

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Matches", name);
            }

            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue("Matches", column, value);
            }

            public Match(Player a, Player b, Tournament tournament)
            {
                PlayerA = a;
                PlayerB = b;

                PrevMatchA = null;
                PrevMatchB = null;

                Tournament = tournament;
            }

            public Match(Match a, Match b)
            {
                PrevMatchA = a;
                PrevMatchB = b;

                a.NextMatch = this;
                b.NextMatch = this;

                Tournament = a.Tournament;
            }

            public void Update()
            {
                if (PlayerA != null && PlayerB != null)
                {
                    int aPoints = 0;
                    int bPoints = 0;

                    

                    //choose stage?? should i make stages??????
                    /*
                        ok wait seriously how are we gonna do this
                        compare characters pros and cons with eachother
                        check player stats and how they flow with character stats
                        randomness
                        ok sounds good

                        separate class for match contender???
                            holds current character
                            match-only modifiers?
                            etc?????
                    */

                }
            }

            public void GivePoint(PlayerIdentifier p)
            {
                if (p == PlayerIdentifier.A)
                {
                    PointsA++;
                    if (PointsA == 2) Win(PlayerIdentifier.A);
                }
                else
                {
                    PointsB++;
                    if (PointsB == 2) Win(PlayerIdentifier.B);
                }
            }

            public void Win(PlayerIdentifier p)
            {

            }

            public static string short_tab_string = "\t\t";
            public static string tab_string = "\t\t";
            public static int depth = 0;
            public const int BRACKET_NAME_LIMIT = 10;
            public string Build(ref string str)
            {
                depth += 1;
                if (str == null) str = "";
                if (PrevMatchA != null)
                {
                    PrevMatchA.Build(ref str);
                    str += '\n';
                }
                else
                    str += PlayerA.GetTag(BRACKET_NAME_LIMIT) + " ━━━━━┓\n";

                if (NextMatch != null)
                {
                    int round = GetRound();
                    int pRound = NextMatch.GetRound();

                    string tab = "";
                    for (int i = 0; i < round; i++) tab += "                ";
                    if (round != 1) tab += "";
                    tab += "┣━";

                    int tabsToUse = pRound - round - 1;

                    for (int i = 0; i < tabsToUse; i++) tab += "━━━━━━━━━━━━━━━━";

                    string nam = "[        ]";
                    if (NextMatch.PrevMatchA == this)
                    {
                        if (NextMatch.PlayerA != null) nam = NextMatch.PlayerA.GetTag(BRACKET_NAME_LIMIT);
                        str += $"{tab} {nam} ━━┓\n";
                    }
                    else
                    {
                        if (NextMatch.PlayerB != null) nam = NextMatch.PlayerB.GetTag(BRACKET_NAME_LIMIT);
                        str += $"{tab} {nam} ━━┛\n";
                    }
                }

                if (PrevMatchB != null)
                    PrevMatchB.Build(ref str);
                else
                    str += PlayerB.GetTag(BRACKET_NAME_LIMIT) + " ━━━━━┛";
                depth -= 1;

                return str;
            }


            public int DistanceToBottom(PlayerIdentifier p)
            {
                Match curr = this;
                int counter = 0;
                //Console.Write($"Dist to bottom for {PlayerA} vs. {PlayerB}");

                if (p == PlayerIdentifier.A)
                    while (curr.PrevMatchA != null)
                    {
                        counter++;
                        curr = curr.PrevMatchA;
                    }
                else
                    while (curr.PrevMatchB != null)
                    {
                        counter++;
                        curr = curr.PrevMatchB;
                    }
                //Console.WriteLine(": " + counter);
                return counter;
            }

            public int GetRound()
            {
                return Math.Max(DistanceToBottom(PlayerIdentifier.A), DistanceToBottom(PlayerIdentifier.B)) + 1;
            }

            public enum PlayerIdentifier {
                A,
                B
            }
        }
    }
}