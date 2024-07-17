using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;
using ChatApp;

namespace ChatApp.IntegrationTests
{
    [TestClass]
    public class ChatsControllerIntegrationTests
    {
        private readonly WebApplicationFactory<ChatApp.Startup> _factory;

        public ChatsControllerIntegrationTests()
        {
            _factory = new WebApplicationFactory<ChatApp.Startup>();
        }

        [TestMethod]
        public async Task GetChats_ReturnsSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/chats");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
        }

        // Add more test methods as needed
    }
}
