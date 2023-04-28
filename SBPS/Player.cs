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
            public string Tag {get; private set; }
            public float Weight {get; private set; }
            public float Charm {get; private set; }
            public float Anger {get; private set; }
            public float Depression {get; private set; }
            public float Highness {get; private set; }
            public float Coordination {get; private set; }
            public float Intelligence {get; private set; }
            public float Tech_Knowledge {get; private set; }
            public float Stink {get; private set; }
            public int MainID {get; private set; }
            public int SecondaryID {get; private set; }
            public int Finger_Count { get; private set; }
            public int Team_ID { get; private set; }

            public Player() { }

            public Player(int id, bool bypassCheck = false)
            {
                ID = id;
                if (!bypassCheck && GetDBValScalar<int>("ID") == default)
                    ID = -1;
                else
                    LoadData();
            }

            void LoadData() // can we make this less redundant for all SBPS types? (match, character, etc...) 
            {
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT Name,Tag,Main_ID,Secondary_ID,Weight,Charm,Anger,Depression,Highness,Finger_Count,Coordination,Intelligence,Tech_Knowledge,Stink FROM PLAYERS WHERE ID = $1";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("$1", ID));

                        var reader = cmd.ExecuteReader();
                        reader.Read();

                        Name = reader.GetString(0);
                        Tag = reader.GetString(1);
                        MainID = reader.GetInt32(2);
                        object sec = reader.GetValue(3);
                        if (sec.GetType() != typeof(DBNull))
                            SecondaryID = reader.GetInt32(3);
                        
                        Weight = reader.GetFloat(4);
                        Charm = reader.GetFloat(5);
                        Anger = reader.GetFloat(6);
                        Depression = reader.GetFloat(7);
                        Highness = reader.GetFloat(8);
                        Finger_Count = reader.GetInt32(9);
                        Coordination = reader.GetFloat(10);
                        Intelligence = reader.GetFloat(11);
                        Tech_Knowledge = reader.GetFloat(12);
                        Stink = reader.GetFloat(13);
                    }
                }
            }

            public Player(string name, string tag, int mainID, int secID, float weight, float charm, float anger, float depression, float highness, int fingerCount, float coordination, float intelligence, float tech, float stink)
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

            public static string GetBracketName(Player player, int limit = 0)
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
                ret += Tag;
                return ret;
            }
            public override bool SetDBValue<T>(string column, T value)
            {
                return SetDBValue("Players", column, value);
            }

            public override T GetDBValScalar<T>(string name)
            {
                return GetDBValScalar<T>("Players", name);
            }

            public string GetTag(int limit = 0, bool fillEmpty = true, char fillChar = ' ')
            {
                if (limit != 0 && Tag.Length > limit)
                    Tag = Tag.Substring(0, limit - 2) + "..";
                if (limit != 0 && Tag.Length < limit)
                    for (int i = limit - Tag.Length; i > 0; i--) Tag += fillChar;
                return Tag;
            }

            public Character GetMain()
            {
                return new Character(MainID);
            }

            public Character GetSecondary()
            {
                return new Character(SecondaryID);
            }

            public static Player[] GetAll() //********************** test me
            {
                List<Player> players = new List<Player>();
                using (var sql = new SQLiteConnection(Constants.Strings.DB_CONNECTION_STRING))
                {
                    sql.Open();
                    var query = $"SELECT ID,Name,Tag,Main_ID,Secondary_ID,Weight,Charm,Anger,Depression,Highness,Finger_Count,Coordination,Intelligence,Tech_Knowledge,Stink,Team_ID FROM PLAYERS";
                    using (var cmd = new SQLiteCommand(query, sql))
                    {

                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            Player p = new Player();
                            p.ID = reader.GetInt32(0);
                            p.Name = reader.GetString(1);
                            p.Tag = reader.GetString(2);
                            p.MainID = reader.GetInt32(3);

                            object sec = reader.GetValue(4);
                            if (sec.GetType() != typeof(DBNull))
                                p.SecondaryID = reader.GetInt32(3);

                            p.Weight = reader.GetFloat(5);
                            p.Charm = reader.GetFloat(6);
                            p.Anger = reader.GetFloat(7);
                            p.Depression = reader.GetFloat(8);
                            p.Highness = reader.GetFloat(9);
                            p.Finger_Count = reader.GetInt32(10);
                            p.Coordination = reader.GetFloat(11);
                            p.Intelligence = reader.GetFloat(12);
                            p.Tech_Knowledge = reader.GetFloat(13);
                            p.Stink = reader.GetFloat(14);
                            p.Team_ID = reader.GetInt32(15);
                            players.Add(p);
                        }
                    }
                }
            
                return players.ToArray();
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
                    var query = $"SELECT ID FROM Players WHERE name like $1 OR tag LIKE $1 LIMIT 1";
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