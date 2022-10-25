using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Discord;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
// sort out usings

namespace SBPS
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
            if (Tournament.Current == null)
            {
                Tournament.Current = new Tournament();
            }

            Tournament.Current.Update();
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
                        if (val == DBNull.Value || val == null) return default(T);
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
