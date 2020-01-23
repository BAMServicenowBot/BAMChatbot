﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
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

		public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IBot bot)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"];
            _conversationReferences = conversationReferences;
            _bot = bot;

            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostProcessStatus(JObject jsonData)
        {
            dynamic json = jsonData;

            JObject jProcessStatus = json;

            _processStatus = jProcessStatus.ToObject<ProcessStatus>();
			var conversationReferenceActivityIds = new List<string>();

			foreach (var conversationReference in _conversationReferences.Values)
            {
				conversationReferenceActivityIds.Add(conversationReference.ActivityId);

				if (conversationReference.ActivityId == _processStatus.ActivityId)
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                }
            }
            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = "<html><body><h1>Process "+ _processStatus.Process+" appId "+_appId+ "_processStatus activityId " + _processStatus.ActivityId + "conversationReference.ActivityId " + conversationReferenceActivityIds.ToString() + " has finished.</h1></body></html>",
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
			var message = MessageFactory.Text("Hello, process " + _processStatus.Process + " has finished with following status."+Environment.NewLine+
				"Status: "+_processStatus.State +Environment.NewLine+
				"Start Time: "+_processStatus.Start+Environment.NewLine+
				"End Time: " + _processStatus.End);
			await turnContext.SendActivityAsync(message);
          
        }
    }
}