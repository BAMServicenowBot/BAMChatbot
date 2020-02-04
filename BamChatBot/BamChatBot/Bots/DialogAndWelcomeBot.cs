// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace BamChatBot.Bots
{
	public class DialogAndWelcomeBot<T> : DialogBot<T>
    where T : Dialog
{
		protected readonly BotState UserState;
		public readonly IStatePropertyAccessor<User> _userAccessor;

		public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, User user)
        : base(conversationState, userState, dialog, logger, conversationReferences, user)
        {
			UserState = userState;
			_userAccessor = userState.CreateProperty<User>("User");
		}

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
		
		foreach (var member in membersAdded)
        {
            // Greet anyone that was not the target (recipient) of this message.
            // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for morBe details.
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
					var user = new User();
					if (!string.IsNullOrEmpty(this._user.UserId))
					{
						user = user.GetUser(this._user.UserId);
						var cacheUser = await this._userAccessor.GetAsync(turnContext, () => new User());
						cacheUser.Name = user.Name;
						cacheUser.UserId = user.UserId;
						await this._userAccessor.SetAsync(turnContext, cacheUser, cancellationToken);
					}
					this.AddConversationReference(turnContext.Activity as Activity);		
					
                    //var _user = user.GetUser();
					var msg = string.Empty;
					if (!string.IsNullOrWhiteSpace(user.UserId))
                    {
						msg = "Hello " + user.Name + ", welcome to Bayview ChatBot!";
						
					}
                    else
                    {
						/*user.Name = "Dayamis Ruiz";
						user.UserId = "f8e33eb11b94b384dbc4c91e1e4bcb9b";
						await _userAccessor.SetAsync(turnContext, user, cancellationToken);*/
						msg = "Welcome to Bayview ChatBot!";
						// await turnContext.SendActivityAsync(MessageFactory.Text(_user.Name), cancellationToken);
					}
					await turnContext.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
					await turnContext.SendActivityAsync(MessageFactory.Text("What can I help you with today?"), cancellationToken);
				}
               
        }
    }

        private static async Task SendSuggestedActionsAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("Please, select from below options what you want.");
           
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
        {
            new CardAction() { Title = "Running RPA Bot", Type = ActionTypes.ImBack, Value = "Running RPA Bot" },
            new CardAction() { Title = "Check RPA Bot Status", Type = ActionTypes.ImBack, Value = "Check RPA Bot Status" }
           
        },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        // Load attachment from embedded resource.
        private Attachment CreateAdaptiveCardAttachment()
    {
        var cardResourcePath = "BamChatBot.Cards.welcomeCard.json";

        using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
        {
            using (var reader = new StreamReader(stream))
            {
                var adaptiveCard = reader.ReadToEnd();
                return new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(adaptiveCard),
                };
            }
        }
    }
}
}

