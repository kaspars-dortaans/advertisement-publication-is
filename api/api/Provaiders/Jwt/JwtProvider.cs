﻿using api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace api.Provaiders.Jwt
{
    public class JwtProvider : IJwtProvider
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtProviderOptions _options;

        public JwtProvider(UserManager<User> userManager, IOptions<JwtProviderOptions> options)
        {
            _userManager = userManager;
            _options = options.Value;
        }

        public async Task<string> GetJwtToken(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("Invalid credentials");
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                throw  new Exception("Invalid credentials");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new List<Claim> { new Claim(ClaimTypes.Email, user.Email!), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };

            var securityToken = new JwtSecurityToken(
              _options.Issuer,
              _options.Audience,
              userClaims,
              expires: DateTime.Now.AddMinutes(_options.TokenExpirationTimeInMinutes),
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return token;
        }
    }
}
