using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Discord;
using System.Data.SQLite;
using System.Drawing;
using System.IO;

namespace Stevebot
{
    class SBPS
    {
        static ulong Channel_ID = 0;
        public static void Initialize(ulong channel_id)
        {
            Channel_ID = channel_id;
        }

        public static bool isInit()
        {
            return Channel_ID != 0;
        }

        public static void Update()
        {

        }

        public class Tournament : IDataBaseObject
        {
            int MatchCount;
            public int Rounds;
            List<Match> Matches = new List<Match>();

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
                GenerateMatches(Player.GetAll());
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
                    Matches[i].GivePoint(true);
                    Matches[i].GivePoint(true);
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
            public Stream DrawChart()
            {
                Bitmap bitmap = new Bitmap(200 * Rounds, 20 + ((DRAW_SPACING * 3) * MatchCount));
                Graphics graphics = Graphics.FromImage(bitmap);

                Pen pen = new Pen(Color.FromKnownColor(KnownColor.White), 2);

                for (int i = 0; i < MatchCount; i++)
                    Matches[i].Draw(graphics, pen, new Point(10,10+((DRAW_SPACING*3) *i)));

                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream;
            }
        }

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

            // TRUE for Player A | FALSE for Player B
            public void GivePoint(bool A)
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

            // TRUE for Player A ┃ FALSE for Player B 
            public void Win(bool A)
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

            public int DistanceToBottom(bool A = true)
            {
                Match curr = this;
                int counter = 0;
                //Console.Write($"Dist to bottom for {PlayerA} vs. {PlayerB}");

                if (A)
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
        }


        public abstract class IDataBaseObject
        {
            public int ID;
            public T GetDBValScalar<T>(string table, string name)
            {
                T value = default;
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT {name} FROM {table} WHERE ID = $1";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", ID));

                        var val = cmd.ExecuteScalar();

                        value = (T)Convert.ChangeType(val, typeof(T));
                    }
                }

                if (EqualityComparer<T>.Default.Equals(value,default))
                {
                    Console.WriteLine($"DB Result {table} {name} {ID} Not found!");
                }
                return value;
            }
            public abstract T GetDBValScalar<T>(string name);

            public bool SetDBValue<T>(string table, string column, T value)
            {
                bool success = false;
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"UPDATE {table} SET {column} = $1 WHERE ID = $2;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", value));
                        cmd.Parameters.Add(new SQLiteParameter("$2", ID));

                        if (cmd.ExecuteNonQuery() > 0) success = true;

                    }
                }
                return success;
            }
            public abstract bool SetDBValue<T>(string column, T value);

            public string GetName()
            {
                string table = GetTableName();
                return GetDBValScalar<string>(table,"Name");
            }

            public static string GetTableName(Type T)
            {
                string table = T.Name;
                if (table.Last() != 's')
                    table += 's';
                return table;
            }

            public string GetTableName()
            {
                string table = GetType().Name;
                if (table.Last() != 's')
                    table += 's';
                return table;
            }
        }

        public class Character : IDataBaseObject
        {

            // Gets Character with id from database
            public Character(int id, bool bypassCheck = false)
            {
                ID = id;
                if (!bypassCheck)
                    if (GetDBValScalar<int>("ID") == default)
                        ID = -1;
            }

            public Character(string name, double range, double weight, double power, double speed, double weapon_size, double sex_appeal, double style, System.Drawing.Color color, string blurb, int seriesID)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query =
                        @"INSERT INTO Characters(
                            Name,Range,Weight,Power,Speed,Weapon_Size,Sex_Appeal,Style,Colour_Hex,Blurb,Series_ID
                        ) VALUES (
                            $1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11
                        )";

                    int rows = 0;
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", name));
                        cmd.Parameters.Add(new SQLiteParameter("$2", range));
                        cmd.Parameters.Add(new SQLiteParameter("$3", weight));
                        cmd.Parameters.Add(new SQLiteParameter("$4", power));
                        cmd.Parameters.Add(new SQLiteParameter("$5", speed));
                        cmd.Parameters.Add(new SQLiteParameter("$6", weapon_size));
                        cmd.Parameters.Add(new SQLiteParameter("$7", sex_appeal));
                        cmd.Parameters.Add(new SQLiteParameter("$8", style));
                        cmd.Parameters.Add(new SQLiteParameter("$9", System.Drawing.ColorTranslator.ToHtml(color)));
                        cmd.Parameters.Add(new SQLiteParameter("$10", blurb));
                        cmd.Parameters.Add(new SQLiteParameter("$11", seriesID));

                        rows = cmd.ExecuteNonQuery();
                    }

                    if (rows == 0) ID = -1;
                    else
                    {
                        var query2 = "SELECT last_insert_rowid()";

                        using (var cmd = new SQLiteCommand(query2, sql))
                        {
                            ID = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
            }

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Characters", name);
            }
            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue<T>("Characters", column, value);
            }

            public string GetBlurb()
            {
                return GetDBValScalar<string>("Blurb");
            }

            public double GetPower()
            {
                return GetDBValScalar<double>("Power");
            }
            public double GetSpeed()
            {
                return GetDBValScalar<double>("Speed");
            }

            public double GetRange()
            {
                return GetDBValScalar<double>("Range");
            }
            public double GetWeight()
            {
                return GetDBValScalar<double>("Weight");
            }

            public double GetWeaponLength()
            {
                return GetDBValScalar<double>("Weapon_Size");
            }
            public double GetSexiness()
            {
                return GetDBValScalar<double>("Sex_Appeal");
            }
            public double GetStyle()
            {
                return GetDBValScalar<double>("Style");
            }

            public Discord.Color GetColour()
            {
                var  c = System.Drawing.ColorTranslator.FromHtml(GetDBValScalar<string>("Colour_Hex"));
                return new Discord.Color(c.R, c.G, c.B);
            }

            public int GetSeriesID()
            {
                return GetDBValScalar<int>("Series_ID");
            }
            public Series GetSeries()
            {
                return new Series(GetSeriesID());
            }

            public static Character Search(string name)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT ID FROM Characters WHERE name like $1 LIMIT 1";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("$1", '%' + name + '%');
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Character(id);
                    }
                }
            }

            public static Character GetRandom()
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Characters ORDER BY RANDOM() LIMIT 1;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Character(id);
                    }
                }
            }

            public static Character[] GetAll()
            {
                List<Character> chars = new List<Character>();
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Characters;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            chars.Add(new Character(reader.GetInt32(0),true));
                        }
                    }
                }
                return chars.ToArray();
            }
        }

        public class Player : IDataBaseObject
        {

            public Player(int id, bool bypassCheck = false)
            {
                ID = id;
                if (!bypassCheck && GetDBValScalar<int>("ID") == default)
                    ID = -1;
            }

            public Player(string name, string tag, int mainID, int secID, double weight, double charm, double anger, double depression, double highness, int fingerCount, double coordination, double intelligence, double tech, double stink)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query =
                        @"INSERT INTO Players(
                            Name,Tag,Main_ID,Secondary_ID,Weight,Charm,Anger,Depression,Highness,Finger_Count,Coordination,Intelligence,Tech_Knowledge,Stink
                        ) VALUES (
                            $1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,$13,$14
                        )";

                    int rows = 0;
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", name));
                        cmd.Parameters.Add(new SQLiteParameter("$2", tag));
                        cmd.Parameters.Add(new SQLiteParameter("$3", mainID));
                        if (secID != -1)
                            cmd.Parameters.Add(new SQLiteParameter("$4", secID));
                        else
                            cmd.Parameters.Add(new SQLiteParameter("$4", null));
                        cmd.Parameters.Add(new SQLiteParameter("$5", weight));
                        cmd.Parameters.Add(new SQLiteParameter("$6", charm));
                        cmd.Parameters.Add(new SQLiteParameter("$7", anger));
                        cmd.Parameters.Add(new SQLiteParameter("$8", depression));
                        cmd.Parameters.Add(new SQLiteParameter("$9", highness));
                        cmd.Parameters.Add(new SQLiteParameter("$10", fingerCount));
                        cmd.Parameters.Add(new SQLiteParameter("$11", coordination));
                        cmd.Parameters.Add(new SQLiteParameter("$12", intelligence));
                        cmd.Parameters.Add(new SQLiteParameter("$13", tech));
                        cmd.Parameters.Add(new SQLiteParameter("$14", stink));

                        rows = cmd.ExecuteNonQuery();
                    }

                    if (rows == 0) ID = -1;
                    else
                    {
                        var query2 = "SELECT last_insert_rowid()";

                        using (var cmd = new SQLiteCommand(query2, sql))
                        {
                            ID = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                }
            }

            public static string GetName(Player player, int limit = 0)
            {
                if (player == null) return "[        ]";
                else
                {
                    string name = player.ToString();
                    if (limit != 0 && name.Length > limit)
                        name = name.Substring(0, limit - 2) + "..";
                    return name;
                }
            }

            public override string ToString()
            {
                string ret = "";
                //if () get team abbr
                ret += GetTag();
                return ret;
            }
            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue("Players", column, value);
            }
            public string GetTag(int limit = 0, bool fillEmpty = true, char fillChar = ' ')
            {
                string tag = GetDBValScalar<string>("Tag");
                if (limit != 0 && tag.Length > limit)
                    tag = tag.Substring(0, limit - 2) + "..";
                if (limit != 0 && tag.Length < limit)
                    for (int i = limit - tag.Length ; i > 0; i--) tag += fillChar;
                return tag;
            }

            public int GetMainID()
            {
                return GetDBValScalar<int>("Main_ID");
            }

            public int GetSecondaryID()
            {
                return GetDBValScalar<int>("Secondary_ID");
            }

            public double GetWeight()
            {
                return GetDBValScalar<double>("Weight");
            }

            public double GetCharm()
            {
                return GetDBValScalar<double>("Charm");
            }

            public double GetAnger()
            {
                return GetDBValScalar<double>("Anger");
            }

            public double GetDepression()
            {
                return GetDBValScalar<double>("Depression");
            }

            public double GetHighness()
            {
                return GetDBValScalar<double>("Highness");
            }

            public double GetCoordination()
            {
                return GetDBValScalar<double>("Coordination");
            }

            public double GetIntelligence()
            {
                return GetDBValScalar<double>("Intelligence");
            }

            public double GetTechKnowledge()
            {
                return GetDBValScalar<double>("Tech_Knowledge");
            }

            public double GetStink()
            {
                return GetDBValScalar<double>("Stink");
            }

            public int GetFinger_Count()
            {
                return GetDBValScalar<int>("Finger_Count");
            }

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Players", name);
            }

            public static Player[] GetAll()
            {
                List<Player> chars = new List<Player>();
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Players;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            chars.Add(new Player(reader.GetInt32(0), true));
                        }
                    }
                }
                return chars.ToArray();
            }
        }

        public class Series : IDataBaseObject
        {
            public Series(int id)
            {
                ID = id;
                if (GetDBValScalar<int>("ID") == default)
                    ID = -1;
            }

            public Series(string name, int releaseYear, string genre)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query =
                        @"INSERT INTO Series(
                            Name,Release_Year,Genre
                        ) VALUES (
                            $1,$2,$3
                        )";

                    int rows = 0;
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", name));
                        cmd.Parameters.Add(new SQLiteParameter("$2", releaseYear));
                        cmd.Parameters.Add(new SQLiteParameter("$3", genre));

                        rows = cmd.ExecuteNonQuery();
                    }

                    if (rows == 0) ID = -1;
                    else
                    {
                        var query2 = "SELECT last_insert_rowid()";

                        using (var cmd = new SQLiteCommand(query2, sql))
                        {
                            var obj = cmd.ExecuteScalar();
                            ID = Convert.ToInt32(obj);
                        }
                    }
                }
            }

            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue<T>("Series", column, value);
            }

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Series", name);
            }

            public int GetReleaseYear()
            {
                return GetDBValScalar<int>("Release_Year");
            }

            public string GetGenre()
            {
                return GetDBValScalar<string>("Genre");
            }

            public static Series GetRandom()
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Series ORDER BY RANDOM() LIMIT 1;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Series(id);
                    }
                }
            }

            public static Series Search(string name)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT ID FROM Series WHERE name like $1 LIMIT 1";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("$1", '%' + name + '%');
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Series(id);
                    }
                }
            }

            public Character[] GetCharacters()
            {
                var chars = new List<Character>();
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT ID FROM CHARACTERS WHERE SERIES_ID = $id";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("$id", ID);

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                            chars.Add(new Character(reader.GetInt32(0)));
                        
                    }
                }
                return chars.ToArray();
            }
        }

        public class Team : IDataBaseObject
        {
            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Teams", name);
            }
            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue<T>("Teams", column, value);
            }
        }
    }
}
