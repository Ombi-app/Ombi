#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: MovieBase.cs
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
using RequestPlex.Helpers;

namespace RequestPlex.Api
{
    public abstract class MovieBase
    {
        private static readonly string Encrypted = "0T3QNSseexLO7n7UPiJvl70Y+KKnvbeTlsusl7Kwq0hPH0BHOuFNGwksNCjkwqWedyDdI/MJeUR4wtL4bIl0Z+//uHXEaYM/4H2pjeLbH5EWdUe5TTj1AhaIR5PQweamvcienRyFD/3YPCC/+qL5mHkKXBkPumMod3Zb/4yN0Ik=";
        protected string ApiKey = StringCipher.Decrypt(Encrypted, "ApiKey");
        protected string Url = "http://api.themoviedb.org/3";
    }
}
