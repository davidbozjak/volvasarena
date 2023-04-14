using System.Data.SQLite;

namespace TestableAbstractions;

public interface ITransactionHandle : IDisposable
{
    void Commit();
    void Rollback();
}

public class SQLiteTransactionHandle : ITransactionHandle
{
    private bool _disposed = false;
    private readonly Action completeCallback;

    public SQLiteTransaction Transaction { get; }
    public SQLiteTransactionHandle(SQLiteTransaction transaction, Action completeCallback)
    {
        this.Transaction = transaction;
        this.completeCallback = completeCallback;
    }

    ~SQLiteTransactionHandle()
    {
        this.Rollback();
    }

    public void Commit()
    {
        if (_disposed) return;
        _disposed = true;

        this.Transaction.Commit();
        this.completeCallback();
    }

    public void Rollback()
    {
        if (_disposed)
            return;
        _disposed = true;

        this.Transaction.Rollback();
        this.completeCallback();
    }

    public void Dispose()
    {
        this.Rollback();
        GC.SuppressFinalize(this);
    }
}