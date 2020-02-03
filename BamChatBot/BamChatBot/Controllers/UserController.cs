using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Bots;
using BamChatBot.Dialogs;
using BamChatBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
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
		protected readonly BotState UserState;

		public UserController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration,  ConcurrentDictionary<string, ConversationReference> conversationReferences, IBot bot, UserState userState)
		{
			_adapter = adapter;
			_appId = configuration["MicrosoftAppId"];
			_conversationReferences = conversationReferences;
			_bot = bot;
			_user = new User();
			UserState = userState;
			
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]  User user)
		{
			/*var _userAccessor = ((DialogAndWelcomeBot<MainDialog>)_bot)._userAccessor;
			await _adapter.ProcessAsync(Request, Response, _bot);*/
			_user = user;
			
			/*var conversationReference = new ConversationReference
			{
				ActivityId = "",
				Bot = new ChannelAccount(),
				ChannelId = "",
				Conversation = new ConversationAccount(),
				ServiceUrl= "",
				User = new ChannelAccount()
			};*/
			foreach (var conversationReference in _conversationReferences.Values)
			{
			  await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
			}

			//await _adapter.ProcessAsync(Request, Response, _bot);
			return  new ContentResult()
			{
				Content = "<html><body><h1>User " + user.Name + " " + _conversationReferences.Values.Count+" has joined.</h1></body></html>",
				ContentType = "text/html",
				StatusCode = (int)HttpStatusCode.OK,
			};
		}

		

			private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
		{
			var serviceUrl = turnContext.Activity.ServiceUrl;
			// If you encounter permission-related errors when sending this message, see
			// https://aka.ms/BotTrustServiceUrl
			MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);

			var userStateAccessors = UserState.CreateProperty<User>(nameof(User));
			var user = await userStateAccessors.GetAsync(turnContext, () => new User());
			user.Name = _user.Name;
			user.UserId = _user.UserId;
			await userStateAccessors.SetAsync(turnContext, user, cancellationToken);
			await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
		}
	}
}