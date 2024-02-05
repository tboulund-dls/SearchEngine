using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace WordService;

public class Database
{
    private Coordinator coordinator = new Coordinator();
    private static Database instance = new Database();

    public static Database GetInstance()
    {
        return instance;
    }

    public async Task DeleteDatabase()
    {
        foreach (var connection in coordinator.GetAllConnections())
        {
            await ExecuteAsync(connection, "DROP TABLE IF EXISTS Occurrences");
            await ExecuteAsync(connection, "DROP TABLE IF EXISTS Words");
            await ExecuteAsync(connection, "DROP TABLE IF EXISTS Documents");
        }
    }

    public async Task RecreateDatabase()
    {
        await ExecuteAsync(coordinator.GetDocumentConnection(), "CREATE TABLE Documents(id INTEGER PRIMARY KEY, url VARCHAR(500))");
        await ExecuteAsync(coordinator.GetOccurrenceConnection(), "CREATE TABLE Occurrences(wordId INTEGER, docId INTEGER)");
        //Execute(coordinator.GetOccurrenceConnection(), "CREATE INDEX word_index ON Occ (wordId)");

        foreach (var connection in coordinator.GetAllWordConnections())
        {
            await ExecuteAsync(connection, "CREATE TABLE Words(id INTEGER PRIMARY KEY, name VARCHAR(500))");
        }
    }

    // key is the id of the document, the value is number of search words in the document
    public async Task<Dictionary<int, int>> GetDocuments(List<int> wordIds)
    {
        var res = new Dictionary<int, int>();

        var sql = @"SELECT docId, COUNT(wordId) AS count FROM Occurrences WHERE wordId IN " + AsString(wordIds) +
                  " GROUP BY docId ORDER BY count DESC;";

        var connection = coordinator.GetOccurrenceConnection();
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = sql;

        using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
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

    public async Task<List<string>> GetDocDetails(List<int> docIds)
    {
        List<string> res = new List<string>();

        var connection = coordinator.GetDocumentConnection();
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Documents WHERE id IN " + AsString(docIds);

        using (var reader = await selectCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32(0);
                var url = reader.GetString(1);

                res.Add(url);
            }
        }

        return res;
    }

    private async Task ExecuteAsync(DbConnection connection, string sql)
    {
        using var trans = connection.BeginTransaction();
        var cmd = connection.CreateCommand();
        cmd.Transaction = trans;
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
        trans.Commit();
    }

    internal async Task InsertAllWords(Dictionary<string, int> res)
    {
        foreach (var p in res)
        {
            var connection = coordinator.GetWordConnection(p.Key);
            await using (var transaction = await connection.BeginTransactionAsync())
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO Words(id, name) VALUES(@id,@name)";

                var paramName = command.CreateParameter();
                paramName.ParameterName = "name";
                command.Parameters.Add(paramName);

                var paramId = command.CreateParameter();
                paramId.ParameterName = "id";
                command.Parameters.Add(paramId);

                paramName.Value = p.Key;
                paramId.Value = p.Value;
                await command.ExecuteNonQueryAsync();
                
                await transaction.CommitAsync();
            }

        }
    }

    internal async Task InsertAllOcc(int docId, ISet<int> wordIds)
    {
        var connection = coordinator.GetOccurrenceConnection();
        await using (var transaction = await connection.BeginTransactionAsync())
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"INSERT INTO Occurrences(wordId, docId) VALUES(@wordId,@docId)";

            var paramwordId = command.CreateParameter();
            paramwordId.ParameterName = "wordId";

            command.Parameters.Add(paramwordId);

            var paramDocId = command.CreateParameter();
            paramDocId.ParameterName = "docId";
            paramDocId.Value = docId;

            command.Parameters.Add(paramDocId);

            foreach (var wordId in wordIds)
            {
                paramwordId.Value = wordId;
                await command.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
    }

    public async Task InsertDocument(int id, string url)
    {
        var connection = coordinator.GetDocumentConnection();
        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Documents(id, url) VALUES(@id,@url)";

        var pName = new SqlParameter("url", url);
        insertCmd.Parameters.Add(pName);

        var pCount = new SqlParameter("id", id);
        insertCmd.Parameters.Add(pCount);

        await insertCmd.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<string, int>> GetAllWords()
    {
        Dictionary<string, int> res = new Dictionary<string, int>();

        foreach (var connection in coordinator.GetAllWordConnections())
        {
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Words";

            await using (var reader = await selectCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    var w = reader.GetString(1);

                    res.Add(w, id);
                }
            }
        }
        return res;
    }
}