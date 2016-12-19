#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: OperatingSystemHelper.cs
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
namespace Ombi.Helpers
{
    public static class OperatingSystemHelper
    {
        public static string GetOs()
        {
            var os = System.Environment.OSVersion;
            var osVersion = os.Version;
            if (osVersion.Major.Equals(10))
            {
                return "Windows 10/Windows Server 2016";
            }
            if (osVersion.Major.Equals(6))
            {
                if (osVersion.Minor.Equals(3))
                {
                    return "Windows 8.1/Windows Server 2012 R2";
                }
                if (osVersion.Minor.Equals(2))
                {
                    return "Windows 8/Windows Server 2012";
                }
                if (osVersion.Minor.Equals(1))
                {
                    return "Windows 7/Windows Server 2008 R2";
                }
                if (osVersion.Minor.Equals(0))
                {
                    return "Windows Vista/Windows Server 2008";
                }
            }
            if (osVersion.Major.Equals(5))
            {
                if (osVersion.Minor.Equals(2))
                {
                    return "Windows XP 64-Bit Edition/Windows Server 2003";
                }
                if (osVersion.Minor.Equals(1))
                {
                    return "Windows XP";
                }
                if (osVersion.Minor.Equals(0))
                {
                    return "Windows 2000";
                }
            }
            return os.VersionString;
        }
    }
}