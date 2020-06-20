using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class VoteEngineTests
    {
        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            VoteRepository = new Mock<IRepository<Votes>>();
            VoteSettings = new Mock<ISettingsService<VoteSettings>>();
            MusicRequestEngine = new Mock<IMusicRequestEngine>();
            TvRequestEngine = new Mock<ITvRequestEngine>();
            MovieRequestEngine = new Mock<IMovieRequestEngine>();
            MovieRequestEngine = new Mock<IMovieRequestEngine>();
            User = new Mock<IPrincipal>();
            User.Setup(x => x.Identity.Name).Returns("abc");
            UserManager = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { Id = "abc", UserName = "abc", NormalizedUserName = "ABC" } });
            Rule = new Mock<IRuleEvaluator>();
            Engine = new VoteEngine(VoteRepository.Object, User.Object, UserManager.Object, Rule.Object, VoteSettings.Object, MusicRequestEngine.Object,
                TvRequestEngine.Object, MovieRequestEngine.Object);

            F.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
    .ForEach(b => F.Behaviors.Remove(b));
            F.Behaviors.Add(new OmitOnRecursionBehavior());


        }

        public Fixture F { get; set; }
        public VoteEngine Engine { get; set; }
        public Mock<IPrincipal> User { get; set; }
        public Mock<OmbiUserManager> UserManager { get; set; }
        public Mock<IRuleEvaluator> Rule { get; set; }
        public Mock<IRepository<Votes>> VoteRepository { get; set; }
        public Mock<ISettingsService<VoteSettings>> VoteSettings { get; set; }
        public Mock<IMusicRequestEngine> MusicRequestEngine { get; set; }
        public Mock<ITvRequestEngine> TvRequestEngine { get; set; }
        public Mock<IMovieRequestEngine> MovieRequestEngine { get; set; }

        [TestCaseSource(nameof(VoteData))]
        public async Task Vote(VoteType type, RequestType request)
        {
            VoteSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new VoteSettings
            {
                Enabled = true,
                MovieVoteMax = 10
            });
            var votes = F.CreateMany<Votes>().ToList();

            VoteRepository.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Votes>(votes)
                .AsQueryable()
                .BuildMock().Object);
            var result = new VoteEngineResult();
            if (type == VoteType.Downvote)
            {
                result = await Engine.DownVote(1, request);
            }
            else
            {
                result = await Engine.UpVote(1, request);
            }

            Assert.That(result.Result, Is.True);
            VoteRepository.Verify(x => x.Add(It.Is<Votes>(c => c.UserId == "abc" && c.VoteType == type)), Times.Once);
            VoteRepository.Verify(x => x.Delete(It.IsAny<Votes>()), Times.Never);
            MovieRequestEngine.Verify(x => x.ApproveMovieById(1), Times.Never);
        }
        public static IEnumerable<TestCaseData> VoteData
        {

            get
            {
                yield return new TestCaseData(VoteType.Upvote, RequestType.Movie).SetName("Movie_Upvote");
                yield return new TestCaseData(VoteType.Downvote, RequestType.Movie).SetName("Movie_Downvote");
                yield return new TestCaseData(VoteType.Upvote, RequestType.TvShow).SetName("Tv_Upvote");
                yield return new TestCaseData(VoteType.Downvote, RequestType.TvShow).SetName("Tv_Downvote");
            }
        }


        [TestCaseSource(nameof(AttemptedTwiceData))]
        public async Task Attempted_Twice(VoteType type, RequestType request)
        {
            VoteSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new VoteSettings
            {
                Enabled = true,
                MovieVoteMax = 10
            });
            var votes = F.CreateMany<Votes>().ToList();
            votes.Add(new Votes
            {
                RequestId = 1,
                RequestType = RequestType.Movie,
                UserId = "abc",
                VoteType = type
            });
            VoteRepository.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Votes>(votes)
                .AsQueryable()
                .BuildMock().Object);
            var result = new VoteEngineResult();
            if (type == VoteType.Downvote)
            {
                result = await Engine.DownVote(1, request);
            }
            else
            {
                result = await Engine.UpVote(1, request);
            }

            Assert.That(result.Result, Is.False);
            VoteRepository.Verify(x => x.Delete(It.IsAny<Votes>()), Times.Never);
            MovieRequestEngine.Verify(x => x.ApproveMovieById(1), Times.Never);
        }
        public static IEnumerable<TestCaseData> AttemptedTwiceData
        {

            get
            {
                yield return new TestCaseData(VoteType.Upvote, RequestType.Movie).SetName("Upvote_Attemped_Twice_Movie");
                yield return new TestCaseData(VoteType.Downvote, RequestType.Movie).SetName("Downvote_Attempted_Twice_Movie");
                yield return new TestCaseData(VoteType.Upvote, RequestType.TvShow).SetName("Upvote_Attemped_Twice_Tv");
                yield return new TestCaseData(VoteType.Downvote, RequestType.TvShow).SetName("Downvote_Attempted_Twice_Tv");
            }
        }

        [TestCaseSource(nameof(VoteConvertData))]
        public async Task Downvote_Converted_To_Upvote(VoteType type, RequestType request)
        {
            VoteSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new VoteSettings
            {
                Enabled = true,
                MovieVoteMax = 10
            });
            var votes = F.CreateMany<Votes>().ToList();
            votes.Add(new Votes
            {
                RequestId = 1,
                RequestType = request,
                UserId = "abc",
                VoteType = type == VoteType.Upvote ? VoteType.Downvote : VoteType.Upvote
            });
            VoteRepository.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Votes>(votes)
                .AsQueryable()
                .BuildMock().Object);

            var result = new VoteEngineResult();
            if (type == VoteType.Downvote)
            {
                result = await Engine.DownVote(1, request);
            }
            else
            {
                result = await Engine.UpVote(1, request);
            }
            Assert.That(result.Result, Is.True);
            VoteRepository.Verify(x => x.Delete(It.IsAny<Votes>()), Times.Once);
            VoteRepository.Verify(x => x.Add(It.Is<Votes>(v => v.VoteType == type)), Times.Once);
            MovieRequestEngine.Verify(x => x.ApproveMovieById(1), Times.Never);
        }
        public static IEnumerable<TestCaseData> VoteConvertData
        {

            get
            {
                yield return new TestCaseData(VoteType.Upvote, RequestType.Movie).SetName("Downvote_Converted_To_UpVote_Movie");
                yield return new TestCaseData(VoteType.Downvote, RequestType.Movie).SetName("UpVote_Converted_To_DownVote_Movie");
                yield return new TestCaseData(VoteType.Upvote, RequestType.TvShow).SetName("Downvote_Converted_To_UpVote_TvShow");
                yield return new TestCaseData(VoteType.Downvote, RequestType.TvShow).SetName("UpVote_Converted_To_DownVote_TvShow");
            }
        }


        [TestCaseSource(nameof(VotingDisabledData))]
        public async Task Voting_Disabled(RequestType type)
        {
            VoteSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new VoteSettings
            {
                Enabled = false,
                MovieVoteMax = 10
            });

            var result = await Engine.UpVote(1, type);

            Assert.That(result.Result, Is.True);
            VoteRepository.Verify(x => x.Add(It.IsAny<Votes>()), Times.Never);
        }
        public static IEnumerable<TestCaseData> VotingDisabledData
        {
            get
            {
                yield return new TestCaseData(RequestType.Movie).SetName("Voting_Disabled_Movie");
                yield return new TestCaseData(RequestType.TvShow).SetName("Voting_Disabled_TV");
            }
        }
    }
}