using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ombi.Core.IdentityResolver
{
    public class UserIdentityManager : IUserIdentityManager
    {
        public UserIdentityManager(IUserRepository userRepository, IMapper mapper, ITokenRepository token)
        {
            UserRepository = userRepository;
            Mapper = mapper;
            TokenRepository = token;
        }

        private IMapper Mapper { get; }
        private IUserRepository UserRepository { get; }
        private ITokenRepository TokenRepository { get; }

        public async Task<bool> CredentialsValid(string username, string password)
        {
            var user = await UserRepository.GetUser(username);
            if (user == null) return false;

            var hash = HashPassword(password, user.Salt);

            return hash.HashedPass.Equals(user.Password);
        }

        public async Task<UserDto> GetUser(string username)
        {
            return Mapper.Map<UserDto>(await UserRepository.GetUser(username));
        }
        public async Task<UserDto> GetUser(int userId)
        {
            return Mapper.Map<UserDto>(await UserRepository.GetUser(userId));
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            return Mapper.Map<List<UserDto>>(await UserRepository.GetUsers());
        }

        public async Task<UserDto> CreateUser(UserDto userDto)
        {
            var user = Mapper.Map<User>(userDto);
            user.Claims.RemoveAll(x => x.Type == ClaimTypes.Country); // This is a hack around the Mapping Profile
            var result = HashPassword(Guid.NewGuid().ToString("N")); // Since we do not allow the admin to set up the password. We send an email to the user
            user.Password = result.HashedPass;
            user.Salt = result.Salt;
            await UserRepository.CreateUser(user);

            await TokenRepository.CreateToken(new EmailTokens
            {
                UserId = user.Id,
                ValidUntil = DateTime.UtcNow.AddDays(7),
            });

            //BackgroundJob.Enqueue(() => );

            return Mapper.Map<UserDto>(user);
        }

        public async Task DeleteUser(UserDto user)
        {
            await UserRepository.DeleteUser(Mapper.Map<User>(user));
        }

        public async Task<UserDto> UpdateUser(UserDto userDto)
        {
            userDto.Claims.RemoveAll(x => x.Type == ClaimTypes.Country); // This is a hack around the Mapping Profile
            var user = Mapper.Map<User>(userDto);
            return Mapper.Map<UserDto>(await UserRepository.UpdateUser(user));
        }

        private UserHash HashPassword(string password)
        {
            // generate a 128-bit salt using a secure PRNG
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return HashPassword(password, salt);
        }

        private UserHash HashPassword(string password, byte[] salt)
        {
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA1,
                10000,
                256 / 8));

            return new UserHash {HashedPass = hashed, Salt = salt};
        }

        private class UserHash
        {
            public string HashedPass { get; set; }
            public byte[] Salt { get; set; }
        }
    }
}