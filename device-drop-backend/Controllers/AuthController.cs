using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;
using device_drop_backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService; 
    private readonly string _jwtSecret; 

    public AuthController(AppDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _jwtSecret = configuration["Jwt:Secret"] ?? "your-secret-key";
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user != null)
            {
                if (user.Verified == null)
                {
                    return BadRequest(new { error = "Аккаунт не подтвержден, перейдите по ссылке в письме" });
                }
                return BadRequest(new { error = "Пользователь уже существует" });
            }

            var createdUser = new User
            {
                FirstName = dto.FirstName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), 
                Role = UserRole.USER
            };
            _context.Users.Add(createdUser);
            await _context.SaveChangesAsync();

            var code = new Random().Next(100000, 999999).ToString();
            var sessionToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddHours(2);

            var verificationCode = new VerificationCode
            {
                Code = code,
                UserId = createdUser.Id,
                SessionToken = sessionToken,
                ExpiresAt = expiresAt
            };
            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();
         
            Response.Cookies.Append("verification_token", sessionToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                MaxAge = TimeSpan.FromHours(2),
                Path = "/",
                SameSite = SameSiteMode.Strict
            });

            await _emailService.SendEmailAsync(
                createdUser.Email,
                "DeviceDrop / 📝 Подтверждение регистрации",
                $"Ваш код: {code}\nhttp://localhost:3000/verify?code={code}"
            );

            return Ok(new { message = "Пользователь зарегистрирован, проверьте почту для подтверждения" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error [REGISTER_USER]: {ex.Message}");
            return StatusCode(500, new { error = "Не удалось зарегистрировать пользователя" });
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Code))
            {
                return BadRequest(new { error = "Неверный код" });
            }

            var sessionToken = Request.Cookies["verification_token"];
            if (string.IsNullOrEmpty(sessionToken))
            {
                return BadRequest(new { error = "Токен верификации отсутствует" });
            }

            var verificationCode = await _context.VerificationCodes
                .Include(vc => vc.User)
                .FirstOrDefaultAsync(vc => vc.Code == dto.Code && vc.SessionToken == sessionToken && vc.ExpiresAt > DateTime.UtcNow);

            if (verificationCode == null)
            {
                return BadRequest(new { error = "Неверный код" });
            }

            var user = verificationCode.User;
            user.Verified = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _context.VerificationCodes.Remove(verificationCode);
            await _context.SaveChangesAsync();

            Response.Cookies.Delete("verification_token");

            var authToken = GenerateJwtToken(user);

            return Ok(new { success = true, authToken });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error [VERIFY_CODE]: {ex.Message}");
            return StatusCode(500, new { error = "Не удалось подтвердить код" });
        }
    }
    
    [HttpPost("verify-token")]
        public IActionResult VerifyToken([FromBody] VerifyTokenDto dto)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSecret);
                tokenHandler.ValidateToken(dto.AuthToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
    
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
    
                var user = _context.Users.FirstOrDefault(u => u.Id == userId && u.Verified != null);
                if (user == null)
                {
                    return Unauthorized(new { error = "Пользователь не найден или не верифицирован" });
                }
    
                return Ok(new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying token: {ex.Message}");
                return Unauthorized(new { error = "Неверный токен" });
            }
        }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("firstName", user.FirstName),
            new Claim("role", user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}