using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Rule.Search
{
    public class AvailabilityRuleHelperTests
    {


        [Test]
        public void Is_Available_When_All_We_Have_All_Aired_Episodes()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = true
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(1), // Tomorrow!
                    Available = false
                }
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests> { new SeasonRequests { Episodes = episodes } }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.True);
        }

        [Test]
        public void Is_Available_When_All_We_Have_All_Aired_Episodes_With_Unknown_Dates()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = true
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, // Unknown date!
                    Available = false
                }
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests> { new SeasonRequests { Episodes = episodes } }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.True);
        }

        [Test]
        public void Is_PartlyAvailable_When_All_We_Have_Some_Aired_Episodes()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = true
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-14), // Yesterday
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, // Unknown date!
                    Available = false
                }
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests> { new SeasonRequests { Episodes = episodes } }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.False);
            Assert.That(model.PartlyAvailable, Is.True);
        }

        [Test]
        public void Is_SeasonAvailable_When_All_We_Have_All_Aired_Episodes_In_A_Season()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = true
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-14), // Yesterday
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, // Unknown date!
                    Available = false
                }
            };

            var availableEpisodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = true
                },
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests { Episodes = episodes },
                    new SeasonRequests { Episodes = availableEpisodes },
                }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.False);
            Assert.That(model.PartlyAvailable, Is.True);
            Assert.That(model.SeasonRequests[1].SeasonAvailable, Is.True);
        }

        [Test]
        public void Is_NotAvailable_When_All_We_Have_No_Aired_Episodes()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-1), // Yesterday
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.Now.AddDays(-14), 
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, // Unknown date!
                    Available = false
                }
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests> { new SeasonRequests { Episodes = episodes } }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.False);
            Assert.That(model.PartlyAvailable, Is.False);
        }
        [Test]
        public void Is_NotAvailable_When_All_Episodes_Are_Unknown()
        {
            var episodes = new List<EpisodeRequests>
            {
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue,
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, 
                    Available = false
                },
                new EpisodeRequests
                {
                    AirDate = DateTime.MinValue, // Unknown date!
                    Available = false
                }
            };

            var model = new SearchTvShowViewModel
            {
                SeasonRequests = new List<SeasonRequests> { new SeasonRequests { Episodes = episodes } }
            };
            AvailabilityRuleHelper.CheckForUnairedEpisodes(model);
            Assert.That(model.FullyAvailable, Is.False);
            Assert.That(model.PartlyAvailable, Is.False);
        }
    }
}