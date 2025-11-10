using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query;

namespace hw.Tests.TestInfrastructure.AsyncEnumerable;

internal class TestAsyncEnumerable<T> : IOrderedQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IQueryable<T> _queryable;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
    {
        _queryable = enumerable.AsQueryable();
        Provider = new TestAsyncQueryProvider<T>(_queryable.Provider);
    }

    public TestAsyncEnumerable(Expression expression)
    {
        _queryable = new EnumerableQuery<T>(expression);
        Provider = new TestAsyncQueryProvider<T>(_queryable.Provider);
    }

    public Type ElementType => typeof(T);
    public Expression Expression => _queryable.Expression;
    public IQueryProvider Provider { get; }

    public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default
    ) => new TestAsyncEnumerator<T>(_queryable.GetEnumerator());
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
        new TestAsyncEnumerable<TResult>(expression);

    public TResult ExecuteAsync<TResult>(
        Expression expression,
        CancellationToken cancellationToken = default
    ) => Execute<TResult>(expression);
}
