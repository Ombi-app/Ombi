using System;
using NUnit.Framework;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Tests.Entities
{
    [TestFixture]
    public class MovieRequestsTests
    {
        private static MovieRequests StandardRequest() => new MovieRequests { RequestedDate = DateTime.UtcNow };

        private static MovieRequests FourKRequest() => new MovieRequests
        {
            Has4KRequest = true,
            RequestedDate = default,
            RequestedDate4k = DateTime.UtcNow
        };

        private static MovieRequests CombinedRequest() => new MovieRequests
        {
            Has4KRequest = true,
            RequestedDate = DateTime.UtcNow,
            RequestedDate4k = DateTime.UtcNow
        };

        [Test]
        public void RequestStatus_StandardRequest_PendingApproval()
        {
            Assert.That(StandardRequest().RequestStatus, Is.EqualTo("Common.PendingApproval"));
        }

        [Test]
        public void RequestStatus_StandardRequest_Processing_WhenApproved()
        {
            var request = StandardRequest();
            request.Approved = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.ProcessingRequest"));
        }

        [Test]
        public void RequestStatus_StandardRequest_Available()
        {
            var request = StandardRequest();
            request.Approved = true;
            request.Available = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.Available"));
        }

        [Test]
        public void RequestStatus_StandardRequest_Denied()
        {
            var request = StandardRequest();
            request.Denied = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.Denied"));
        }

        [Test]
        public void RequestStatus_4KRequest_PendingApproval4K()
        {
            Assert.That(FourKRequest().RequestStatus, Is.EqualTo("Common.PendingApproval4K"));
        }

        [Test]
        public void RequestStatus_4KRequest_Processing4K_WhenApproved4K()
        {
            var request = FourKRequest();
            request.Approved4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.ProcessingRequest4K"));
        }

        /// <summary>
        /// A 4K only request never sets Available, so reading the status from that flag
        /// alone left a fulfilled request reading as "Pending Approval" forever.
        /// </summary>
        [Test]
        public void RequestStatus_4KRequest_Available4K_WhenFulfilled()
        {
            var request = FourKRequest();
            request.Approved4K = true;
            request.Available4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.Available4K"));
        }

        [Test]
        public void RequestStatus_4KRequest_Denied4K()
        {
            var request = FourKRequest();
            request.Denied4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.RequestDenied4K"));
        }

        [Test]
        public void RequestStatus_CombinedRequest_ReportsStandardWhileItIsOutstanding()
        {
            var request = CombinedRequest();
            request.Approved = true;
            request.Approved4K = true;
            request.Available4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.ProcessingRequest"));
        }

        [Test]
        public void RequestStatus_CombinedRequest_FallsThroughTo4K_OnceStandardIsAvailable()
        {
            var request = CombinedRequest();
            request.Approved = true;
            request.Available = true;
            request.Approved4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.ProcessingRequest4K"));
        }

        [Test]
        public void RequestStatus_CombinedRequest_Available_WhenBothFulfilled()
        {
            var request = CombinedRequest();
            request.Approved = true;
            request.Available = true;
            request.Approved4K = true;
            request.Available4K = true;

            Assert.That(request.RequestStatus, Is.EqualTo("Common.Available"));
        }

        [Test]
        public void RequestStatus_IsNeverEmpty()
        {
            Assert.Multiple(() =>
            {
                Assert.That(StandardRequest().RequestStatus, Is.Not.Empty);
                Assert.That(FourKRequest().RequestStatus, Is.Not.Empty);
                Assert.That(CombinedRequest().RequestStatus, Is.Not.Empty);
            });
        }
    }
}
