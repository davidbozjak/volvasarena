using Microsoft.Data.Sqlite;
using System.Data.SQLite;

namespace TestableAbstractions;

public interface IDAL : IDisposable
{
    IEnumerable<T> ExecuteReadQuery<T>(string sql, Func<IEnumerable<object>, T> createFunc, params (string, object)[] args);

    T ExecuteScalar<T>(string sql, Func<object, T> createFunc, params (string, object)[] args);

    void ExecuteWriteQuery(string sql, params (string, object)[] args);

    ITransactionHandle StartTransaction();
}

public class DAL : IDAL
{
    private readonly Lazy<SQLiteConnection> lazyConnection;

    private bool _disposed = false;
    private SQLiteTransactionHandle? transaction;

    private SQLiteConnection Connection => this.lazyConnection.Value;

    public DAL(string dbFilePath)
    {
        this.lazyConnection = new Lazy<SQLiteConnection>(() =>
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder("Data Source=" + dbFilePath)
            {
                Mode = SqliteOpenMode.ReadWrite,
                ForeignKeys = true,
            };

            var connection = new SQLiteConnection(connectionStringBuilder.ToString());

            connection.Open();
            return connection;
        });
    }

    public ITransactionHandle StartTransaction()
    {
        if (this.transaction != null)
            throw new Exception("Transaction already open, abort!");

        var trans = this.lazyConnection.Value.BeginTransaction();

        this.transaction = new SQLiteTransactionHandle(trans, () => this.transaction = null);

        return this.transaction;
    }

    public IEnumerable<T> ExecuteReadQuery<T>(string sql, Func<IEnumerable<object>, T> createFunc, params (string, object)[] args)
    {
        using (var command = new SQLiteCommand(sql, this.Connection, this.transaction?.Transaction))
        {
            foreach ((var argName, var argValue) in args)
            {
                command.Parameters.AddWithValue("$" + argName, argValue);
            }

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var resultsRow = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        resultsRow.Add(reader.GetValue(i));
                    }

                    yield return createFunc(resultsRow);
                }
            }
        }
    }

    public T ExecuteScalar<T>(string sql, Func<object, T> createFunc, params (string, object)[] args)
    {
        using (var command = new SQLiteCommand(sql, this.Connection, this.transaction?.Transaction))
        {
            foreach ((var argName, var argValue) in args)
            {
                command.Parameters.AddWithValue("$" + argName, argValue);
            }

            return createFunc(command.ExecuteScalar());
        }
    }

    public void ExecuteWriteQuery(string sql, params (string, object)[] args)
    {
        using (var command = new SQLiteCommand(sql, this.Connection, this.transaction?.Transaction))
        {
            foreach ((var argName, var argValue) in args)
            {
                command.Parameters.AddWithValue("$" + argName, argValue);
            }

            command.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (this.lazyConnection.IsValueCreated)
        {
            this.lazyConnection.Value.Dispose();
        }
    }
}