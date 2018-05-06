// <copyright file="CommandSourceTests.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace Juvo.Tests.Bots
{
    using JuvoProcess.Bots;
    using Xunit;

    public class CommandSourceTests
    {
        private static CommandSource[] sources = new[]
        {
            new CommandSource { Identifier = "testchannel", SourceType = CommandSourceType.ChannelOrGroup },
            new CommandSource { Identifier = "testchannel", SourceType = CommandSourceType.ChannelOrGroup },
            new CommandSource { Identifier = "testchannel", SourceType = CommandSourceType.Message },
            new CommandSource { Identifier = "newchannel", SourceType = CommandSourceType.ChannelOrGroup }
        };

        [Fact]
        public void EqualsReturnsCorrectResult()
        {
            Assert.True(sources[0].Equals(sources[1]));
            Assert.True(sources[1].Equals(sources[0]));
            Assert.True(sources[0] == sources[1]);

            Assert.False(sources[0].Equals(sources[2]));
            Assert.False(sources[1].Equals(sources[3]));
            Assert.False(sources[0] == sources[3]);
            Assert.False(sources[1] == sources[2]);

            Assert.True(sources[0] != sources[2]);
            Assert.True(sources[1] != sources[3]);
            Assert.False(sources[0] != sources[1]);
        }

        [Fact]
        public void GetHashCodeReturnsConsistentResults()
        {
            Assert.True(sources[0].GetHashCode() == sources[1].GetHashCode());
            Assert.True(sources[2].GetHashCode() != sources[3].GetHashCode());
        }
    }
}
