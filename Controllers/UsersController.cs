using Microsoft.AspNetCore.Mvc;
using FarmManagement.API.Helpers;
using Microsoft.EntityFrameworkCore;
using FarmManagement.API.Data;
using FarmManagement.API.Models;
using FarmManagement.API.DTOs;
using BCrypt.Net;

namespace FarmManagement.API.Controllers
{
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly FarmDbContext _context;

    public UsersController(FarmDbContext context)
    {
        _context = context;
    }

[HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    if (await _context.AppUsers.AnyAsync(u => u.Username == dto.Username))
        return BadRequest("اسم المستخدم موجود بالفعل");

    if (dto.UserType == UserType.Owner)
    {
        var ownerExists = await _context.AppUsers
            .AnyAsync(u => u.UserType == UserType.Owner);

        if (ownerExists)
            return BadRequest("يوجد Owner بالفعل ولا يمكن إنشاء آخر");
    }

    var user = new AppUser
    {
        Username = dto.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        UserType = dto.UserType
    };

    _context.AppUsers.Add(user);
    await _context.SaveChangesAsync();

    return Ok("تم إنشاء المستخدم بنجاح");
}

[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _context.AppUsers
        .FirstOrDefaultAsync(u => u.Username == dto.Username);

    if (user == null)
        return Unauthorized("بيانات غير صحيحة");

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Unauthorized("بيانات غير صحيحة");

    return Ok(new
    {
        user.Id,
        user.Username,
        user.UserType
    });
}

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult> GetUsers(
          int SkipCount = 0,
          int MaxResultCount = 7)
        {
            var query = _context.AppUsers.AsQueryable();

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.Id)
                .Skip(SkipCount)
                .Take(MaxResultCount)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UserType = u.UserType.ToString() // Employee أو Owner
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Users = users
            });
        }

        // GET: api/users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            var user = await _context.AppUsers
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    UserType = u.UserType.ToString()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("المستخدم غير موجود");

            return Ok(user);
        }

    }

}



