using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        public readonly IUserRepository _userRepository;

        public readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper) 
        {
            _userRepository = userRepository;    
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers() 
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok( users );
        }

        [HttpGet("{username}")] // /api/users/username
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.FindFirst( ClaimTypes.NameIdentifier )?.Value;
            var user = await _userRepository.GetUserByUsernameAsync( username );

            if( user == null ) return NotFound();

            _mapper.Map( memberUpdateDto, user );

            if(await _userRepository.SaveAllAsync()) return NoContent();
        
            return BadRequest("Failed to update user");
        }
    }
}
