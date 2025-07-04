﻿using System.ComponentModel.DataAnnotations;

namespace TestApi.DTOS
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string[] Roles { get; set; }
    }
}
