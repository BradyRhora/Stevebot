using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Stevebot;

namespace SuperBlastPals
{
    partial class SBPS
    {
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
                    for (int i = limit - tag.Length; i > 0; i--) tag += fillChar;
                return tag;
            }

            public int GetMainID()
            {
                return GetDBValScalar<int>("Main_ID");
            }

            public Character GetMain()
            {
                return new Character(GetMainID());
            }

            public int GetSecondaryID()
            {
                return GetDBValScalar<int>("Secondary_ID");
            }

            public Character GetSecondary()
            {
                return new Character(GetSecondaryID());
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

            public static Player[] GetAllShuffled()
            {
                List<Player> chars = new List<Player>();
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Players ORDER BY RANDOM();";
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

            public static Player GetRandom()
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = "SELECT ID FROM Players ORDER BY RANDOM() LIMIT 1;";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Player(id);
                    }
                }
            }

            public static Player Search(string name)
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT ID FROM Players WHERE name like $1 LIMIT 1";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.AddWithValue("$1", '%' + name + '%');
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return new Player(id);
                    }
                }
            }

        }
    }
}