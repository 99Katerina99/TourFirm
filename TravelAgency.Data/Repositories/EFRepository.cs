using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TravelAgency.Core.Interfaces;

// 🔥 ВАЖНО: Создаём алиас, чтобы отличать наш контекст от класса из EF Core
using AppDbContext = TravelAgency.Data.DbContext.TravelAgencyDbContext;

namespace TravelAgency.Data.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    // 🔥 Используем алиас AppDbContext вместо простого DbContext
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public EfRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();
}