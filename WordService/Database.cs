using Microsoft.Data.SqlClient;

namespace WordService;

public class Database
{
    private static Database instance = new Database();
    private SqlConnection _connection;
    private Database()
    {
        _connection = new("Server=word-db;User Id=sa;Password=SuperSecret7!;Encrypt=false;");
        _connection.Open();
    }

    public static Database GetInstance()
    {
        return instance;
    }

    public void DeleteDatabase()
    {
        Execute("DROP TABLE IF EXISTS Occurrences");
        Execute("DROP TABLE IF EXISTS Words");
        Execute("DROP TABLE IF EXISTS Documents");
    }

    public void RecreateDatabase()
    {
        Execute("CREATE TABLE Documents(id INTEGER PRIMARY KEY, url VARCHAR(500))");
        Execute("CREATE TABLE Words(id INTEGER PRIMARY KEY, name VARCHAR(500))");
        Execute("CREATE TABLE Occurrences(wordId INTEGER, docId INTEGER, "
                + "FOREIGN KEY (wordId) REFERENCES Words(id), "
                + "FOREIGN KEY (docId) REFERENCES Documents(id))");

        //Execute("CREATE INDEX word_index ON Occ (wordId)");
    }

    // key is the id of the document, the value is number of search words in the document
    public Dictionary<int, int> GetDocuments(List<int> wordIds)
    {
        var res = new Dictionary<int, int>();

        var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) +
                  " GROUP BY docId ORDER BY count DESC;";

        var selectCmd = _connection.CreateCommand();
        selectCmd.CommandText = sql;

        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var docId = reader.GetInt32(0);
                var count = reader.GetInt32(1);

                res.Add(docId, count);
            }
        }

        return res;
    }

    private string AsString(List<int> x)
    {
        return string.Concat("(", string.Join(',', x.Select(i => i.ToString())), ")");
    }

    public List<string> GetDocDetails(List<int> docIds)
    {
        List<string> res = new List<string>();

        var selectCmd = _connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var url = reader.GetString(1);

                res.Add(url);
            }
        }

        return res;
    }

    private void Execute(string sql)
    {
        using var trans = _connection.BeginTransaction();
        var cmd = _connection.CreateCommand();
        cmd.Transaction = trans;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
        trans.Commit();
    }

    internal void InsertAllWords(Dictionary<string, int> res)
    {
        using (var transaction = _connection.BeginTransaction())
        {
            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"INSERT INTO Words(id, name) VALUES(@id,@name)";

            var paramName = command.CreateParameter();
            paramName.ParameterName = "name";
            command.Parameters.Add(paramName);

            var paramId = command.CreateParameter();
            paramId.ParameterName = "id";
            command.Parameters.Add(paramId);

            // Insert all entries in the res

            foreach (var p in res)
            {
                paramName.Value = p.Key;
                paramId.Value = p.Value;
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    internal void InsertAllOcc(int docId, ISet<int> wordIds)
    {
        using (var transaction = _connection.BeginTransaction())
        {
            var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"INSERT INTO Occurrences(wordId, docId) VALUES(@wordId,@docId)";

            var paramwordId = command.CreateParameter();
            paramwordId.ParameterName = "wordId";

            command.Parameters.Add(paramwordId);

            var paramDocId = command.CreateParameter();
            paramDocId.ParameterName = "docId";
            paramDocId.Value = docId;

            command.Parameters.Add(paramDocId);

            foreach (var p in wordIds)
            {
                paramwordId.Value = p;
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public void InsertDocument(int id, string url)
    {
        var insertCmd = new SqlCommand("INSERT INTO Documents(id, url) VALUES(@id,@url)");
        insertCmd.Connection = _connection;

        var pName = new SqlParameter("url", url);
        insertCmd.Parameters.Add(pName);

        var pCount = new SqlParameter("id", id);
        insertCmd.Parameters.Add(pCount);

        insertCmd.ExecuteNonQuery();
    }

    public Dictionary<string, int> GetAllWords()
    {
        Dictionary<string, int> res = new Dictionary<string, int>();

        var selectCmd = _connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Words";

        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var w = reader.GetString(1);

                res.Add(w, id);
            }
        }

        return res;
    }
}