using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;

namespace BradyMS
{
    partial class BMS
    {
        static string CONN_STRING = Stevebot.Constants.Strings.BMS_CONNECTION_STRING + File.ReadAllText("Files/bmsdb_pwd").Trim('\n');
        public static MySqlConnection DBConn = new MySqlConnection(CONN_STRING);

        public static Character GetCharacter(string name)
        {
            return new Character(name);
        }
    }
}
