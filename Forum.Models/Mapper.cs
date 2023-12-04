using Forum.Models.Dto;
using Forum.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum.Models
{
    public static class Mapper
    {
        public static UserDto ForumUserToUserDto(ForumUser user)
        {
            return new UserDto
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Id = user.Id,
            };
        }

        public static ICollection<UserDto> ForumUserToUserDto(ICollection<ForumUser> users)
        {
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                result.Add(ForumUserToUserDto(user));
            }

            return result;
        }

        public static ForumUser RegistationRequestDtoToForumUser(RegistrationRequestDto registrationRequest)
        {
            return new ForumUser
            {
                UserName = registrationRequest.UserName,
                Email = registrationRequest.Email,
            };
        }
    }
}
