using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;

        public AccountController( DataContext context, ITokenService tokenService )
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost( "register" )]
        public async Task<ActionResult<UserDto>> Register( RegisterDto pRegisterDto )
        {

            if ( await UserExists( pRegisterDto.UserName ) ) return BadRequest( "Username is taken" );
            
            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = pRegisterDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash( Encoding.UTF8.GetBytes( pRegisterDto.Password ) ),
                PasswordSalt = hmac.Key
            };

            if ( !await UserExists( user.UserName ) )
            {
                _context.Users.Add( user );
                await _context.SaveChangesAsync();
            }

            return new UserDto{
                Username = pRegisterDto.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync( x => x.UserName == loginDto.UserName );

            if ( user == null ) return Unauthorized( "User doesn't exist" );

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash( Encoding.UTF8.GetBytes( loginDto.Password ) );

            for( int i = 0; i < computedHash.Length; i++ )
            {
                if ( computedHash[i] != user.PasswordHash[i] )
                    return Unauthorized( "Invalid password" );
            }


            return new UserDto
            {
                Username = loginDto.UserName,
                Token = _tokenService.CreateToken( user )
            };
        }

        private async Task<bool> UserExists( string pUsername )
        {
            return await _context.Users.AnyAsync( x => x.UserName == pUsername.ToLower() );
        }
    }
}
