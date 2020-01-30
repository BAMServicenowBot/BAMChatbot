using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BamChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace BamChatBot.Controllers
{
	[Route("api/user")]
	[ApiController]
	public class UserController : ControllerBase
    {
		private readonly IBotFrameworkHttpAdapter _adapter;
		private readonly string _appId;
		private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
		private readonly IBot _bot;
		private User _user;

		public UserController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IBot bot)
		{
			_adapter = adapter;
			_appId = configuration["MicrosoftAppId"];
			_conversationReferences = conversationReferences;
			_bot = bot;
			_user = new User();
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]  User user)
		{
			_user = user;
			
			return  new ContentResult()
			{
				Content = "<html><body><h1>User " + user.Name + " has joined.</h1></body></html>",
				ContentType = "text/html",
				StatusCode = (int)HttpStatusCode.OK,
			};
		}
	}
}