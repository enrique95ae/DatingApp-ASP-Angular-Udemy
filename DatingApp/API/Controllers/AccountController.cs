using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AccountController( DataContext context, ITokenService tokenService, IMapper mapper )
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost( "register" )]
        public async Task<ActionResult<UserDto>> Register( RegisterDto pRegisterDto )
        {

            if ( await UserExists( pRegisterDto.UserName ) ) return BadRequest( "Username is taken" );

            var user = _mapper.Map<AppUser>( pRegisterDto );

            using var hmac = new HMACSHA512();
            {
                user.UserName = pRegisterDto.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash( Encoding.UTF8.GetBytes( pRegisterDto.Password ) );
                user.PasswordSalt = hmac.Key;

                if ( !await UserExists( user.UserName ) )
                {
                    _context.Users.Add( user );
                    await _context.SaveChangesAsync();
                }

                return new UserDto
                {
                    Username = pRegisterDto.UserName,
                    Token = _tokenService.CreateToken( user ),
                    KnownAs = user.KnownAs,
                };
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include( p => p.Photos )
                .SingleOrDefaultAsync( x => x.UserName == loginDto.UserName );

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
                Token = _tokenService.CreateToken( user ),
                PhotoUrl = user.Photos.FirstOrDefault( x => x.IsMain )?.Url,
                KnownAs = user.KnownAs,
            };
        }

        private async Task<bool> UserExists( string pUsername )
        {
            return await _context.Users.AnyAsync( x => x.UserName == pUsername.ToLower() );
        }
    }
}
