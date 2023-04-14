public class Cached<T> : IDisposable
{
    private readonly Func<T> initializer;
    private Lazy<T> value;

    public Cached(Func<T> initializer)
    {
        this.initializer = initializer;
        this.value = this.ResetEx();
    }

    public T Value => this.value.Value;

    public bool IsValueCreated => this.value.IsValueCreated;

    public void Reset() => this.ResetEx();

    private Lazy<T> ResetEx()
    {
        this.DisposeCreatedValue();
        return this.value = new Lazy<T>(this.initializer);
    }

    public void Dispose()
    {
        this.Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.DisposeCreatedValue();
        }
    }

    private void DisposeCreatedValue()
    {
        if (this.value?.IsValueCreated == true && this.value.Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
