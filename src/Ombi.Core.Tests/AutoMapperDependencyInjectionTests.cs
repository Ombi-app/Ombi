using System;
using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Ombi.Core.Models.UI;
using Ombi.Mapping;

namespace Ombi.Core.Tests
{
    [TestFixture]
    public class AutoMapperDependencyInjectionTests
    {
        [Test]
        public void AddOmbiMappingProfile_RegistersCustomTypeConverters()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddOmbiMappingProfile();

            using var provider = services.BuildServiceProvider();
            var mapper = provider.GetRequiredService<IMapper>();

            var mappedDate = mapper.Map<DateTime>("2026-10-01");
            var mappedClaim = mapper.Map<ClaimCheckboxes>(new Claim(ClaimTypes.Role, "Admin"));

            Assert.That(mappedDate, Is.EqualTo(new DateTime(2026, 10, 1)));
            Assert.That(mappedClaim.Enabled, Is.True);
            Assert.That(mappedClaim.Value, Is.EqualTo("Admin"));
        }
    }
}
