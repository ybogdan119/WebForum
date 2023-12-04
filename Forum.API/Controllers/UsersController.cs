using Azure;
using Forum.Data.Repository.IRepository;
using Forum.Models;
using Forum.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Forum.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            try
            {
                ICollection<UserDto> users = Mapper.ForumUserToUserDto(await _userRepository.GetAllAsync());
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllByRole")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllByRole(string role)
        {
            try
            {
                ICollection<UserDto> users = Mapper.ForumUserToUserDto(await _userRepository.GetAllByRoleAsync(role));
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetById(string id)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(Mapper.ForumUserToUserDto(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequest)
        {
            try
            {
                var errors = await _userRepository.CreateAsync(
                    Mapper.RegistationRequestDtoToForumUser(registrationRequest),
                    registrationRequest.Password,
                    registrationRequest.Role);

                if (errors.Count > 0)
                {
                    return BadRequest(errors);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequstDTO)
        {
            LoginResponseDto? loginResponse = await _userRepository.LoginAsync(loginRequstDTO);
            if (loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                return BadRequest("Username or password is invalid");
            }
            return Ok(loginResponse);
        }
    }
}
