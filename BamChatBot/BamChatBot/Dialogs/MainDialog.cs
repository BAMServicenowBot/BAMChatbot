// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using BamChatBot.CognitiveModels;
using BamChatBot.Models;
using Microsoft.Bot.Builder.Dialogs.Choices;
using BamChatBot.Services;
using Newtonsoft.Json;

namespace BamChatBot.Dialogs
{
	public class MainDialog : ComponentDialog
	{
		private readonly ProcessRecognizer _luisRecognizer;
		protected readonly ILogger Logger;
		protected readonly IStatePropertyAccessor<User> _userAccessor;
		public readonly IStatePropertyAccessor<ConversationFlow> _conversationFlow;
		private ProcessDetails processDetails;

		// Dependency injection uses this constructor to instantiate MainDialog
		public MainDialog(ProcessRecognizer luisRecognizer, ILogger<MainDialog> logger, UserState userState, ConversationState conversationState)
			: base(nameof(MainDialog))
		{
			_luisRecognizer = luisRecognizer;
			Logger = logger;
			_userAccessor = userState.CreateProperty<User>(nameof(User));
			_conversationFlow = conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
			processDetails = new ProcessDetails();

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new StartProcessDialog(_userAccessor, _conversationFlow));
			AddDialog(new StatusDialog(_userAccessor));
			AddDialog(new StopProcessDialog(_userAccessor));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ActStepAsync,
				FinalStepAsync,
				ContinueStepAsync,
				RPASupportStepAsync
			}));

			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			
			//to continue the conversation
			if (stepContext.Options != null)
			{
				var txt = "Would you like to do something else related to RPA?";
				var processDetails = (ProcessDetails)stepContext.Options;
				var message = string.Empty;
				switch (processDetails.Action)
				{
					case "default":
						message = "I am sorry, I cannot help you with that right now but I'm working on it. Please try asking in a different way.";
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(message), }, cancellationToken);
					case "startOver":
						message = "What can I help you with today?";
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(message), }, cancellationToken);
					case "done":

					case "start":
						message = processDetails.ProcessSelected.Name + " process  has started, you will be notified when it finishes. Do you want to run another process?";
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), message),
							//Prompt = MessageFactory.Text(message),
							//Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
						}, cancellationToken);
					case "error":
						message = processDetails.Error + " " + txt;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), message),
							//Prompt = MessageFactory.Text(message),
							//Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
						}, cancellationToken);
					default:
						message = txt;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), message)
							//Prompt = MessageFactory.Text(message),
							//Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
						}, cancellationToken);
				}
			}
			else//first interaction
			{
				return await stepContext.NextAsync(null, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{

			var processDetails = new ProcessDetails();
			if (stepContext.Result != null)
			{
				var action = stepContext.Result.ToString();

				//if (stepContext.Result.GetType() == typeof(FoundChoice))
				//{
				//var action = (FoundChoice)stepContext.Result;
				if (action == "Yes")
				{
					//set rpa for LUIS to recognize it as RAP intent, and show the list of actions again
					stepContext.Context.Activity.Text = "rpa";
				}
				//}
				//}
			}
			return await RecognizeText(stepContext, cancellationToken);
		}

		private async Task<DialogTurnResult> RecognizeText(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var user = new User();

			var processDetails = new ProcessDetails();

			try
			{
				var luisResult = await _luisRecognizer.RecognizeAsync<Process>(stepContext.Context, cancellationToken);
				switch (luisResult.TopIntent().intent)
				{
					case Process.Intent.RPA:
						var choices = new List<Choice>();
						var rpaOptions = new RPAOptions();

						foreach (var option in rpaOptions.Options)
						{
							choices.Add(new Choice
							{
								Value = option,
								Action = new CardAction(ActionTypes.PostBack, option, null, option, option, value: option, null)
							});
						}

						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.HeroCard(choices, "Here is a list of your available commands."),//.ForChannel(stepContext.Context.Activity.ChannelId, choices, "Here is a list of your available commands.", null, ChoiceFactoryOptions.),
						}, cancellationToken);
					/*return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
					{
						Prompt = MessageFactory.Text("Here is a list of your available commands."),
						Choices = choices,
						Style = ListStyle.HeroCard
					}, cancellationToken);*/

					default:
						processDetails.Action = "default";
						// Catch all for unhandled intents
						var didntUnderstandMessageText = "I am sorry, I cannot help you with that right now but I'm working on it. Please try asking in a different way.";
						var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
						//start this dialog again
						return await stepContext.NextAsync(processDetails, cancellationToken);//.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);
				}
				// return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
			}
			catch (Exception ex)
			{
				var rpaService = new RPAService();
				var rpaSupport = rpaService.GetRPASupportOption();
				var choices = new List<Choice> { rpaSupport };
				processDetails.Action = "error";

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "To continue to run this bot, please contact RPA Support.")
																													 /*Prompt = MessageFactory.Text("To continue to run this bot, please contact RPA Support."),
																													 Choices = choices*/
				}, cancellationToken);
			}
		}

		// Shows a warning if the requested From or To cities are recognized as entities but they are not in the Airport entity list.
		// In some cases LUIS will recognize the From and To composite entities as a valid cities but the From and To Airport values
		// will be empty if those entity values can't be mapped to a canonical item in the Airport.
		private static async Task ShowWarningForUnsupportedCities(ITurnContext context, FlightBooking luisResult, CancellationToken cancellationToken)
		{
			var unsupportedCities = new List<string>();

			var fromEntities = luisResult.FromEntities;
			if (!string.IsNullOrEmpty(fromEntities.From) && string.IsNullOrEmpty(fromEntities.Airport))
			{
				unsupportedCities.Add(fromEntities.From);
			}

			var toEntities = luisResult.ToEntities;
			if (!string.IsNullOrEmpty(toEntities.To) && string.IsNullOrEmpty(toEntities.Airport))
			{
				unsupportedCities.Add(toEntities.To);
			}

			if (unsupportedCities.Any())
			{
				var messageText = $"Sorry but the following airports are not supported: {string.Join(',', unsupportedCities)}";
				var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
				await context.SendActivityAsync(message, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var rpaService = new RPAService();
			var result = rpaService.GetUser(stepContext.Context.Activity.Conversation.Id);
			var user = JsonConvert.DeserializeObject<List<User>>(result.Content);
			//var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);

			var option = string.Empty;
			try
			{
				this.processDetails = (ProcessDetails)stepContext.Result;

			}
			catch (Exception)
			{
				option = stepContext.Result.ToString();
			}
			if(this.processDetails.User != null)
			{
				this.processDetails.User.UserId = user[0].u_user;
			}
			else
			{
				this.processDetails.User = new User
				{
					UserId = user[0].u_user
				};
			}
			if (!string.IsNullOrEmpty(option))
			{
				switch (option)
				{
					case "Start Process":
						processDetails.Action = "start";
						return await stepContext.BeginDialogAsync(nameof(StartProcessDialog), processDetails, cancellationToken);
					case "Process Status":
						processDetails.Action = "check status";
						return await stepContext.BeginDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
					case "Stop a Process":
						processDetails.Action = "stop";
						return await stepContext.BeginDialogAsync(nameof(StopProcessDialog), processDetails, cancellationToken);
					case "Start Over":
						processDetails.Action = "startOver";
						return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
					case "Report an Issue":
						processDetails.Action = string.Empty;
						var choices = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=incident",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", "bam?id=rpa_new_request&type=incident", null)
							 } };
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "To Report an Issue. ")
							/*Prompt = MessageFactory.Text("To Report an Issue. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=incident",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", "bam?id=rpa_new_request&type=incident", null)
							 } }*/
						}, cancellationToken);
					case "Contact RPA Support":

						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), "You would like to contact RPA Support, is that correct?")
							/*Prompt = MessageFactory.Text("You would like to contact RPA Support, is that correct?"),
							Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })*/
						}, cancellationToken);
					case "Request an Enhancement":
						processDetails.Action = string.Empty;
						var options = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=enhancement",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", "bam?id=rpa_new_request&type=enhancement", null)
							 } };
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(options, "To Request an Enhancement")
							/*Prompt = MessageFactory.Text("To Request an Enhancement. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=enhancement",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", "bam?id=rpa_new_request&type=enhancement", null)
							 } }*/
						}, cancellationToken);
					case "Submit a New Idea":
						processDetails.Action = string.Empty;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(new List<Choice> { new Choice
							{
								Value = "bam?id=sc_cat_item&sys_id=a41ac289db7c6f0004b27709af9619a3",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", "bam?id=sc_cat_item&sys_id=a41ac289db7c6f0004b27709af9619a3", null)
							 } }, "To Submit an Idea")
							
						}, cancellationToken);
						
					case "Done":
						processDetails.Action = "done";
						return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
					//this happen if an exception
					case "RPASupport@bayview.com":
						processDetails.Action = string.Empty;
						return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
					//if type something instead of clicking an option
					default:
						return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
				}
			}
			else
			{//if luis not recognize what the user enter
				return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
			}
		}


		private async Task<DialogTurnResult> ContinueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = this.processDetails;
			processDetails.Action = string.Empty;
			var option = stepContext.Result.ToString();
			if (option == "Yes")
			{
				var choices = new List<Choice> { new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openEmail", "RPASupport@bayview.com", null)
							 } };
				var rpaService = new RPAService();
				var rpaSupport = rpaService.GetRPASupportOption();
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "To Contact RPA Support.")
					/*Prompt = MessageFactory.Text("To Contact RPA Support. "),
					Choices = new List<Choice> { new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openEmail", "RPASupport@bayview.com", null)
							 } }*/

				}, cancellationToken);
			}
			else
			{
				return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> RPASupportStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = this.processDetails;
			processDetails.Action = string.Empty;
			return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
		}
	}
}
