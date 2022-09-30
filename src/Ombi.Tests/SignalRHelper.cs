using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Collections.Generic;

namespace Ombi.Tests
{
    public class SignalRHelper
    {
        public static Mock<IHubContext<T>> MockHub<T>() where T : Hub
        {
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mockClientProxy.Object);

            var hubContext = new Mock<IHubContext<T>>();
            hubContext.Setup(x => x.Clients).Returns(() => mockClients.Object);

            return hubContext;
        }
    }
}
