using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

var secretKey = "MySuperSecretPassPhraseGoesHere!";
var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(options =>
{
  options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false;
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = securityKey,
    ValidateIssuer = false,
    ValidateAudience = false,
    ClockSkew = TimeSpan.FromMinutes(5)
  };
});

builder.Services.AddAuthorization(options =>
{
  options.DefaultPolicy = new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();
});

builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(builder =>
  {
    builder.WithOrigins("https://localhost:4200")
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/", () => "Hello World!");
app.MapGet("/weatherforecast", [Authorize] () =>
{
  var forecast = Enumerable.Range(1, 5).Select(index =>
      new WeatherForecast
      (
          DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
          Random.Shared.Next(-20, 55),
          summaries[Random.Shared.Next(summaries.Length)]
      ))
      .ToArray();
  return forecast;
});
app.MapGet("/hello", [Authorize] () => "Hello World #1!");
app.MapGet("/world", [Authorize] () => "Hello World #2!");

// This was done on purpose. W aren't trying to prove that we can
// deal with usernames or passwords or other forms of authentication
// we just want to simulate the process of getting a JWT token and
// so that we can validate our refresh token flow is working from the
// front end.
app.MapPost("/token", () =>
{
  return Results.Ok(GenerateToken());
});

app.MapPost("/refresh", (string refreshToken) =>
{
  // We'd check if the refresh token is valid ideally we would
  // store this in a database and associate it with the user and
  // then invalidate it when the user logs out or after a specific
  // amount of time has passed.
  return Results.Ok(GenerateToken());
});

app.Run();

JWT GenerateToken()
{
  var claims = new[]
  {
    new Claim(ClaimTypes.Name, "John Doe"),
    new Claim(ClaimTypes.Email, "john.doe@example.com"),
    new Claim(ClaimTypes.Role, "User")
  };

  var tokenDescriptor = new SecurityTokenDescriptor
  {
    Audience = "https://localhost:7001",
    Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddSeconds(10),
    SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
  };

  var tokenHandler = new JwtSecurityTokenHandler();
  var token = tokenHandler.CreateToken(tokenDescriptor);
  var jwtToken = tokenHandler.WriteToken(token);

  var refreshToken = string.Empty;
  var randomNumber = new byte[32];
  using (var rng = RandomNumberGenerator.Create())
  {
    rng.GetBytes(randomNumber);
    refreshToken = Convert.ToBase64String(randomNumber);
  }

  return new JWT(jwtToken, refreshToken);
}

internal record JWT(string AccessToken, string RefreshToken);

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
