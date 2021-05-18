using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StreamBadgerLogin.Models;

namespace StreamBadgerLogin
{
    public class OAuthCallback
    {
        public static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
        {
            context.Identity.AddClaim(new Claim("access_token", context.AccessToken));
            context.Identity.AddClaim(new Claim("refresh_token", context.RefreshToken));
        }
    }
}