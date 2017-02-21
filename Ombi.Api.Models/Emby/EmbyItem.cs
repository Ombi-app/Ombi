﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: EmbyLibrary.cs
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
namespace Ombi.Api.Models.Emby
{
    public class EmbyLibrary
    {
        public string Name { get; set; }
        public string ServerId { get; set; }
        public string Id { get; set; }
        public bool HasDynamicCategories { get; set; }
        public string PlayAccess { get; set; }
        public bool IsFolder { get; set; }
        public string Type { get; set; }
        public EmbyUserdata UserData { get; set; }
        public int ChildCount { get; set; }
        public string CollectionType { get; set; }
        public string OriginalCollectionType { get; set; }
        public EmbyImagetags ImageTags { get; set; }
        public object[] BackdropImageTags { get; set; }
        public string LocationType { get; set; }
    }

}