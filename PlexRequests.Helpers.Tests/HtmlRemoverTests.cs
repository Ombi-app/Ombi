#region Copyright

// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: HtmlRemoverTests.cs
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

using NUnit.Framework;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class HtmlRemoverTests
    {
        [Test]
        public void RemoveHtmlBasic()
        {
            var html = "this is <b>bold</b> <p>para</p> OK!";
            var result = html.RemoveHtml();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("this is bold para OK!"));
        }

        [Test]
        public void RemoveHtmlMoreTags()
        {
            // Good 'ol Ali G ;)
            var html = "<p><strong><em>\"Ali G: Rezurection\"</em></strong> includes every episode of <em>Da Ali G Show</em> with new, original introductions by star, creator/writer Sacha Baron Cohen, along with the BAFTA(R) Award-winning English episodes of <em>Da Ali G Show</em> which have never aired on American television and <em>The Best of Ali G</em>.</p>";
            var result = html.RemoveHtml();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("\"Ali G: Rezurection\" includes every episode of Da Ali G Show with new, original introductions by star, creator/writer Sacha Baron Cohen, along with the BAFTA(R) Award-winning English episodes of Da Ali G Show which have never aired on American television and The Best of Ali G."));
        }
    }
}