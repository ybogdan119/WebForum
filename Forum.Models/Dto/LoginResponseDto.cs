using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forum.Models.Dto
{
    public class LoginResponseDto
    {
        public required UserDto User { get; set; }
        public required string Token { get; set; }
    }
}
