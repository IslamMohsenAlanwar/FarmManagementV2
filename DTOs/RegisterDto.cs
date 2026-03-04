using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public UserType UserType { get; set; }
    }


    public class LoginDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string UserType { get; set; } = null!; // Employee / Owner
    }

}