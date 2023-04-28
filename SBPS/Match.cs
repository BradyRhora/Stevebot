using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Stevebot;
using System.Drawing;

namespace SuperBlastPals
{
    partial class SBPS
    {
       
        public class Match : IDataBaseObject
        {
            public MatchContender PlayerA;
            public MatchContender PlayerB;
            public Match NextMatch;
            public Match PrevMatchA;
            public Match PrevMatchB;
            public Tournament Tournament;
            public float Timer = 8; // 1 represents a minute, so .5 is 30 "seconds"

            bool done = false;

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
                PlayerA = new MatchContender(a, this);
                PlayerB = new MatchContender(b, this);

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
                    if (Contest(PlayerA.Character.Speed, PlayerB.Character.Speed))
                        PlayerA.Update(PlayerB);
                    else
                        PlayerB.Update(PlayerA);


                    


                }
            }

            public void Win(MatchContender p)
            {
                done = true;                
                if (NextMatch != null)
                {
                    if (NextMatch.PrevMatchA == this)
                        NextMatch.PlayerA = new MatchContender(p,NextMatch);
                    else
                        NextMatch.PlayerB = new MatchContender(p, NextMatch);
                }
            }

            public static string short_tab_string = "\t\t";
            public static string tab_string = "\t\t";
            public static int depth = 0;
            public const int BRACKET_NAME_LIMIT = 10;
            
            public string Build(ref string str, int min_round)
            {
                depth += 1;

                if (GetRound() < min_round) return str;


                if (str == null) str = "";
                if (PrevMatchA != null)
                {
                    PrevMatchA.Build(ref str, min_round);                                     // ####################### GO INTO PREVIOUS MATCH A
                    str += '\n';
                }
                else
                    str += PlayerA.GetTag(BRACKET_NAME_LIMIT) + $"[{PlayerA.Wins}]" + " ━━━━━┓\n";            // ####################### DISPLAY PLAYER A

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
                        str += $"{tab} {nam} ━━┓\n";                                    // ####################### DISPLAY NEXT PLAYER A
                    }
                    else
                    {
                        if (NextMatch.PlayerB != null) nam = NextMatch.PlayerB.GetTag(BRACKET_NAME_LIMIT);
                        str += $"{tab} {nam} ━━┛\n";                                    // ####################### DISPLAY NEXT PLAYER B
                    }
                }

                if (PrevMatchB != null)
                    PrevMatchB.Build(ref str, min_round);                                     // ####################### GO INTO PREVIOUS MATCH B
                else
                    str += PlayerB.GetTag(BRACKET_NAME_LIMIT) + $"[{PlayerA.Wins}]" + " ━━━━━┛";              // ####################### DISPLAY PLAYER B
                depth -= 1;

                return str;
            }

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

            const int xChange = 250;
            public void Draw(ref Graphics g, Pen p, Font f, PointF point)
            {
                int halfText = (int)(f.Size / 2);

                int dist = (int)Math.Pow(GetRound(),3.5)+15;

                var aOffset = point - new Size(0, dist);
                var bOffset = point + new Size(0, dist);

                string nameA = Player.GetBracketName(PlayerA, 10);
                g.DrawString(nameA, f, p.Brush, aOffset);
                g.DrawLine(p,aOffset + new Size(nameA.Length * 16, halfText), aOffset + new Size(xChange-5, halfText)); // top horizontal line

                string nameB = Player.GetBracketName(PlayerB, 10);
                g.DrawString(nameB, f, p.Brush, bOffset);
                g.DrawLine(p, bOffset + new Size(nameB.Length * 16, halfText), bOffset + new Size(xChange-5, halfText)); // bottom horizontal line

                g.DrawLine(p, aOffset + new Size(xChange-5, halfText), bOffset + new Size(xChange-5, halfText));    // vertical line
                g.DrawLine(p, aOffset + new Size(xChange-5, halfText) + new Size(0, dist), aOffset + new Size(xChange - 5, halfText) + new Size(5, dist));                            // pointer to right name


                if (PrevMatchA != null)
                {
                    PointF mid = aOffset - new Size(xChange, -halfText);
                    PrevMatchA.Draw(ref g, p, f, mid);
                }

                if (PrevMatchB != null)
                {
                    PointF mid = bOffset - new Size(xChange, halfText);
                    PrevMatchB.Draw(ref g, p, f, mid);
                }
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

            public bool HasStarted()
            {
                return PlayerA != null && PlayerB != null;
            }

            public bool HasFinished()
            {
                return done;
            }

            public static bool Contest(float playerA_stat, float playerB_stat)
            {
                var resA = Bot.rdm.Next(100) * playerA_stat;
                var resB = Bot.rdm.Next(100) * playerB_stat;

                if (resA > resB) return true;
                else return false;
            }

            public enum PlayerIdentifier {
                A,
                B,
                None
            }
        }
    
        public class MatchContender : Player
        {
            public Character Character { get; }

            public Player_Stance Stance { get; set; } = Player_Stance.Defense;
            public Player_Location Location { get; set; } = Player_Location.Ground;
            public float Damage { get; } = 0;
            public float Shield { get; } = 1.0f;
            public int Stocks { get; } = 3;
            public int Wins { get; private set; } = 0;
            public Match Match { get; }

            public MatchContender(int id, Match match) : base(id) { Match = match; }

            public MatchContender(Player p, Match match) : base(p.ID) { Match = match; }

            public void GivePoint()
            {
                if (++Wins == 2)
                    Match.Win(this);                
            }

            public void Update(MatchContender opponent)
            {
                if (Stance == Player_Stance.Defense)
                {
                    if (opponent.Stance == Player_Stance.Defense)
                    {
                        // start of match? on respawn? both players acting defensive 

                        // players can try to become offensive or stall ?if intelligent?

                        if (Match.Contest(Coordination, opponent.Coordination))
                        {
                            Stance = Player_Stance.Attack;
                            // can Match.contest OR (SBPS.)check dont forget
                        }
                    }
                }
                else if (Stance == Player_Stance.Attack)
                {
                    if (opponent.Stance == Player_Stance.Attack)
                    {
                        bool faster = Match.Contest(Character.Speed, opponent.Character.Speed);

                        if (faster)
                        {
                            Attack(opponent);
                        }
                        // both players approaching/attacking eachother

                        // compete with speed, tech_knowledge, intelligence?
                    }
                }


                //choose stage?? should i make stages??????
                /*
                    ok wait seriously how are we gonna do this
                    compare characters pros and cons with eachother
                    check player stats and how they flow with character stats
                    randomness
                    ok sounds good

                    hold on thats not enough we need a whole simulation of this dang match
                        - combos
                        - spikes
                        - damage
                */

                /* Player stats to use: 
                 * Weight - chair breaking, cant move for/to next match
                 * Charm - persuade opponent?, persuade TO?
                 * Anger - rage, controller smash, punch opponent???
                 * Depression - give up, cry
                 * Highness - 
                 * 
                 * Coordination - knows when to do what, can combo
                 * Intelligence - better and choosing smart decisions
                 * Tech_Knowledge - better at playing/controlling
                 * Stink - can negatively impact opponents but too much of it could result in a ban
                 * Finger_Count - having less than 10 can negatively impact, having more..?
                 */

                /* Character stats to use:
                 * Power
                 * Speed
                 * Weight
                 * Range
                 * Weapon_Length
                 * Sexiness
                 * Style
                 */
            }

            void Attack(MatchContender target)
            {
                // combo counter?
                Match.Contest(Character.Range, target.Tech_Knowledge);
            }

            public enum Player_Stance
            {
                Attack,
                Defense,
                Launched
            }

            public enum Player_Location
            {
                Ground,
                Air,
                Platform,
                Ledge,
                Offstage
            }

        }
    }
}