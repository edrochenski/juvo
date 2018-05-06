// <copyright file="JuvoClientTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Juvo.Tests.Juvo
{
    using System.IO;
    using System.Threading;
    using JuvoProcess;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Moq;
    using Xunit;

    public class JuvoClientTests
    {
        [Fact]
        public void CreateInstance()
        {
            var instance = new JuvoClient(
                this.GetWindowsConfiguration(),
                new Mock<IDiscordBotFactory>().Object,
                new Mock<IIrcBotFactory>().Object,
                new Mock<SlackBotFactory>().Object,
                new Mock<ILogManager>().Object,
                new Mock<IWebHost>().Object,
                new ManualResetEvent(false));
            Assert.NotNull(instance);
        }

        private Config GetWindowsConfiguration()
        {
            return new Config
            {
                System = new SystemInfo
                {
                    AppDataPath = new DirectoryInfo(@"z:\some\fake\path\app"),
                    LocalAppDataPath = new DirectoryInfo(@"z:\some\fake\path\app\local"),
                    Os = OperatingSystem.Windows
                }
            };
        }
    }
}
