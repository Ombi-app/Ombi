using System;
using System.Linq;
using NUnit.Framework;
using Ombi.Controllers.V1;

namespace Ombi.Tests
{
    [TestFixture]
    public class MediaCleanupControllerRateLimitTests
    {
        private const string MutationPolicy = "MediaCleanupMutation";

        [TestCase(nameof(MediaCleanupController.RequestOwnRemoval))]
        [TestCase(nameof(MediaCleanupController.Nominate))]
        [TestCase(nameof(MediaCleanupController.Vote))]
        [TestCase(nameof(MediaCleanupController.Approve))]
        [TestCase(nameof(MediaCleanupController.Reject))]
        [TestCase(nameof(MediaCleanupController.Cancel))]
        public void MutationEndpoint_UsesMutationRateLimitPolicy(string methodName)
        {
            var method = typeof(MediaCleanupController).GetMethod(methodName);
            Assert.That(method, Is.Not.Null);

            var rateLimitAttribute = method.GetCustomAttributesData()
                .SingleOrDefault(x => x.AttributeType.FullName == "Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute");

            Assert.That(rateLimitAttribute, Is.Not.Null, $"{methodName} must be rate limited.");
            Assert.That(rateLimitAttribute.ConstructorArguments, Has.Count.EqualTo(1));
            Assert.That(rateLimitAttribute.ConstructorArguments[0].Value, Is.EqualTo(MutationPolicy));
        }
    }
}
