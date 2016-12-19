#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RequestedModelDataProvider.cs
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

using Nancy.Swagger;
using Nancy.Swagger.Services;
using Ombi.Store;

namespace Ombi.UI.ModelDataProviders
{
    public class RequestedModelDataProvider : ISwaggerModelDataProvider
    {
        /// <summary>
        /// Gets the model data for the api documentation.
        /// </summary>
        /// <returns></returns>
        public SwaggerModelData GetModelData()
        {
            return SwaggerModelData.ForType<RequestedModel>(
                with =>
                {
                    with.Property(x => x.Title).Description("The requests title e.g. Star Wars Episode III").Required(true);
                    with.Property(x => x.AdminNote).Description("A note left by the administrator");
                    with.Property(x => x.Approved).Description("true or false if the request is approved").Required(true).Default(false);
                    with.Property(x => x.ArtistId).Description("The artist ID (if this request is for Headphones then it is required)");
                    with.Property(x => x.ArtistName).Description("The artist name (if this request is for Headphones then it is required)");
                    with.Property(x => x.Available).Description("If the request is available on Plex").Default(false);
                    with.Property(x => x.CanApprove).Description("Ignore");
                    with.Property(x => x.ImdbId).Description("The IMDB id of the request").Required(true);
                    with.Property(x => x.Issues)
                        .Description(
                            "The issue type,  None = 99, WrongAudio = 0, NoSubtitles = 1, WrongContent = 2, PlaybackIssues = 3, Other = 4. Use Other(4) when leaving an issue note");
                    with.Property(x => x.MusicBrainzId).Description("The MusicBrainz ID of the album request (if this request is for Headphones then it is required)");
                    with.Property(x => x.OtherMessage)
                        .Description("The issue message left by the user. The Issues property needs to be set to Other (4) for this to work correctly");
                    with.Property(x => x.PosterPath).Description("The poster path for the request").Required(true);
                    with.Property(x => x.ProviderId)
                        .Description("The TVMaze/TheMovieDB Id for the request depending if it's a movie request or Tv request")
                        .Required(true);
                    with.Property(x => x.ReleaseDate).Description("The release date of the request").Required(true);
                    with.Property(x => x.RequestedDate)
                        .Description("The date if the request, if this is not set, the request date will be set at the time of the Api call");
                    with.Property(x => x.RequestedUsers).Description("A collection of the requested users").Required(true);
                    with.Property(x => x.Type).Description("The type of request: Movie = 0, TvShow = 1, Album = 2").Required(true);
                    with.Property(x => x.Id).Description("The request Id (Only use for deleting)");
                });
        }
    }
}