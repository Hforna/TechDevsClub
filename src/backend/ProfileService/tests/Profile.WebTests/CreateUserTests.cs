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
    public class CreateUserTests : IClassFixture<ConfigureWebApiTests>
    {
        private readonly ConfigureWebApiTests _app;

        public CreateUserTests(ConfigureWebApiTests app) => _app = app;

        [Fact]
        public async Task RequestValidatorError()
        {
            //Arrange
            var request = CreateUserRequestFaker.Build();
            request.Email = "notvalidemail.com";
            request.Password = "less";

            //Act
            var client = _app.CreateClient();
            var response = await client.PostAsJsonAsync("api/users/create", request);
            var content = await response.Content.ReadAsStringAsync();

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(ResourceExceptMessages.EMAIL_FORMAT, content);
        }
    }
}
