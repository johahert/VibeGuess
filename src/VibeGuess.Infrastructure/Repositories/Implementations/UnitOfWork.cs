using Microsoft.EntityFrameworkCore.Storage;
using VibeGuess.Infrastructure.Data;
using VibeGuess.Infrastructure.Repositories.Interfaces;

namespace VibeGuess.Infrastructure.Repositories.Implementations;

/// <summary>
/// Unit of work implementation for managing transactions across multiple repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly VibeGuessDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private IQuizRepository? _quizzes;
    private IQuizSessionRepository? _quizSessions;
    private ITrackRepository? _tracks;

    public UnitOfWork(VibeGuessDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    /// <inheritdoc />
    public IQuizRepository Quizzes =>
        _quizzes ??= new QuizRepository(_context);

    /// <inheritdoc />
    public IQuizSessionRepository QuizSessions =>
        _quizSessions ??= new QuizSessionRepository(_context);

    /// <inheritdoc />
    public ITrackRepository Tracks =>
        _tracks ??= new TrackRepository(_context);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}