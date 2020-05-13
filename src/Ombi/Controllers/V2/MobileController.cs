using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.CloudService;
using Ombi.Attributes;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Models.V2;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Controllers.V2
{
    [ApiV2]
    [Authorize]
    [Produces("application/json")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        public MobileController(IRepository<MobileDevices> mobileDevices, OmbiUserManager user, ICloudMobileNotification mobileNotificationService)
        {
            _mobileDevices = mobileDevices;
            _userManager = user;
            _mobileNotificationService = mobileNotificationService;
        }

        private readonly IRepository<MobileDevices> _mobileDevices;
        private readonly OmbiUserManager _userManager;
        private readonly ICloudMobileNotification _mobileNotificationService;

        [HttpPost("Notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AddNotitficationId([FromBody] MobileNotificationBody body)
        {
            if (!string.IsNullOrEmpty(body?.Token))
            {

                var username = User.Identity.Name.ToUpper();
                var user = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
                if (user == null)
                {
                    return Ok();
                }
                // Check if we already have this notification id
                var alreadyExists = await _mobileDevices.GetAll().AnyAsync(x => x.Token == body.Token && x.UserId == user.Id);

                if (alreadyExists)
                {
                    return Ok();
                }
                // Ensure we don't have too many already for this user
                var tokens = await _mobileDevices.GetAll().Where(x => x.UserId == user.Id).OrderBy(x => x.AddedAt).ToListAsync();
                if (tokens.Count() > 5)
                {
                    var toDelete = tokens.Take(tokens.Count() - 5);
                    await _mobileDevices.DeleteRange(toDelete);
                }

                await _mobileDevices.Add(new MobileDevices
                {
                    Token = body.Token,
                    UserId = user.Id,
                    AddedAt = DateTime.Now,
                });
                return Ok();
            }
            return BadRequest();
        }

        [HttpDelete("Notification")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> RemoveNotifications()
        {

            var username = User.Identity.Name.ToUpper();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
            // Check if we already have this notification id
            var currentDevices = await _mobileDevices.GetAll().Where(x => x.UserId == user.Id).ToListAsync();

            if (currentDevices == null || !currentDevices.Any())
            {
                return Ok();
            }

            await _mobileDevices.DeleteRange(currentDevices);

            return Ok();
        }


        [HttpPost("Send")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Admin]
        public async Task<IActionResult> SendNotification([FromBody] SendMobileNotification model)
        {

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == model.UserId);
            if (user == null)
            {
                return Ok();
            }
            // Check if we already have this notification id
            var currentDevices = await _mobileDevices.GetAll().Where(x => x.UserId == model.UserId).ToListAsync();

            if (currentDevices == null || !currentDevices.Any())
            {
                return Ok();
            }
            

            foreach (var d in currentDevices)
            {
                await _mobileNotificationService.SendMessage(new MobileNotificationRequest
                {
                    To = d.Token,
                    Body = model.Message,
                });
            }

            return Ok();
        }

        [HttpGet("Users")] [ApiExplorerSettings(IgnoreApi = true)]
        [Admin]
        public async Task<IActionResult> GetUsers()
        {
            var devices = await _mobileDevices.GetAll().Include(x => x.User).ToListAsync();
            var unique = devices.GroupBy(x => x.UserId,  (key, g) => new { UserId = key, Username = g.Select(x => x.User.UserName).FirstOrDefault(), Devices = g.ToList() });
            return Ok(unique);
        }
    }
}
