using NUnit.Framework;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Helpers;
using Ombi.Store.Entities;
using System.Collections.Generic;

namespace Ombi.Core.Tests
{
    [TestFixture]
    public class WatchProviderParserTests
    {
        [TestCase("GB", TestName = "UpperCase")]
        [TestCase("gb", TestName = "LowerCase")]
        [TestCase("gB", TestName = "MixedCase")]
        public void GetValidStreamData(string streamingCountry)
        {
            var result = WatchProviderParser.GetUserWatchProviders(new WatchProviders
            {
                Results = new Results
                {
                    GB = new WatchProviderData()
                    {
                        StreamInformation = new List<StreamData>
                        {
                            new StreamData
                            {
                                provider_name = "Netflix",
                                display_priority = 0,
                                logo_path = "logo",
                                provider_id = 8
                            }
                        }
                    }
                }
            }, new OmbiUser { StreamingCountry = streamingCountry });

            Assert.That(result[0].provider_name, Is.EqualTo("Netflix"));
        }

        [TestCase("GB", TestName = "Missing_UpperCase")]
        [TestCase("gb", TestName = "Missing_LowerCase")]
        [TestCase("gB", TestName = "Missing_MixedCase")]
        public void GetMissingStreamData(string streamingCountry)
        {
            var result = WatchProviderParser.GetUserWatchProviders(new WatchProviders
            {
                Results = new Results
                {
                    AR = new WatchProviderData()
                    {
                        StreamInformation = new List<StreamData>
                        {
                            new StreamData
                            {
                                provider_name = "Netflix",
                                display_priority = 0,
                                logo_path = "logo",
                                provider_id = 8
                            }
                        }
                    }
                }
            }, new OmbiUser { StreamingCountry = streamingCountry });

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetInvalidStreamData()
        {
            var result = WatchProviderParser.GetUserWatchProviders(new WatchProviders
            {
                Results = new Results
                {
                    AR = new WatchProviderData()
                    {
                        StreamInformation = new List<StreamData>
                        {
                            new StreamData
                            {
                                provider_name = "Netflix",
                                display_priority = 0,
                                logo_path = "logo",
                                provider_id = 8
                            }
                        }
                    }
                }
            }, new OmbiUser { StreamingCountry = "BLAH" });

            Assert.That(result, Is.Empty);
        }
    }
}
