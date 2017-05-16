using System;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Xunit;

namespace Ombi.Tests.Core
{
    public class UserIdentityManagerTests
    {
        public UserIdentityManagerTests()
        {
            UserRepo = new Mock<IUserRepository>();
            var mapper = new Mock<IMapper>();
            Manager = new UserIdentityManager(UserRepo.Object, mapper.Object);
        }
        private Mock<IUserRepository> UserRepo { get; }
        private IUserIdentityManager Manager { get; }

        [Fact]
        public async Task CredentialsValid()
        {
            UserRepo.Setup(x => x.GetUser("ABC")).ReturnsAsync(() => new User{Password = "4tDKIbNCZ0pMxNzSSjVbT6mG88o52x9jOixPEwQS9rg=", Salt = new byte[]{30,80,214,127,185,134,75,86,80,177,0,242,202,161,219,246}});
           
            Assert.True(await Manager.CredentialsValid("ABC", "ABC"));
        }
    }
}
