using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Stevebot;

namespace SuperBlastPals
{
    partial class SBPS {
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
                var c = System.Drawing.ColorTranslator.FromHtml(GetDBValScalar<string>("Colour_Hex"));
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
                            chars.Add(new Character(reader.GetInt32(0), true));
                        }
                    }
                }
                return chars.ToArray();
            }
        }  
    }
}