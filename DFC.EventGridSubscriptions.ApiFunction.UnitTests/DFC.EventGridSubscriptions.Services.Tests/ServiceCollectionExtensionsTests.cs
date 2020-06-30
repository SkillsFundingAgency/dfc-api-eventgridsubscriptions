using DFC.EventGridSubscriptions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DFC.EventGridSubscriptions.ApiFunction.UnitTests.DFC.EventGridSubscriptions.Services.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void ServiceCollectionWhenAddKeyVaultClientAddsKeyVaultService()
        {
            //Arrange
            //Act
            var sc = new ServiceCollection();
            sc.AddKeyVaultClient("http://somekeyvaultaddress.com");
            var sp = sc.BuildServiceProvider();

            //Assert
            Assert.IsAssignableFrom<IKeyVaultService>(sp.GetRequiredService<IKeyVaultService>());
        }
    }
}
