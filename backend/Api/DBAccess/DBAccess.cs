using Microsoft.EntityFrameworkCore;
using Models;

namespace Api.DBAccess
{
    public class DBAccess
    {
        private readonly DBContext _context;

        public DBAccess(DBContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateUser(User user)
        {
            var users = await _context.Users.ToListAsync();

            foreach (var item in users)
            {
                if (item.UserName == user.UserName || item.Email == user.Email)
                {
                    return false;
                }
            }

            _context.Users.Add(user);
            return await _context.SaveChangesAsync() == 1;
        }

        public async Task<User> Login(User user)
        {
            var profile = await _context.Users.FirstAsync(u => u.UserName == user.UserName);

            if (profile.Password == user.Password)
            {
                profile.Password = "";
                return profile;
            }
            return new User();            
        }

        public async Task<bool> EditUser(User user)
        {
            var profile = await _context.Users.FirstAsync(u => u.Id == user.Id);

            profile.UserName = user.UserName;

            profile.Email = user.Email;

            profile.Password = user.Password;

            return await _context.SaveChangesAsync() == 1;
        }
    }
}
