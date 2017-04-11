#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: UserIdentityManager.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.IdentityResolver
{
    public class UserIdentityManager : IUserIdentityManager
    {
        public UserIdentityManager(IUserRepository userRepository)
        {
            UserRepository = userRepository;
        }

        private IUserRepository UserRepository { get; }

        public async Task<bool> CredentialsValid(string username, string password)
        {
            var user = await UserRepository.GetUser(username);
            var hashedPass = HashPassword(password);

            return hashedPass.Equals(user.Password);
        }

        public async Task<User> GetUser(string username)
        {
            return await UserRepository.GetUser(username);
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await UserRepository.GetUsers();
        }

        public async Task CreateUser(User user)
        {
            user.Password = HashPassword(user.Password);
            await UserRepository.CreateUser(user);
        }

        private string HashPassword(string password)
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}