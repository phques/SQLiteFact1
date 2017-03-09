using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;

namespace SQLiteFact1
{
    public class Language
    {
        public int Id { get; set; }
        public string LangTitle { get; set; }
    }

    public class Toto
    {
        //
        string connectionString = @"Data Source=langDB.sqlite; Version=3; FailIfMissing=True; Foreign Keys=True;";
        DbProviderFactory dbProviderFactory = null;

        public Toto()
        {
            //dbProviderFactory = SQLiteFactory.Instance;

            // throws if not found, need to add entry in app.config,
            // might need to use this when provider is not known / taken from config
            dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SQLite");

        }

        public int CreateLangTable()
        {
            int result = -1;
            using (DbConnection conn = dbProviderFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                using (DbCommand cmd = dbProviderFactory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"CREATE TABLE Language ( Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,";
                    cmd.CommandText += @"LangTitle TEXT NOT NULL UNIQUE CHECK (LangTitle <> '') )";
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (DbException)
                    {
                        //...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public int AddLang(string langTitle)
        {
            int result = -1;
            using (DbConnection conn = dbProviderFactory.CreateConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                using (DbCommand cmd = dbProviderFactory.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO Language(LangTitle) VALUES (@Lang)";
                    cmd.Prepare();
                    DbParameter param = dbProviderFactory.CreateParameter();
                    param.ParameterName = "@Lang";
                    param.Value = langTitle;
                    cmd.Parameters.Add(param);
                    try
                    {
                        result = cmd.ExecuteNonQuery();
                    }
                    catch (DbException)
                    {
                        //...
                    }
                }
                conn.Close();
            }
            return result;
        }

        public List<Language> GetLanguages(int langId)
        {
            List<Language> langs = new List<Language>();
            try
            {
                using (DbConnection conn = dbProviderFactory.CreateConnection())
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();
                    string sql = "SELECT * FROM Language WHERE Id = " + langId;
                    if (langId == 0)
                    {
                        sql = "SELECT * FROM Language";
                    }
                    using (DbCommand cmd = dbProviderFactory.CreateCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Language la = new Language()
                                {
                                    LangTitle = reader["LangTitle"].ToString(),
                                    Id = Int32.Parse(reader["Id"].ToString())
                                };
                                langs.Add(la);
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (DbException)
            {
                //...
            }

            return langs;
        }
    }


    class Program
    {

        static void Main(string[] args)
        {
            // (re)create the dtabase from scratch
            SQLiteConnection.CreateFile("langDB.sqlite");

            var toto = new Toto();
            toto.CreateLangTable();

            toto.AddLang("french");      // 1
            toto.AddLang("english");     // 2
            toto.AddLang("deutch");      // 3
            toto.AddLang("ststh");       // 4

            var lls = toto.GetLanguages(0);
            foreach (var lang in lls)
            {
                Console.WriteLine("{0} {1}", lang.Id, lang.LangTitle);
            }
        }
    }
}
