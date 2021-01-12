using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Controllers.V1
{
    [ApiV1]
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        public MobileController(IRepository<NotificationUserId> notification, OmbiUserManager user, ILogger<MobileController> log)
        {
            _notification = notification;
            _userManager = user;
            _log = log;
        }

        private readonly IRepository<NotificationUserId> _notification;
        private readonly OmbiUserManager _userManager;
        private readonly ILogger _log;

        [HttpPost("Notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddNotitficationId([FromBody] NotificationIdBody body)
        {
            if (body?.PlayerId.HasValue() ?? false)
            {
                var username = User.Identity.Name.ToUpper();
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
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

        [HttpGet("Notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Admin]
        public IEnumerable<MobileUsersViewModel> GetRegisteredMobileUsers()
        {
            var users = _userManager.Users.Include(x => x.NotificationUserIds).Where(x => x.NotificationUserIds.Any());

            var vm = new List<MobileUsersViewModel>();

            foreach (var u in users)
            {
                vm.Add(new MobileUsersViewModel
                {
                    UserId = u.Id,
                    Username = u.UserAlias,
                    Devices = u.NotificationUserIds.Count
                });
            }
            return vm;
        }

        public class RemoveUserModel
        {
            public string UserId { get; set; }
        }
        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Admin]
        public async Task<bool> RemoveUser([FromBody] RemoveUserModel userId)
        {
            var user = await _userManager.Users.Include(x => x.NotificationUserIds).FirstOrDefaultAsync(x => x.Id.Equals(userId.UserId, StringComparison.InvariantCultureIgnoreCase));
            try
            {
                await _notification.DeleteRange(user.NotificationUserIds);
                return true;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not delete user notification");
            }

            return false;
        }
    }
}