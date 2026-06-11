using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

// Assembly-level fixture: the concrete SQLite contexts only run their EF migrations once per
// process (a static guard), so all tests must share a single host/database instance.
[SetUpFixture]
public class SharedTestFixture
{
    public static OmbiApiTestFactory Factory { get; private set; }

    [OneTimeSetUp]
    public void GlobalSetUp()
    {
        Factory = new OmbiApiTestFactory();
        // Force the host to build (runs migrations + seeding) before any test executes.
        using (Factory.CreateClient())
        {
        }
    }

    [OneTimeTearDown]
    public void GlobalTearDown()
    {
        Factory?.Dispose();
    }
}
