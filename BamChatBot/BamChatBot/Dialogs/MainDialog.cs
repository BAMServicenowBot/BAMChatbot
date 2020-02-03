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
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using BamChatBot.CognitiveModels;
using BamChatBot.Models;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Office.Interop;

namespace BamChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly  ProcessRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
		protected readonly IStatePropertyAccessor<User> _userAccessor;

		// Dependency injection uses this constructor to instantiate MainDialog
		public MainDialog(ProcessRecognizer luisRecognizer, ILogger<MainDialog> logger, UserState userState)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
			_userAccessor =  userState.CreateProperty<User>(nameof(User));

			AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new StartProcessDialog());
			AddDialog(new StatusDialog());
			AddDialog(new StopProcessDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
				ContinueStepAsync
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
				if (processDetails.Action == "default")
				{
					message = "I am sorry, I cannot help you with that right now but I'm working on it. Please try asking in a different way.";
					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions{ Prompt = MessageFactory.Text(message), }, cancellationToken);
				}
				else if(processDetails.Action == "startOver")
				{
					message = "What can I help you with today?";
					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(message), }, cancellationToken);
				}
				else
				{
					if (processDetails.Action == "start")
					{
						message = "Process " + processDetails.ProcessSelected.Name + " was started, you will be notified when it finishes. " + txt;
					}

					else if (!string.IsNullOrEmpty(processDetails.Error))
					{
						message = processDetails.Error + " " + txt;
					}
					else
					{
						message = txt;
					}
					return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
					{
						Prompt = MessageFactory.Text(message),
						Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
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
				if (stepContext.Result.GetType() == typeof(FoundChoice))
				{
					var action = (FoundChoice)stepContext.Result;
					if(action.Value== "Yes")
					{
						//set rpa for LUIS to recognize it as RAP intent, and show the list of actions again
						stepContext.Context.Activity.Text = "rpa";
					}
				}
			}
			return await RecognizeText(stepContext, cancellationToken);
		}

		private  async Task<DialogTurnResult> RecognizeText(WaterfallStepContext stepContext,  CancellationToken cancellationToken)
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

						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("Here is all you can do related to RPA."),
							Choices = choices,
							Style = ListStyle.HeroCard
						}, cancellationToken);


					/*case Process.Intent.StartProcess:
                         processDetails.Action = "start";
                         //await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);
                         return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
                     
                     case Process.Intent.CheckState:
                         processDetails.Action = "check status";
                         return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
					case Process.Intent.Stop:
						processDetails.Action = "stop";
						return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
					case Process.Intent.Done:
                        return await stepContext.EndDialogAsync(stepContext, cancellationToken);
					case Process.Intent.Enhancement:
						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("To request an enhancement. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "enhancement ",
								Action = new CardAction(ActionTypes.OpenUrl, "Click Here", value: "https://bayviewdev.service-now.com/bam?id=rpa_new_request&type=enhancement")
							 } }
						}, cancellationToken);
					case Process.Intent.Issue:
						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("To report an issue. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "enhancement ",
								Action = new CardAction(ActionTypes.OpenUrl, "Click Here", value: "https://bayviewdev.service-now.com/bam?id=rpa_new_request&type=incident")
							 } }
						}, cancellationToken);
					case Process.Intent.RPASupport:
						processDetails.Action = "rpaSupport";
						Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
						Microsoft.Office.Interop.Outlook._MailItem oMailItem = (Microsoft.Office.Interop.Outlook._MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
						oMailItem.To = "RPASupport@bayview.com";
						// body, bcc etc...
						oMailItem.Display(true);
						return await stepContext.NextAsync(processDetails, cancellationToken);*/
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
				processDetails.Action = "error";
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text("To continue to run this bot, please contact RPA Support. "),
					Choices = new List<Choice> { new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.OpenUrl, "Click Here", value: "https://bayviewdev.service-now.com/bam?id=rpa_new_request&type=incident")
							 } }
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
			var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);
			var processDetails = new ProcessDetails
			{ User = _user };

			//var option = (FoundChoice)stepContext.Result;
			var resultType = stepContext.Result.GetType();
			if (resultType == typeof(FoundChoice))
			{
			var option = (FoundChoice)stepContext.Result;
			switch(option.Value)
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
					return	await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("To Report an Issue. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "issue",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", null, "issue", null)
							 } }
						}, cancellationToken);
					case "Contact RPA Support":
						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("To Contact RPA Support. "),
							Choices = new List<Choice> { new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.PostBack, "Click Here", null, "Click Here", null, "rpaSupport", null)
							 } }
						}, cancellationToken);
					default:
						return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);

				}
			}
			else
			{
			  processDetails = (ProcessDetails)stepContext.Result;
				// Restart the main dialog with a different message the second time around
				return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
			}

			// If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
			// the Result here will be null.
			/*if (stepContext.Result is BookingDetails result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }*/

            // Restart the main dialog with a different message the second time around
           // return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
        }

		private async Task<DialogTurnResult> ContinueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var result = (FoundChoice)stepContext.Result;
			switch (result.Value)
			{
				case "issue":
					System.Diagnostics.Process.Start("https://bayviewdev.service-now.com/bam?id=rpa_new_request&type=incident");
					return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
				case "rpaSupport":
					
					Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
					Microsoft.Office.Interop.Outlook._MailItem oMailItem = (Microsoft.Office.Interop.Outlook._MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
					oMailItem.To = "RPASupport@bayview.com";
					// body, bcc etc...
					oMailItem.Display(true);
					return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
			}
			
			return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
		}
		}
}
