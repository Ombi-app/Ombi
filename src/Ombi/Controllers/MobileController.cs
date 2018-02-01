using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    public class MobileController : Controller
    {
        public MobileController(IRepository<NotificationUserId> notification, OmbiUserManager user)
        {
            _notification = notification;
            _userManager = user;
        }

        private readonly IRepository<NotificationUserId> _notification;
        private readonly OmbiUserManager _userManager;

        [HttpPost("Notification")]
        public async Task<IActionResult> AddNotitficationId([FromBody] NotificationIdBody body)
        {
            if (body?.PlayerId.HasValue() ?? false)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                // Check if we already have this notification id
                var alreadyExists = await _notification.GetAll().AnyAsync(x => x.PlayerId == body.PlayerId && x.UserId == user.Id);

                if (alreadyExists)
                {
                    return Ok();
                }

                // let's add it
                await _notification.Add(new NotificationUserId
                {
                    PlayerId = body.PlayerId,
                    UserId = user.Id,
                    AddedAt = DateTime.Now,
                });
                return Ok();
            }
            return BadRequest();
        }
    }
}