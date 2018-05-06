// <copyright file="JuvoClientTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Juvo.Tests.Juvo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using JuvoProcess;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.IO;
    using Microsoft.AspNetCore.Hosting;
    using Moq;
    using Xunit;

    public class JuvoClientTests
    {
        [Fact]
        public void CreateInstanceAndRun()
        {
            var instance = new JuvoClient(
                this.GetWindowsConfiguration(),
                new Mock<IDiscordBotFactory>().Object,
                new Mock<IIrcBotFactory>().Object,
                new Mock<SlackBotFactory>().Object,
                new Mock<ILogManager>().Object,
                new Mock<IWebHost>().Object,
                new Mock<IStorageHandler>().Object,
                new ManualResetEvent(false));
            Assert.NotNull(instance);

            instance.Run().Wait(1000);
        }

        private Config GetWindowsConfiguration()
        {
            return new Config
            {
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
                System = new SystemInfo
                {
                    AppDataPath = new DirectoryInfo(@"z:\some\fake\path\app"),
                    LocalAppDataPath = new DirectoryInfo(@"z:\some\fake\path\app\local"),
                    Os = OperatingSystem.Windows
                },
                WebServer = new WebServerConfig
                {
                    Enabled = false
                }
            };
        }
    }
}
