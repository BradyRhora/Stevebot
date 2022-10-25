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
