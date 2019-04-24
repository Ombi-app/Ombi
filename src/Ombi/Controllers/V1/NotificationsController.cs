#region Copyright
// /************************************************************************
//    Copyright (c) 2018 Jamie Rees
//    File: NotificationsController.cs
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Attributes;
using Ombi.Core.Models;
using Ombi.Core.Senders;

namespace Ombi.Controllers.V1
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        public NotificationsController(IMassEmailSender sender)
        {
            _sender = sender;
        }

        private readonly IMassEmailSender _sender;

        [HttpPost("massemail")]
        public async Task<bool> SendMassEmail([FromBody]MassEmailModel model)
        {
            return await _sender.SendMassEmail(model);
        }

    }
}