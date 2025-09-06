using Microsoft.AspNetCore.SignalR;

namespace Career.Api.Hubs
{
    public class CustomUserProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var context = connection.GetHttpContext();

            if (context is null)
                return null;

            string? token = context.Request.Query["access_token"];

            return token;
        }
    }
}
