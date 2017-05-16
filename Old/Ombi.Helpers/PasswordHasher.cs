#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PasswordHasher.cs
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

using System.Collections.Generic;
using System.Security.Cryptography;

namespace Ombi.Helpers
{

    public static class PasswordHasher
    {
        // 24 = 192 bits
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;

        public static byte[] ComputeHash(string password, byte[] salt, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
        {
            var hashGenerator = new Rfc2898DeriveBytes(password, salt) { IterationCount = iterations };
            return hashGenerator.GetBytes(hashByteSize);
        }

        public static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
        {
            var saltGenerator = new RNGCryptoServiceProvider();
            var salt = new byte[saltByteSize];
            saltGenerator.GetBytes(salt);
            return salt;
        }

        public static bool VerifyPassword(string password, byte[] passwordSalt, byte[] passwordHash)
        {
            var computedHash = ComputeHash(password, passwordSalt);
            return AreHashesEqual(computedHash, passwordHash);
        }

        //Length constant verification - prevents timing attack
        private static bool AreHashesEqual(IReadOnlyList<byte> firstHash, IReadOnlyList<byte> secondHash)
        {
            var minHashLength = firstHash.Count <= secondHash.Count ? firstHash.Count : secondHash.Count;
            var xor = firstHash.Count ^ secondHash.Count;
            for (var i = 0; i < minHashLength; i++)
                xor |= firstHash[i] ^ secondHash[i];
            return 0 == xor;
        }
    }
}
