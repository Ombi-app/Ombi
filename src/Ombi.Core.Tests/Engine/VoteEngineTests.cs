using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class VoteEngineTests
    {
        [SetUp]
        public void Setup()
        {
            VoteRepository = new Mock<IRepository<Votes>>();
            VoteSettings = new Mock<ISettingsService<VoteSettings>>();
            MusicRequestEngine = new Mock<IMusicRequestEngine>();
            TvRequestEngine = new Mock<ITvRequestEngine>();
            MovieRequestEngine = new Mock<IMovieRequestEngine>();
            MovieRequestEngine = new Mock<IMovieRequestEngine>();
            User = new Mock<IPrincipal>();
            UserManager = new Mock<OmbiUserManager>();
            Rule = new Mock<IRuleEvaluator>();
            Engine = new VoteEngine(VoteRepository.Object, User.Object, UserManager.Object, Rule.Object, VoteSettings.Object, MusicRequestEngine.Object,
                TvRequestEngine.Object, MovieRequestEngine.Object);
        }

        public VoteEngine Engine { get; set; }
        public Mock<IPrincipal> User { get; set; }
        public Mock<OmbiUserManager> UserManager { get; set; }
        public Mock<IRuleEvaluator> Rule { get; set; }
        public Mock<IRepository<Votes>> VoteRepository { get; set; }
        public Mock<ISettingsService<VoteSettings>> VoteSettings { get; set; }
        public Mock<IMusicRequestEngine> MusicRequestEngine { get; set; }
        public Mock<ITvRequestEngine> TvRequestEngine { get; set; }
        public Mock<IMovieRequestEngine> MovieRequestEngine { get; set; }

        [Test]
        public async Task New_Upvote()
        {
            var votes = new List<Votes>();
            AutoFi
            VoteRepository.Setup(x => x.GetAll()).Returns(new EnumerableQuery<Votes>())
            var result = await Engine.UpVote(1, RequestType.Movie);

        }
    }
}