using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Stevebot;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using System.Threading.Tasks;

namespace SuperBlastPals
{
    partial class SBPS
    {
        static ITextChannel Channel;
        static async Task Announce(string message = "", System.Drawing.Bitmap image = null)
        {
            if (Channel == null)
            {
                Channel = await Bot.client.GetChannelAsync(Constants.Channels.SBPS_TESTING) as ITextChannel;
            }
            if (image == null)
                await Channel.SendMessageAsync(message);
            else {
                System.IO.Stream stream = new System.IO.MemoryStream();
                image.Save(stream,System.Drawing.Imaging.ImageFormat.Jpeg);
                await Channel.SendFileAsync(new FileAttachment(stream, "bracket.jpeg"),message);
            }

        }
        public static async Task Update()
        {
            if (Tournament.Current == null)
            {
                Tournament.Current = new Tournament();
                //await Announce($":sparkles: A new tournament is underway! :sparkles:\n**{Tournament.Current.GetName()}**");
                //await Announce("```"+Tournament.Current.BuildChart()+"```");
            }

            Tournament.Current.Update();
        }

        public abstract class IDataBaseObject
        {
            public string Name { get; set; }
            public int ID { get; set; }
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
                        if (val == DBNull.Value || val == null) return default(T);
                        value = (T)Convert.ChangeType(val, typeof(T));
                    }
                }

                if (EqualityComparer<T>.Default.Equals(value, default))
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

            public string GetName() //dont delete me ######################
            {
                string table = GetTableName();
                var ti = new CultureInfo("en-US", false).TextInfo;
                return ti.ToTitleCase(GetDBValScalar<string>(table, "Name").ToLower());
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
    
        public enum Check_Difficulty
        {
            Easy,
            Medium,
            Hard
        }

        public static Dictionary<Check_Difficulty, int> Difficulty_Chances = new Dictionary<Check_Difficulty, int>()
        {
            {Check_Difficulty.Easy, 30 },
            {Check_Difficulty.Medium, 50 },
            {Check_Difficulty.Hard, 75 }
        };

        public static bool Check(float stat = 1, Check_Difficulty difficulty = Check_Difficulty.Medium)
        {
            var roll = Bot.rdm.Next(0, 100) * stat;
            return roll >= Difficulty_Chances[difficulty];
        }
    }
}
