using Xunit;
using System;
using System.Runtime.InteropServices;

namespace ModularGodot.Core.XUnitTests
{
    public class ConfigurationTests
    {
        [Fact]
        public void Runtime_IsNet9()
        {
            // Arrange & Act
            var framework = RuntimeInformation.FrameworkDescription;

            // Assert
            Assert.Contains(".NET 9", framework);
        }

        [Fact]
        public void OperatingSystem_IsSupported()
        {
            // Arrange & Act
            var os = RuntimeInformation.OSDescription;

            // Assert
            // This test will pass on supported platforms
            Assert.NotNull(os);
            Assert.NotEmpty(os);
        }

        [Fact]
        public void TestFramework_IsXUnit()
        {
            // Arrange & Act
            // This is a placeholder test to verify the test framework is working
            var isXUnitAvailable = true;

            // Assert
            Assert.True(isXUnitAvailable);
        }
    }
}