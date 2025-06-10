using CommonUtilities.Fakers.Requests;
using Profile.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.WebTests
{
    [Collection(nameof(CollectionTest))]
    public class UpdaterUserAddressTests
    {
        private readonly ConfigureWebApiTests _app;
        public const string Endpoint = "api/users/update-address";

        public UpdaterUserAddressTests(ConfigureWebApiTests app)
        {
            _app = app;
        }

        [Fact]
        public async Task UserNotAuthenticated_Error()
        {
            //Arrange
            var request = UpdateAddressRequestFaker.Build();

            //Act
            var client = _app.CreateClient();
            var response = await client.PutAsJsonAsync(Endpoint, request);
            var content = await response.Content.ReadAsStringAsync();
            
            //Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains(ResourceExceptMessages.USER_NOT_AUTHENTICATED, content);
        }

        [Fact]
        public async Task Success()
        {
            //Arrange
            var request = UpdateAddressRequestFaker.Build();

            //Act
            var client = await _app.GenerateClientWithToken();
            var response = await client.PutAsJsonAsync(Endpoint, request);
            var content = await response.Content.ReadAsStreamAsync();
            var toJson = JsonDocument.Parse(content);

            //Assert
            Assert.Equal(toJson.RootElement.GetProperty("country").GetString(), request.Country);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
