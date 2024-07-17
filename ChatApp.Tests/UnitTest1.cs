using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using ChatApp;

namespace ChatApp.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private WebApplicationFactory<Startup> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [TestMethod]
        public async Task TestChatHubConnection()
        {
            // Arrange
            var client = _factory.CreateClient();
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("/chathub", options =>
                {
                    options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                })
                .Build();

            // Act
            await hubConnection.StartAsync();

            // Assert
            Assert.AreEqual(HubConnectionState.Connected, hubConnection.State);

            await hubConnection.StopAsync();
        }
    }
}
