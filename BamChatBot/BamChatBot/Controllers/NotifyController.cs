using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Bots;
using BamChatBot.Dialogs;
using BamChatBot.Models;
using BamChatBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BamChatBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly IBot _bot;
		private ProcessStatus _processStatus;
		private readonly User _user;

		public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IBot bot, User user)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"];
            _conversationReferences = conversationReferences;
            _bot = bot;
			_user = user;
			_processStatus = new ProcessStatus();

            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]  ProcessStatus processStatus)
        {
			_processStatus = processStatus;
			var user = ((DialogBot<MainDialog>)_bot)._user;
			var conversationReferenceActivityIds = new List<string>();
			conversationReferenceActivityIds.Add(user.UserId);

			foreach (var conversationReference in _conversationReferences.Values)
            {
				//if (conversationReference.ActivityId == processStatus.ActivityId)
				//if(this._user.UserId == processStatus.ChatbotUser)
                //{
				    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
					//break;
				//}
				
			}
			var message = MessageFactory.Text("The " + _processStatus.Process + " process has finished. Here is the run status." + Environment.NewLine +
				"Start Time: " + _processStatus.Start + Environment.NewLine +
				"End Time: " + _processStatus.End + Environment.NewLine +
				"Status: " + _processStatus.State);//Exceptions
												   // Let the caller know proactive messages have been sent
			return new ContentResult()
            {
                Content = "<html><body><h1>Process has finished. User: "+string.Join(",", conversationReferenceActivityIds )+"User from SN "+ processStatus.ChatbotUser +"Conversation "+ _conversationReferences.Values.Count+"</h1></body></html>",
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
			//var user = await((DialogBot<MainDialog>)_bot)._userAccessor.GetAsync(turnContext, () => new User());
			var rpaService = new RPAService();
			var response = rpaService.GetUser(turnContext.Activity.Conversation.Id);
			var user = JsonConvert.DeserializeObject<List<User>>(response.Content);
			if (user[0].u_user == _processStatus.ChatbotUser)
			{
				var message = MessageFactory.Text("Process " + _processStatus.Process + " has finished with following status." + Environment.NewLine +
				"Status: " + _processStatus.State + Environment.NewLine +
				"Start Time: " + _processStatus.Start + Environment.NewLine +
				"End Time: " + _processStatus.End+Environment.NewLine+"To continue");
				message.SuggestedActions = new SuggestedActions
				{
					Actions = new List<CardAction>
					{
						new CardAction()
						{
							Value = "continue",
							Type = ActionTypes.PostBack,
							Title = "Click Here",
							Text = "Click Here",
							DisplayText = "Click Here"
						}
					}
				};
				await turnContext.SendActivityAsync(message);
			}
        }
    }
}