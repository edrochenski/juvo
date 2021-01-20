// <copyright file="ProgramTests.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace Juvo.Tests
{
    using JuvoProcess;
    using Moq;
    using Xunit;

    public class ProgramTests
    {
        [Fact]
        public void CreateInstance()
        {
            var program = new Program(
                new Mock<IJuvoClient>().Object);
            Assert.NotNull(program);
        }
    }
}
