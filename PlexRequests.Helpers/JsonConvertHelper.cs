#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: JsonConvertHelper.cs
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

using Newtonsoft.Json;

namespace Ombi.Helpers
{
    public static class JsonConvertHelper
    {
        public static T[] ParseObjectToArray<T>(object ambiguousObject)
        {
            var json = ambiguousObject.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new T[0]; // Could return null here instead.
            }
            if (json.TrimStart().StartsWith("["))
            {
                return JsonConvert.DeserializeObject<T[]>(json);
            }
            if (json.TrimStart().Equals("{}"))
            {
                return new T[0];
            }

            return new T[1] { JsonConvert.DeserializeObject<T>(json) };

        }
    }
}