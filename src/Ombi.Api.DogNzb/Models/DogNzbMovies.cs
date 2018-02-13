#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: DogNzbMovies.cs
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

namespace Ombi.Api.DogNzb.Models
{

    [XmlRoot(ElementName = "response", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
    public class Response
    {
        [XmlAttribute(AttributeName = "total")]
        public string Total { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "imdbid")]
        public string Imdbid { get; set; }
        [XmlElement(ElementName = "plot")]
        public string Plot { get; set; }
        [XmlElement(ElementName = "actors")]
        public string Actors { get; set; }
        [XmlElement(ElementName = "genres")]
        public string Genres { get; set; }
        [XmlElement(ElementName = "year")]
        public string Year { get; set; }
        [XmlElement(ElementName = "runtime")]
        public string Runtime { get; set; }
        [XmlElement(ElementName = "certification")]
        public string Certification { get; set; }
        [XmlElement(ElementName = "trailer")]
        public string Trailer { get; set; }
        [XmlElement(ElementName = "poster")]
        public string Poster { get; set; }
    }

    [XmlRoot(ElementName = "channel")]
    public class Channel
    {
        [XmlElement(ElementName = "title")]
        public string Title { get; set; }
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "uuid")]
        public string Uuid { get; set; }
        [XmlElement(ElementName = "response", Namespace = "http://www.newznab.com/DTD/2010/feeds/attributes/")]
        public Response Response { get; set; }
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "rss")]
    public class DogNzbMovies
    {
        [XmlElement(ElementName = "channel")]
        public Channel Channel { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "atom", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Atom { get; set; }
        [XmlAttribute(AttributeName = "newznab", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Newznab { get; set; }
    }
}