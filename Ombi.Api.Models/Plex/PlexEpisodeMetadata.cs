#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexEpisodeMetadata.cs
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
using System.Xml.Serialization;

namespace Ombi.Api.Models.Plex
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class PlexEpisodeMetadata
    {
        [XmlElement(ElementName = "Video")]
        public List<Video> Video { get; set; }
        [XmlAttribute(AttributeName = "size")]
        public string Size { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }
        [XmlAttribute(AttributeName = "banner")]
        public string Banner { get; set; }
        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "librarySectionID")]
        public string LibrarySectionID { get; set; }
        [XmlAttribute(AttributeName = "librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }
        [XmlAttribute(AttributeName = "librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }
        [XmlAttribute(AttributeName = "mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }
        [XmlAttribute(AttributeName = "mediaTagVersion")]
        public string MediaTagVersion { get; set; }
        [XmlAttribute(AttributeName = "mixedParents")]
        public string MixedParents { get; set; }
        [XmlAttribute(AttributeName = "nocache")]
        public string Nocache { get; set; }
        [XmlAttribute(AttributeName = "parentIndex")]
        public string ParentIndex { get; set; }
        [XmlAttribute(AttributeName = "parentTitle")]
        public string ParentTitle { get; set; }
        [XmlAttribute(AttributeName = "parentYear")]
        public string ParentYear { get; set; }
        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; }
        [XmlAttribute(AttributeName = "title1")]
        public string Title1 { get; set; }
        [XmlAttribute(AttributeName = "title2")]
        public string Title2 { get; set; }
        [XmlAttribute(AttributeName = "viewGroup")]
        public string ViewGroup { get; set; }
        [XmlAttribute(AttributeName = "viewMode")]
        public string ViewMode { get; set; }
    }

}
