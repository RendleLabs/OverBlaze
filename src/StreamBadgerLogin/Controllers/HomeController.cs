using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using MessagePack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StreamBadgerLogin.Models;

namespace StreamBadgerLogin.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConnectionMultiplexer redis, ILogger<HomeController> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("login/{sessionId}")]
        public async Task<IActionResult> Login(string sessionId)
        {
            Response.Cookies.Append("StreamBadger.SessionId", sessionId);
            if (User.Identity.IsAuthenticated)
            {
                await SaveSessionData(sessionId);
                return View();
            }

            _logger.LogInformation("Authenticating with Twitch...");
            return Challenge(TwitchAuthenticationDefaults.AuthenticationScheme);
        }

        private async Task SaveSessionData(string sessionId)
        {
            var db = _redis.GetDatabase();
            
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var accessToken = User.FindFirst("access_token")?.Value;
            var refreshToken = User.FindFirst("refresh_token")?.Value;
            
            var sessionData = new SessionData
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Id = id,
                Name = name,
            };
            
            var data = MessagePackSerializer.Serialize(sessionData);
            await db.StringSetAsync(sessionId, data);
            _logger.LogInformation($"SessionData '{sessionId}' persisted to Redis.");
        }

        public override SignOutResult SignOut()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpGet("ping/{sessionId}")]
        public async Task<ActionResult<SessionData>> Ping(string sessionId)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(sessionId);
            if (!value.HasValue)
            {
                return NotFound();
            }

            byte[] data = value;
            var sessionData = MessagePackSerializer.Deserialize<SessionData>(data);
            return sessionData;
        }
    }
}