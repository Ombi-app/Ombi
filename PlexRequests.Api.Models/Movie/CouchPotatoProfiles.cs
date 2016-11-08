#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CouchPotatoProfiles.cs
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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlexRequests.Api.Models.Movie
{
    public class ProfileList
    {
        public bool core { get; set; }
        public bool hide { get; set; }
        public string _rev { get; set; }
        public List<bool> finish { get; set; }
        public List<string> qualities { get; set; }
        public string _id { get; set; }
        public string _t { get; set; }
        public string label { get; set; }
        public int minimum_score { get; set; }
        public List<int> stop_after { get; set; }
        public List<object> wait_for { get; set; }
        public int order { get; set; }
        [JsonProperty(PropertyName = "3d")]
        public List<object> threeD { get; set; }
    }

    public class CouchPotatoProfiles
    {
        public List<ProfileList> list { get; set; }
        public bool success { get; set; }
    }
}