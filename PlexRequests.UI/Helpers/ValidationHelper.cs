#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ValidationHelper.cs
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
using System.Linq;
using System.Text;
using Nancy.Validation;


using PlexRequests.UI.Models;

namespace PlexRequests.UI.Helpers
{
    public static class ValidationHelper
    {

        /// <summary>
        /// This will send the first error as a JsonResponseModel
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static JsonResponseModel SendJsonError(this ModelValidationResult result)
        {
            var errors = result.Errors;
            return errors
                .Select(e => e.Value.FirstOrDefault())
                .Where(modelValidationError => modelValidationError != null)
                .Select(modelValidationError =>
                new JsonResponseModel
                {
                    Result = false,
                    Message = modelValidationError.ErrorMessage
                })
                .FirstOrDefault();
        }

        public static JsonResponseModel SendSonarrError(List<string> result)
        {

            var model = new JsonResponseModel {Result = false};
            if (!result.Any())
            {
                return model;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Errors from Sonarr: ");
            for (var i = 0; i < result.Count; i++)
            {
                if (i != result.Count - 1)
                {
                    sb.AppendLine(result[i] + ",");
                }
                else
                {
                    sb.AppendLine(result[i]);
                }
            }

            model.Message = sb.ToString();

            return model;
        }
    }
}