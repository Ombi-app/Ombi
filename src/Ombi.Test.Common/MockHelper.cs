using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using Ombi.Core.Authentication;
using Ombi.Store.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Ombi.Test.Common
{
    public static class MockHelper
    {
        public static Mock<OmbiUserManager> MockUserManager(List<OmbiUser> ls)
        {
            var store = new Mock<IUserStore<OmbiUser>>();
            //var u = new OmbiUserManager(store.Object, null, null, null, null, null, null, null, null,null,null,null,null)
            var mgr = new Mock<OmbiUserManager>(store.Object, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<OmbiUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<OmbiUser>());

            var userMock = ls.AsQueryable().BuildMock();

            mgr.Setup(x => x.Users).Returns(userMock.Object);
            mgr.Setup(x => x.DeleteAsync(It.IsAny<OmbiUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<OmbiUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<OmbiUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }
    }
}
