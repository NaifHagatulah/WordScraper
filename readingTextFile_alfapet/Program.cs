using System;
using System.IO;
using System.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Net;
using System.Runtime.Intrinsics.X86;
using CsvHelper;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.Metrics;
using System.Text;

public class ReadFile
{
    public static void Main() 
    {
        string swedish_words = @"C:\Users\elvir\OneDrive\Skrivbord\swe_wordlist.txt";
        string words = File.ReadAllText(swedish_words);
        string[] word_list = words.Split("\n");
        InsertIntoDatabase(word_list);
    }
    
    public static void AddWord(string word) 
    {
        string connectionString;
        NpgsqlConnection cnn;
        connectionString = @"Host=localhost;Port=5432;Username=postgres;Password=Manser1987;Database=postgres;Trust Server Certificate=True";
        //  Data Source = localhost; Initial Catalog = Naif_Bilbo_Adam; User ID = postgres; Password = Manser1987
        string query = "INSERT INTO lexicon(word) VALUES (@word)";

        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        using (NpgsqlCommand command = new NpgsqlCommand (query,connection))
        {
            connection.Open();

            command.Parameters.Add("@word", NpgsqlDbType.Varchar).Value = word;

            command.ExecuteNonQuery();
        }
    }
    
    public static async Task<List<string>> GetValue() 
    {
        string connectionString;
        connectionString = @"Host=localhost;Port=5432;Username=postgres;Password=Manser1987;Database=postgres;Trust Server Certificate=True";
        List<string> result = new List<string>(new string[] {});

        
        string query = @"SELECT ""word"" FROM ""lexicon""";
        
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
        {
            await connection.OpenAsync();
            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                        string word_2 = reader["word"] as string;
                        result.Add(word_2);                         
                }
            }
        }
        return result;
    }

    public static List<string> GetUrlList() 
    {
        //HtmlWeb web = new HtmlWeb();

        int length = 2;
        List<string> url = new List<string>();
        for (int j = 0; j < 28; j++) 
        {
            char letter = 'a';
            for (int i = 0; i < 26; i++)
            {
                 url.Add("https://wordfrauder.se/wordsby?letter=" + letter + "&length=" + length.ToString()); 
                letter++;
                Console.WriteLine(url.ToString());
                //HtmlDocument doc = web.Load("https://wordfrauder.se/wordsby?letter=" + letter + "&length=" + length.ToString());
            }
            length++;
        }
        return url;
    }

    public static List<string> GetAllWords(string url) 
    { 
        List<string> words = new List<string>();
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);
        HtmlNodeCollection wordContainer = doc.DocumentNode.SelectNodes("//div[@class='wh_words']");
        
        if(wordContainer == null) { return words; }

        var titles = new List<Row>();
        foreach (HtmlNode item in wordContainer[0].ChildNodes)
        {
            words.Add(item.InnerText.Split('(')[0].Trim());
        }
        return words;
    }

    public static void ScrapeWords() 
    {
        string swedish_words = @"C:\Users\elvir\OneDrive\Skrivbord\swe_wordlist.txt";
        StringBuilder stringBuilder = new StringBuilder();

        foreach (string url in GetUrlList())
        {
            foreach (string word in GetAllWords(url))
            {
                stringBuilder.AppendLine(word);
            }
        }
        File.WriteAllText(swedish_words, stringBuilder.ToString());
    }

    public static void InsertIntoDatabase(string[] word_list) 
    {
        foreach(var item in word_list) 
        { 
            AddWord(item);
        }

    }

    public class Row 
    {
        public string Title { get; set; }
    }
}
