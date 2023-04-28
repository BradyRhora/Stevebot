using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BradyMS
{
    partial class BMS
    {
        public class Character
        {
            public static Dictionary<int, string> Jobs = new Dictionary<int, string>()
            {
                { 0, "Beginner" },
                { 100, "Warrior" },
                { 200, "Magician" },
                { 300, "Bowman" },
                { 400, "Thief" },
                { 500, "Pirate" },
                { 1100, "Dawn Warrior" },
                { 1200, "Blaze Wizard" },
                { 1300, "Wind Archer" },
                { 1400, "Night Walker" },
                { 1500, "Thunder Breaker" },
                { 2100, "Aran" },
                { 2200, "Evan" }
            };

            public enum CharacterGender
            {
                Male,
                Female
            }

            public string Name { get; set; }
            public int Level { get; set; }
            public int Mesos { get; set; }
            public int JobID { get; set; }
            public int MapID { get; set; }
            public DateTime CreateDate { get; set; }
            public uint Rank { get; set; }
            public uint JobRank { get; set; }
            public uint GuildID { get; set; }
            public CharacterGender Gender { get; set; }
            public int HP { get; set; }
            public int MaxHP { get; set; }
            public int MP { get; set; }
            public int MaxMP { get; set; }

            public Character(string name)
            {
                BMS.DBConn.Open();
                string sql = "SELECT * FROM characters WHERE name = ?";

                var cmd = new MySqlCommand(sql, BMS.DBConn);
                cmd.Parameters.AddWithValue("param1", name);
                var reader = cmd.ExecuteReader();
                reader.Read();

                Name = reader.GetString("name");
                Level = reader.GetInt32("level");
                Mesos = reader.GetInt32("meso");
                JobID = reader.GetInt32("job");
                MapID = reader.GetInt32("map");
                CreateDate = reader.GetDateTime("createdate");
                Rank = reader.GetUInt32("rank");
                JobRank = reader.GetUInt32("jobRank");
                GuildID = reader.GetUInt32("guildid");
                Gender = reader.GetUInt32("gender") == 0 ? CharacterGender.Male : CharacterGender.Female;
                HP = reader.GetInt32("hp");
                MaxHP = reader.GetInt32("maxhp");
                MP = reader.GetInt32("mp");
                MaxMP = reader.GetInt32("maxmp");
                DBConn.Close();
            }

            public string GetJobNiche()
            {
                var niche = (JobID / 100) % 10;
                switch (niche)
                {
                    case 0: return "Beginner";
                    case 1: return "Warrior";
                    case 2: return "Magician";
                    case 3: return "Bowman";
                    case 4: return "Thief";
                    case 5: return "Pirate";
                }
                return "N/A";
            }
        }
    }
}
