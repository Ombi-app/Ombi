using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Rule;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Core.Services;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Rule.Request
{
    [TestFixture]
    public class RequestLimitRuleTests
    {
        private AutoMocker _mocker;
        private RequestLimitRule _subject;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<RequestLimitRule>();
        }

        [Test]
        public async Task MovieRule_No_Limit()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMovieRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = false
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Movie
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }

        [Test]
        public async Task MovieRule_Limit_NotReached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMovieRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 2,
                Remaining = 1
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Movie
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }


        [Test]
        public async Task MovieRule_Limit_Reached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMovieRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 1,
                Remaining = 0
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Movie
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(false));
        }
        [Test]
        public async Task MusicRule_No_Limit()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMusicRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = false
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Album
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }

        [Test]
        public async Task MusicRule_Limit_NotReached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMusicRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 2,
                Remaining = 1
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Album
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }


        [Test]
        public async Task MusicRule_Limit_Reached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingMusicRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 1,
                Remaining = 0
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.Album
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(false));
        }

        [Test]
        public async Task TvRule_No_Limit()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingTvRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = false
            });

            var result = await _subject.Execute(new Store.Entities.Requests.BaseRequest
            {
                RequestType = RequestType.TvShow
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }

        [Test]
        public async Task TvRule_Limit_NotReached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingTvRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 2,
                Remaining = 1
            });

            var result = await _subject.Execute(new ChildRequests
            {
                RequestType = RequestType.TvShow,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests()
                        }
                    }
                }
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(true));
        }


        [Test]
        public async Task TvRule_Limit_Reached()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingTvRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 1,
                Remaining = 0
            });

            var result = await _subject.Execute(new ChildRequests
            {
                RequestType = RequestType.TvShow,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests()
                        }
                    }
                }
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(false));
        }

        [Test]
        public async Task TvRule_Limit_Reached_ManyEpisodes()
        {
            var limitService = _mocker.GetMock<IRequestLimitService>();
            limitService.Setup(x => x.GetRemainingTvRequests(It.IsAny<OmbiUser>(), It.IsAny<DateTime>())).ReturnsAsync(new Models.RequestQuotaCountModel
            {
                HasLimit = true,
                Limit = 1,
                Remaining = 5
            });

            var result = await _subject.Execute(new ChildRequests
            {
                RequestType = RequestType.TvShow,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests(),
                            new EpisodeRequests(),
                            new EpisodeRequests(),
                        }
                    },
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests(),
                            new EpisodeRequests(),
                            new EpisodeRequests(),
                        }
                    }
                }
            });

            Assert.That(result, Is.InstanceOf<RuleResult>().With.Property(nameof(RuleResult.Success)).EqualTo(false));
        }
    }
}
