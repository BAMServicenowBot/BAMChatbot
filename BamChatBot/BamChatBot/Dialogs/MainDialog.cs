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
		//protected readonly IStatePropertyAccessor<User> _userAccessor;
		public readonly IStatePropertyAccessor<ConversationFlow> _conversationFlow;
		private ProcessDetails processDetails;

		// Dependency injection uses this constructor to instantiate MainDialog
		public MainDialog(ProcessRecognizer luisRecognizer, ILogger<MainDialog> logger, UserState userState, ConversationState conversationState)
			: base(nameof(MainDialog))
		{
			_luisRecognizer = luisRecognizer;
			Logger = logger;
			
			_conversationFlow = conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
			processDetails = new ProcessDetails();

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new StartProcessDialog(_conversationFlow));
			AddDialog(new StatusDialog());
			AddDialog(new EndConversationDialog());
			AddDialog(new StopProcessDialog());
			AddDialog(new SupportDialog());
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
				var txt = "Would you like to return to Main Menu?";
				var processDetails = (ProcessDetails)stepContext.Options;
				processDetails.AttemptCount = 0;
				var message = string.Empty;
				var value = JsonConvert.SerializeObject(new PromptOption { Id = "mainIntro", Value = "Yes" });
				var choices = new List<Choice> { new Choice
							{
								Value = "Yes",
								Action = new CardAction(ActionTypes.PostBack, "Main Menu", null, "Main Menu", "Main Menu", value: value, null)
							 } };
				switch (processDetails.Action)
				{
					case "default":
						message = "I am sorry, I do not understand your request. Please try asking a different way or type "+'"'+"Menu" + '"' +" for available options.";
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(message), }, cancellationToken);
					
					case "error":
						message = processDetails.Error + Environment.NewLine + txt;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, message),
							//Prompt = MessageFactory.Text(message),
							//Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
						}, cancellationToken);
					case "pastMenu":
						message = "Clicking or interacting with a previous menu option is not allowed!" + Environment.NewLine + txt;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, message),
							//Prompt = MessageFactory.Text(message),
							//Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
						}, cancellationToken);
						
					default:
						message = txt;
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, message)
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
				var promptOption = new PromptOption();
				try
				{
					promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
				}
				catch (Exception) { }

				if (!string.IsNullOrEmpty(promptOption.Id))
				{
					if (promptOption.Id != "mainIntro")
					{
						processDetails.Action = "pastMenu";
						return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
					}
					action = promptOption.Value;
				}
				//if (stepContext.Result.GetType() == typeof(FoundChoice))
				//{
				//var action = (FoundChoice)stepContext.Result;
				if (action.ToLower() == "yes" || action.ToLower() == "y")
				{
					//set rpa for LUIS to recognize it as RAP intent, and show the list of actions again
					stepContext.Context.Activity.Text = "rpa";
				}
				//}
				//}
				/*if (action == "No")
				{
					return await stepContext.ReplaceDialogAsync(nameof(EndConversationDialog), processDetails, cancellationToken);
				}*/
			}
			return await RecognizeText(stepContext, cancellationToken);
		}

		private async Task<DialogTurnResult> RecognizeText(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var user = new User();

			var processDetails = new ProcessDetails();

			try
			{
				var promptOption = new PromptOption();
				try
				{
					promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Context.Activity.Text);
				}
				catch (Exception) { }
				if (!string.IsNullOrEmpty(promptOption.Value))
				{
					stepContext.Context.Activity.Text = promptOption.Value;
				}

				var luisResult = await _luisRecognizer.RecognizeAsync<Process>(stepContext.Context, cancellationToken);
				switch (luisResult.TopIntent().intent)
				{
					case Process.Intent.RPA:
						var choices = new List<Choice>();
						var rpaOptions = new RPAOptions();

						foreach (var option in rpaOptions.Options)
						{
							var value = JsonConvert.SerializeObject(option);

							choices.Add(new Choice
							{
								Value = option.Value,
								Action = new CardAction(ActionTypes.PostBack, option.Value, null, option.Value, option.Value, value: value, null)
							});

						}

						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.HeroCard(choices, "Below is a list of available commands:"),//.ForChannel(stepContext.Context.Activity.ChannelId, choices, "Here is a list of your available commands.", null, ChoiceFactoryOptions.),
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
						var didntUnderstandMessageText = "I am sorry, I do not understand your request. Please try asking a different way or type " + '"' + "Menu" + '"' + " for available options.";
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
			var user = new List<User>();
			var rpaService = new RPAService();
			var option = string.Empty;
			try
			{
				this.processDetails = (ProcessDetails)stepContext.Result;
			}
			catch (Exception)
			{
				option = stepContext.Result.ToString();
			}
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "rpaSuport" && promptOption.Id != "RPAOptions")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				option = promptOption.Value;
			}
			var result = rpaService.GetUser(stepContext.Context.Activity.Conversation.Id);
			if (result.IsSuccess)
				 user = JsonConvert.DeserializeObject<List<User>>(result.Content);
			//var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);

			
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
				switch (option.ToLower())
				{
					case "start process":
					case "start":
					case "restart bot":
					case "start bot":
						processDetails.Action = "start";
						return await stepContext.BeginDialogAsync(nameof(StartProcessDialog), processDetails, cancellationToken);
					case "process status":
					case "status":
					case "metrics":
						processDetails.Action = "check status";
						return await stepContext.BeginDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
					case "stop a process":
					case "stop":
					case "stop process":
					case "stop bot":
						processDetails.Action = "stop";
						return await stepContext.BeginDialogAsync(nameof(StopProcessDialog), processDetails, cancellationToken);
					
					case "report an issue":
					case "issue":
					case "report":
					case "report issue":
					case "bot failed":
					case "bot not working":
						processDetails.Action = string.Empty;
						var value = JsonConvert.SerializeObject(new PromptOption { Id = "FinalStep", Value = "bam?id=rpa_new_request&type=incident" }); 
						var choices = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=incident",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", value: value, null)
							 }
						};
						choices.Add(rpaService.GetMainMenuOption());
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "To Report an Issue, click Button below")
						}, cancellationToken);
					case "**contact rpa support**":
					case "contact rpa support":
					case "rpa support":
					case "support":
					case "contact support":
					case "help":
					case "need help":
					case "help needed":
						return await stepContext.BeginDialogAsync(nameof(SupportDialog), processDetails, cancellationToken);
						
					case "request an enhancement":
					case "enhancement":
					case "request":
					case "new request":
						processDetails.Action = string.Empty;
						var enhancementValue = JsonConvert.SerializeObject(new PromptOption { Id = "FinalStep", Value = "bam?id=rpa_new_request&type=enhancement" });
						var options = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_new_request&type=enhancement",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", value: enhancementValue, null)
							 } };
						options.Add(rpaService.GetMainMenuOption());
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(options, "To Request an Enhancement, click Button below")
						}, cancellationToken);
					case "submit a new idea":
					case "new idea":
					case "idea":
					case "new project":
					case "new process":
					case "project request":
						processDetails.Action = string.Empty;
						var ideaValue = JsonConvert.SerializeObject(new PromptOption { Id = "FinalStep", Value = "bam?id=sc_cat_item&sys_id=a41ac289db7c6f0004b27709af9619a3" });
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(new List<Choice> { new Choice
							{
								Value = "bam?id=sc_cat_item&sys_id=a41ac289db7c6f0004b27709af9619a3",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", value: ideaValue, null)
							 } ,rpaService.GetMainMenuOption()}, "To Submit an Idea, click Button below")
							
						}, cancellationToken);
					case "Bot Portal":
					case "bot portal":
						processDetails.Action = string.Empty;
						var rpaProcessValue = JsonConvert.SerializeObject(new PromptOption { Id = "FinalStep", Value = "bam?id=rpa_processes" });
						options = new List<Choice> { new Choice
							{
								Value = "bam?id=rpa_processes",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openUrl", value: rpaProcessValue, null)
							 } };
						options.Add(rpaService.GetMainMenuOption());
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(options, "To go to Bot Portal, click Button below")
						}, cancellationToken);

					case "end chat":
					case "exit":
					case "exit chat":
					case "close":
					case "close chat":
					case "end":
						//processDetails.Action = "done";
						return await stepContext.ReplaceDialogAsync(nameof(EndConversationDialog), processDetails, cancellationToken);
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
			var rpaService = new RPAService();
			var processDetails = this.processDetails;
			processDetails.Action = string.Empty;
			var option = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "Confirm" && promptOption.Id != "mainMenu" && promptOption.Id != "FinalStep" && promptOption.Id != "rpaSuport")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				option = promptOption.Value;
			}
			//if (option.ToLower() == "yes" || option.ToLower() == "y")
			//{
				//return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
				/*var value = JsonConvert.SerializeObject(new PromptOption { Id = "rpaSuport", Value = "RPASupport@bayview.com" });
				var choices = new List<Choice>
				{ new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", "openEmail", value: value, null)
							 } };
				choices.Add(rpaService.GetMainMenuOption());
				
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "To Contact RPA Support, click Button below")
					
				}, cancellationToken);*/
			//}
			if(option.ToLower() == "main menu" || option.ToLower() == "m")
			{
				
				return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
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
			var option = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "mainMenu" && promptOption.Id != "rpaSuport")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				option = promptOption.Value;
			}
			if (option.ToLower() == "main menu" || option.ToLower() == "m")
			{
				
				return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
			}
			return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
		}
	}
}
