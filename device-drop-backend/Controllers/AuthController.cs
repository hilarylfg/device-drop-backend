﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using device_drop_backend.Data;
using device_drop_backend.Dtos;
using device_drop_backend.Models;
using device_drop_backend.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly string _jwtSecret;
    private readonly string _tokenCookieName = "auth_token";
    private readonly string _refreshTokenCookieName = "refresh_token";
    private readonly TimeSpan _tokenExpiration = TimeSpan.FromDays(1);
    private readonly TimeSpan _refreshTokenExpiration = TimeSpan.FromDays(30);

    public AuthController(AppDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _jwtSecret = configuration["Jwt:Secret"] ?? "your-secret-key";
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var token = Request.Cookies[_tokenCookieName];
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { error = "Вы не авторизованы" });
        }

        var jwtToken = ValidateJwtToken(token);
        if (jwtToken == null)
        {
            return Unauthorized(new { error = "Неверный токен" });
        }

        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.Verified != null);

        if (user == null)
        {
            return Unauthorized(new { error = "Пользователь не найден или не верифицирован" });
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            role = user.Role.ToString()
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password) || user.Verified == null)
        {
            return Unauthorized(new { error = "Неверный email или пароль" });
        }

        var authToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user);
        
        SetAuthCookies(authToken, refreshToken);

        return Ok(new { authToken, id = user.Id});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new { error = existingUser.Verified == null ? "Аккаунт не подтвержден, проверьте почту" : "Пользователь уже существует" });
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.USER
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var code = GenerateVerificationCode();
        var sessionToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(2);

        _context.VerificationCodes.Add(new VerificationCode
        {
            Code = code,
            UserId = user.Id,
            SessionToken = sessionToken,
            ExpiresAt = expiresAt
        });
        await _context.SaveChangesAsync();

        SetVerificationCookie(sessionToken);
        await SendVerificationEmail(user.Email, code);

        return Ok(new { message = "Пользователь зарегистрирован, проверьте почту для подтверждения" });
    }

    [HttpPost("oauth")]
    public async Task<IActionResult> OAuth([FromBody] OAuthDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Provider == dto.Provider && u.ProviderId == dto.ProviderId);
        if (user != null)
        {
            user.Email = dto.Email;
            user.FirstName = dto.Name;
            await _context.SaveChangesAsync();
        }
        else
        {
            user = new User
            {
                Email = dto.Email,
                FirstName = dto.Name,
                Provider = dto.Provider,
                ProviderId = dto.ProviderId,
                Verified = DateTime.UtcNow,
                Role = UserRole.USER
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var authToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user);
        
        SetAuthCookies(authToken, refreshToken);

        return Ok(new { authToken, id = user.Id, email = user.Email, firstName = user.FirstName, role = user.Role });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
    {
        if (string.IsNullOrEmpty(dto.Code)) return BadRequest(new { error = "Неверный код" });

        var sessionToken = Request.Cookies["verification_token"];
        if (string.IsNullOrEmpty(sessionToken)) return BadRequest(new { error = "Токен верификации отсутствует" });

        var verificationCode = await _context.VerificationCodes
            .Include(vc => vc.User)
            .FirstOrDefaultAsync(vc => vc.Code == dto.Code && vc.SessionToken == sessionToken && vc.ExpiresAt > DateTime.UtcNow);

        if (verificationCode == null) return BadRequest(new { error = "Неверный код" });

        var user = verificationCode.User;
        user.Verified = DateTime.UtcNow;
        _context.VerificationCodes.Remove(verificationCode);
        await _context.SaveChangesAsync();

        Response.Cookies.Delete("verification_token");
        
        var authToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user);
        
        SetAuthCookies(authToken, refreshToken);

        return Ok(new { success = true, authToken });
    }

    [HttpPost("verify-token")]
    public IActionResult VerifyToken([FromBody] VerifyTokenDto dto)
    {
        string tokenToVerify = dto.AuthToken ?? Request.Cookies[_tokenCookieName];
        
        if (string.IsNullOrEmpty(tokenToVerify)) return BadRequest(new { error = "Токен не предоставлен" });

        var jwtToken = ValidateJwtToken(tokenToVerify);
        if (jwtToken == null) return Unauthorized(new { error = "Неверный токен" });

        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
        var user = _context.Users.FirstOrDefault(u => u.Id == userId && u.Verified != null);

        return user == null
            ? Unauthorized(new { error = "Пользователь не найден или не верифицирован" })
            : Ok(new { id = user.Id, email = user.Email, firstName = user.FirstName, role = user.Role });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[_refreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh токен отсутствует" });
        }

        var jwtToken = ValidateJwtToken(refreshToken);
        if (jwtToken == null)
        {
            return Unauthorized(new { error = "Неверный refresh токен" });
        }

        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.Verified != null);

        if (user == null)
        {
            return Unauthorized(new { error = "Пользователь не найден или не верифицирован" });
        }

        var newAuthToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken(user);
        
        SetAuthCookies(newAuthToken, newRefreshToken);

        return Ok(new { authToken = newAuthToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        RemoveAuthCookies();
        return Ok(new { message = "Выход из системы выполнен успешно" });
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
            expires: DateTime.UtcNow.Add(_tokenExpiration),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("type", "refresh")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(_refreshTokenExpiration),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetAuthCookies(string authToken, string refreshToken)
    {
        Response.Cookies.Append(_tokenCookieName, authToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.Lax,
            MaxAge = _tokenExpiration,
            Path = "/"
        });

        Response.Cookies.Append(_refreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = _refreshTokenExpiration,
            Path = "/"
        });
    }

    private void RemoveAuthCookies()
    {
        Response.Cookies.Delete(_tokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });
        
        Response.Cookies.Delete(_refreshTokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });
    }

    private JwtSecurityToken? ValidateJwtToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return (JwtSecurityToken)validatedToken;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    private void SetVerificationCookie(string sessionToken)
    {
        Response.Cookies.Append("verification_token", sessionToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,  
            MaxAge = TimeSpan.FromHours(2),
            Path = "/",
            SameSite = SameSiteMode.Lax
        });
    }

    private async Task SendVerificationEmail(string email, string code)
    {
        var verificationUrl = $"http://localhost:3000/verify?code={code}";
        var emailBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>DeviceDrop / 📝 Подтверждение регистрации</h2>
                <p>Спасибо за регистрацию в DeviceDrop!</p>
                <p>Ваш код подтверждения: <strong>{code}</strong></p>
                <p>Или перейдите по ссылке для автоматической верификации:</p>
                <a href='{verificationUrl}' style='background-color: #28a745; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Подтвердить аккаунт</a>
                <p>Код действителен 2 часа.</p>
                <p>Если у вас есть вопросы, свяжитесь с нами: support@devicedrop.ru</p>
            </body>
            </html>";

        await _emailService.SendEmailAsync(
            email,
            "DeviceDrop / 📝 Подтверждение регистрации",
            emailBody,
            isHtml: true
        );
    }
}