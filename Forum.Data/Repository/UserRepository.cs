﻿using Forum.Data.Repository.IRepository;
using Forum.Models;
using Forum.Models.Dto;
using Forum.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Forum.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ForumDbContext _db;
        private readonly UserManager<ForumUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPasswordHasher<ForumUser> _passwordHasher;
        private readonly string? _secretKey;

        public UserRepository
            (ForumDbContext db, UserManager<ForumUser> userManager, RoleManager<IdentityRole> roleManager,
            IPasswordHasher<ForumUser> passwordHasher, IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _passwordHasher = passwordHasher;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        }

        public async Task<ICollection<ForumUser>> GetAllByRoleAsync(string role)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                throw new Exception("Role does not exists");
            }

            return await _userManager.GetUsersInRoleAsync(role);
        }

        public async Task<List<string>> CreateAsync(ForumUser user, string password, string role = "user")
        {
            var errorList = new List<string>();
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    errorList.Add(error.Description);
                }
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                errorList.Add("Role does not exists.");
            }

            if (errorList.Count == 0)
            {
                await _userManager.AddToRoleAsync(user, role);

            }

            return errorList;
        }

        public async Task<List<string>> DeleteAsync(string id)
        {
            var errorList = new List<string>();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                errorList.Add("User not found");
                return errorList;
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    errorList.Add(error.Description);
                }
            }

            return errorList;
        }

        public async Task<ICollection<ForumUser>> GetAllAsync(Expression<Func<ForumUser, bool>>? filter = null)
        {
            IQueryable<ForumUser> users = _db.Users;

            if (filter != null)
            {
                users = users.Where(filter);
            }

            return await users.ToListAsync();
        }

        public async Task<ForumUser?> GetAsync(Expression<Func<ForumUser, bool>> filter)
        {
            return await _db.Users.FirstOrDefaultAsync(filter);
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<List<string>> UpdateAsync(ForumUser model, string? password, string? role)
        {
            var errorList = new List<string>();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                errorList.Add("User not found");
                return errorList;
            }

            user.Email = model.Email;
            user.UserName = model.UserName;

            if (password != null)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, password);
            }

            if (role != null)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    errorList.Add("Role does not exists.");
                    return errorList;
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);

                await _userManager.AddToRoleAsync(user, role);
            }

            _db.Users.Update(user);

            return errorList;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            ForumUser? user = _db.Users.FirstOrDefault(u => u.UserName == loginRequest.UserName);

            bool passwordIsValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (user == null || passwordIsValid == false)
            {
                return new LoginResponseDto()
                {
                    Token = "",
                    User = null
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);

            LoginResponseDto loginResponseDTO = new LoginResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                User = Mapper.ForumUserToUserDto(user)
            };

            return loginResponseDTO;
        }
    }
}
