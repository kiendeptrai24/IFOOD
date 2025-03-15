using iFood.Data;
using iFood.Interfaces;
using iFood.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository.Interfaces;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDBContext _context;

    public UserRepository(ApplicationDBContext context)
    {
        _context = context;
    }
    public bool Add(AppUser user)
    {
        _context.Add(user);
        return Save();
    }

    public bool Delete(AppUser user)
    {
        _context.Remove(user);
        return Save();
    }

    public async Task<IEnumerable<AppUser>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<AppUser> GetUserById(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public bool Save()
    {
        var saved = _context.SaveChanges();
        return saved > 0 ? true : false;
    }

    public bool Update(AppUser user)
    {
        _context.Update(user);
        return Save();
    }
}
