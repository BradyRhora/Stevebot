namespace SBPS{
        public class Match
        {
            int PointsA = 0;
            int PointsB = 0;
            public Player PlayerA;
            public Player PlayerB;
            public Match NextMatch;
            public Match PrevMatchA;
            public Match PrevMatchB;
            public Tournament Tournament;

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
                if (A)
                {
                    PointsA++;
                    if (PointsA == 2) Win(A);
                }
                else
                {
                    PointsB++;
                    if (PointsB == 2) Win(A);
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

            
            public void Draw(Graphics graphics, Pen pen, Point pos)
            {
                int charLength = 13;
                Font font = new Font(FontFamily.GenericMonospace, Tournament.DRAW_SPACING);
                string nameA = Player.GetName(PlayerA);
                string nameB = Player.GetName(PlayerB);

                PointF topRight = new PointF(pos.X + 175, pos.Y + (Tournament.DRAW_SPACING * 0.7f));
                PointF topLeft = new PointF(pos.X + (charLength * nameA.Length) + 10, pos.Y + (Tournament.DRAW_SPACING * 0.7f));
                int yMod = GetRound();
                PointF bottomRight = new PointF(pos.X + 175, (pos.Y + (Tournament.DRAW_SPACING * 1.7f)) * yMod);
                PointF bottomLeft = new PointF(pos.X + (charLength * nameB.Length) + 10, (pos.Y + (Tournament.DRAW_SPACING * 1.7f)) * yMod);

                // ############################ Draw Self ###############################

                if (PlayerA != null)pen.Color = new Character(PlayerA.GetMainID()).GetColour();
                else pen.Color = Color.White;
                graphics.DrawString(nameA, font, pen.Brush, pos.X, pos.Y);
                graphics.DrawLine(pen, topLeft , topRight);

                pen.Color = Color.White;
                graphics.DrawLine(pen, topRight, bottomRight); // vertical line

                float midY = pos.Y + Tournament.DRAW_SPACING * 1.1f;
                graphics.DrawLine(pen, pos.X + 175, midY, pos.X + 200, midY); // middle horizontal

                if (PlayerB != null) pen.Color = new Character(PlayerB.GetMainID()).GetColour();
                else pen.Color = Color.White;
                graphics.DrawString(nameB, font, pen.Brush, pos.X, pos.Y + (Tournament.DRAW_SPACING * GetRound()));
                graphics.DrawLine(pen, bottomLeft, bottomRight);
                // ######################################################################

                if (NextMatch!=null && NextMatch.PrevMatchA == this)
                    NextMatch.Draw(graphics, pen, new Point(pos.X + 175 + 20, pos.Y + (Tournament.DRAW_SPACING / 2)));
                
                /* ############################ Draw Next ############################### //
                
                string nextName = Player.GetName(NextMatch.PlayerA);
                graphics.DrawString(nextName, font, pen.Brush, pos.X + 175 + 20, pos.Y + (Tournament.DRAW_SPACING / 2));
                graphics.DrawLine(pen, pos.X + 175 + 20 + (charLength * nextName.Length) + 10, midDist, pos.X + 375, midDist);


                /* ###################################################################### */
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
                return Math.Max(DistanceToBottom(), DistanceToBottom(false)) + 1;
            }

            enum PlayerIdentifier{
                A,
                B
            }
        }

}