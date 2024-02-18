using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        public AccountController(DataContext context) { 
            _context = context; 
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(string pUsername, string pPassword)
        {
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = pUsername,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(pPassword)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
