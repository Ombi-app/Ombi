using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Ombi.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Base class for the API contract tests. Exposes the shared in-memory host, an HttpClient and
    /// a few helpers, and resets the engine mocks between tests.
    /// </summary>
    public abstract class IntegrationTestBase
    {
        protected OmbiApiTestFactory Factory => SharedTestFixture.Factory;
        protected HttpClient Client;

        [OneTimeSetUp]
        public void CreateClient()
        {
            Client = Factory.CreateClient();
        }

        [SetUp]
        public void ResetMocks()
        {
            // Clear any setups/invocations from a previous test so each test is isolated.
            Factory.MovieEngineV2.Invocations.Clear();
            Factory.TvSearchEngineV2.Invocations.Clear();
            Factory.MultiSearchEngine.Invocations.Clear();
            Factory.MovieRequestEngine.Invocations.Clear();
            Factory.TvRequestEngine.Invocations.Clear();
            Factory.RottenTomatoesApi.Invocations.Clear();
            Factory.RecentlyAddedEngine.Invocations.Clear();
            Factory.PlexApi.Invocations.Clear();
        }

        [OneTimeTearDown]
        public void DisposeClient()
        {
            Client?.Dispose();
        }

        protected async Task<(HttpStatusCode status, string body)> GetAsync(string url)
        {
            using var resp = await Client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, body);
        }

        protected async Task<(HttpStatusCode status, string body)> PostJsonAsync(string url, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            using var resp = await Client.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, body);
        }

        protected async Task<(HttpStatusCode status, string body)> PutJsonAsync(string url, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");
            using var resp = await Client.PutAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, body);
        }

        /// <summary>Asserts the JSON object has a property with exactly this (case-sensitive) name.</summary>
        protected static void AssertHasProperty(JObject obj, string propertyName)
        {
            Assert.That(obj.ContainsKey(propertyName), Is.True,
                $"Expected JSON to contain property '{propertyName}'. Actual properties: [{string.Join(", ", obj.Properties().Select(p => p.Name))}]");
        }

        /// <summary>
        /// Asserts the JSON object contains every one of these (case-sensitive) property names. This is
        /// the contract the mobile app relies on - the keys and casing it deserializes into.
        /// </summary>
        protected static void AssertHasProperties(JObject obj, params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                AssertHasProperty(obj, name);
            }
        }

        protected static JObject AsObject(string body) => JObject.Parse(body);

        protected static JArray AsArray(string body) => JArray.Parse(body);
    }
}
