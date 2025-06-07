using CommonUtilities.Fakers.Entities;
using CommonUtilities.Fakers.Requests;
using Microsoft.Extensions.DependencyInjection;
using Profile.Domain.Exceptions;
using Profile.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Profile.WebTests
{
    [Collection(nameof(CollectionTest))]
    public class CreateUserTests : IAsyncDisposable
    {
        private readonly ConfigureWebApiTests _app;

        public CreateUserTests(ConfigureWebApiTests app) => _app = app;

        public async ValueTask DisposeAsync()
        {
            await _app.DeleteTables();
        }

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

        [Fact]
        public async Task EmailExists()
        {
            var user = await _app.GetConfirmedUser();

            var request = CreateUserRequestFaker.Build();
            request.Email = user.Email;

            var client = _app.CreateClient();
            var response = await client.PostAsJsonAsync("api/users/create", request);
            var content = await response.Content.ReadAsStringAsync();

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(ResourceExceptMessages.EMAIL_EXISTS, content);
        }

        [Fact]
        public async Task Success()
        {
            var request = CreateUserRequestFaker.Build();

            var client = _app.CreateClient();
            var response = await client.PostAsJsonAsync("api/users/create", request);
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains(request.Email, content);
        }
    }
}
