using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DDMS.Backend.Shared.TestUtilities;

/// <summary>
/// Execution strategy giả cho unit test: chạy thẳng thao tác, không retry.
/// Service thật bọc transaction trong IExecutionStrategy (vì Program.cs bật
/// EnableRetryOnFailure), nên mock repository cũng phải trả về một strategy —
/// cái này đóng vai "chạy một lần rồi thôi".
/// </summary>
public sealed class ImmediateExecutionStrategy : IExecutionStrategy
{
    public bool RetriesOnFailure => false;

    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        => operation(null!, state);

    public Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
        => operation(null!, state, cancellationToken);
}
