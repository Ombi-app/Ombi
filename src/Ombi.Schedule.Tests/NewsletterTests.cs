using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class NewsletterTests
    {
        [TestCaseSource(nameof(EpisodeListData))]
        public string BuildEpisodeListTest(List<int> episodes)
        {
            var emailSettings = new Mock<ISettingsService<EmailNotificationSettings>>();
            var customziation = new Mock<ISettingsService<CustomizationSettings>>();
            var newsletterSettings = new Mock<ISettingsService<NewsletterSettings>>();
            var newsletter = new NewsletterJob(null, null, null, null, null, null, customziation.Object, emailSettings.Object, null, null, newsletterSettings.Object, null);

            var ep = new List<int>();
            foreach (var i in episodes)
            {
                ep.Add(i);
            }
            var result = newsletter.BuildEpisodeList(ep);
            return result;
        }

        public static IEnumerable<TestCaseData> EpisodeListData
        {
            get
            {
                yield return new TestCaseData(new List<int>{1,2,3,4,5,6}).Returns("1-6").SetName("Simple 1-6");
                yield return new TestCaseData(new List<int>{1,2,3,4,5,6,8,9}).Returns("1-6, 8-9").SetName("Simple 1-6, 8-9");
                yield return new TestCaseData(new List<int>{1,99,101,555,468,469}).Returns("1, 99, 101, 555, 468-469").SetName("More Complex");
                yield return new TestCaseData(new List<int>{1}).Returns("1").SetName("Single Episode");
            }
        }
    }
}