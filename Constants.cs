using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Stevebot
{
    class Constants
    {
        public class Colours
        {
            public static Color SPOTIFY_GREEN = new Color(8, 195, 103);
            public static Color DEFAULT = new Color();
        }

        public class Users
        {
            public const ulong BRADY = 108312797162541056;
            public const ulong STEVEY = 781380323434561547;
        }

        public class Channels
        {
            public static ulong HALO_REACH_FRIENDS = 234882379880071168;
            public static ulong GENERAL = 195670713183633408;
        }

        public class Strings
        {
            public const string DB_CONNECTION_STRING = @"data source=Resources\blastPals.db";
        }
    }
}
