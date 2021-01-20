// <copyright file="JuvoClientTests.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace Juvo.Tests.Juvo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using JuvoProcess;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.IO;
    using JuvoProcess.Net;
    using Microsoft.AspNetCore.Hosting;
    using Moq;
    using Xunit;

    public class JuvoClientTests
    {
        [Fact]
        public void CreateInstanceAndRun()
        {
            var storageMock = new Mock<IStorageHandler>();
            storageMock.Setup<string>(m => m.FileReadAllText(It.IsAny<string>())).Returns(string.Empty);

            var instance = new JuvoClient(
                this.GetWindowsConfiguration(),
                new Mock<IServiceProvider>().Object,
                new Mock<IDiscordBotFactory>().Object,
                new Mock<IIrcBotFactory>().Object,
                new Mock<SlackBotFactory>().Object,
                new Mock<ILogManager>().Object,
                new Mock<IWebHostBuilder>().Object,
                storageMock.Object,
                new Mock<IHttpClient>().Object,
                new ManualResetEvent(false));
            Assert.NotNull(instance);
            instance.Run().Wait(1000);
        }

        private Config GetWindowsConfiguration()
        {
            return new Config
            {
                Juvo = new JuvoConfig
                {
                    BasePath = "/just/can't/be/null",
                    DataPath = "/just/can't/be/null"
                },
                Discord = new DiscordConfig
                {
                    Connections = new List<DiscordConfigConnection>(0),
                    Enabled = false
                },
                Irc = new IrcConfig
                {
                    Connections = new List<IrcConfigConnection>(0),
                    Enabled = false
                },
                Slack = new SlackConfig
                {
                    Connections = new List<SlackConfigConnection>(0),
                    Enabled = false
                },
                WebServer = new WebServerConfig
                {
                    Enabled = false
                }
            };
        }
    }
}
