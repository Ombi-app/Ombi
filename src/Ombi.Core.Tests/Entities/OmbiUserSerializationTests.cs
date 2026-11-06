using Newtonsoft.Json;
using NUnit.Framework;
using Ombi.Store.Entities;

namespace Ombi.Core.Tests.Entities
{
    [TestFixture]
    public class OmbiUserSerializationTests
    {
        private static OmbiUser CreateUser() => new OmbiUser
        {
            Id = "user-1",
            UserName = "someone",
            Email = "someone@example.com",
            UserAccessToken = "access-token-secret",
            MediaServerToken = "media-server-token-secret",
            PasswordHash = "password-hash-secret",
            SecurityStamp = "stamp-value-one",
            ConcurrencyStamp = "concurrency-stamp-secret",
        };

        [TestCase("access-token-secret")]
        [TestCase("media-server-token-secret")]
        [TestCase("password-hash-secret")]
        [TestCase("stamp-value-one")]
        [TestCase("concurrency-stamp-secret")]
        public void Serialized_User_DoesNotContain_Secret(string secret)
        {
            var json = JsonConvert.SerializeObject(CreateUser());

            Assert.That(json, Does.Not.Contain(secret));
        }

        [TestCase("userAccessToken")]
        [TestCase("mediaServerToken")]
        public void Serialized_User_DoesNotContain_TokenProperty(string property)
        {
            var json = JsonConvert.SerializeObject(CreateUser(), new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            Assert.That(json, Does.Not.Contain($"\"{property}\""));
        }

        [Test]
        public void Serialized_User_StillContains_PublicFields()
        {
            var json = JsonConvert.SerializeObject(CreateUser());

            Assert.That(json, Does.Contain("someone"));
        }
    }
}
