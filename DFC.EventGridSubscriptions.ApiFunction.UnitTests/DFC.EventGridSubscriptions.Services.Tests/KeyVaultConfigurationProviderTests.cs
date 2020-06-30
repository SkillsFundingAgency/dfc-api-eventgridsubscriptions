using DFC.EventGridSubscriptions.Services.Extensions;
using DFC.EventGridSubscriptions.Services.Interface;
using DFC.EventGridSubscriptions.Services.Providers;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Services.Tests
{
    public class KeyVaultConfigurationProviderTests
    {
        private readonly IKeyVaultService keyVaultService = A.Fake<IKeyVaultService>();

        [Fact]
        public void KeyVaultConfigurationProviderWhenBuiltWithKeysBuildsProvider()
        {
            //Arrange
            A.CallTo(() => keyVaultService.GetSecretAsync(A<string>.Ignored)).Returns("a-secret-value");
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IKeyVaultService>(keyVaultService);

            var configurationBuilder = new ConfigurationBuilder().AddKeyVaultConfigurationProvider(new List<string>() { "a", "b", "c", "d" }, serviceCollection.BuildServiceProvider());

            //Act
            var configuration = configurationBuilder.Build();

            //Assert
            A.CallTo(() => keyVaultService.GetSecretAsync(A<string>.Ignored)).MustHaveHappened(4, Times.Exactly);
            Assert.Equal("a-secret-value", configuration["a"]);
            Assert.Equal("a-secret-value", configuration["b"]);
            Assert.Equal("a-secret-value", configuration["c"]);
            Assert.Equal("a-secret-value", configuration["d"]);
        }

        [Fact]
        public void KeyVaultConfigurationProviderWhenBuiltWithoutKeysBuildsProviderNoValues()
        {
            //Arrange
            A.CallTo(() => keyVaultService.GetSecretAsync(A<string>.Ignored)).Returns("a-secret-value");
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IKeyVaultService>(keyVaultService);

            var configurationBuilder = new ConfigurationBuilder().AddKeyVaultConfigurationProvider(new List<string>() {}, serviceCollection.BuildServiceProvider());

            //Act
            var configuration = configurationBuilder.Build();

            //Assert
            A.CallTo(() => keyVaultService.GetSecretAsync(A<string>.Ignored)).MustNotHaveHappened();
        }
    }
}
