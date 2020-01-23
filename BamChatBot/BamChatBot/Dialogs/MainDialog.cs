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

namespace BamChatBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly  ProcessRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ProcessRecognizer luisRecognizer, ProcessDialog processDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(processDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            //to continue the conversation
            if (stepContext.Options != null)
            {
                var txt = "Is there something else can I assist you with? You can type " + "'" + "Done" + "'" + " to finish.";
                var processDetails = (ProcessDetails)stepContext.Options;
                var message = string.Empty;
                if (processDetails.ConfirmAction == "No")
                {
                    message = "What can I help you with today? You can type "+"'"+ "Done"+"'"+" to finish.";
                }
                else if (processDetails.Action == "start")
                {
                    message = "Process " + processDetails.ProcessSelected.Name + " was started, you will be notified when it finishes. Meanwhile, is there something else can I assist you with? You can type " + "'" + "Done" + "'" + " to finish.";
                }
                else if(!string.IsNullOrEmpty(processDetails.Error))
                {
                    message = processDetails.Error +" "+ txt;
                }
                else
                {
                    message = txt;
                }
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
            }
            else//first interaction
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
          
            var user = new User();
            var processDetails = new ProcessDetails
            {
                UserName = user.GetLoginUserName()
                
            };
            // Call LUIS.
           try
             {
                 var luisResult = await _luisRecognizer.RecognizeAsync<Process>(stepContext.Context, cancellationToken);
                 switch (luisResult.TopIntent().intent)
                 {
                     case Process.Intent.StartProcess:
                         processDetails.Action = "start";
                         //await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);
                         return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
                     
                     case Process.Intent.CheckState:
                         processDetails.Action = "check status";
                         return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
					case Process.Intent.Stop:
						//check if the process is running
             case Process.Intent.Done:
             return await stepContext.EndDialogAsync(stepContext, cancellationToken);

                     default:
                         // Catch all for unhandled intents
                         var didntUnderstandMessageText = "I am sorry, I cannot help you with that right now but I'm working on it. Please try asking in a different way";
                         var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                        //start this dialog again
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);
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
           // return await stepContext.BeginDialogAsync(nameof(ProcessDialog), processDetails, cancellationToken);
            //return await stepContext.NextAsync(null, cancellationToken);
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
            var processDetails = (ProcessDetails)stepContext.Options;
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            if (stepContext.Result is BookingDetails result)
            {
                // Now we have all the booking details call the booking service.

                // If the call to the booking service was successful tell the user.

                var timeProperty = new TimexProperty(result.TravelDate);
                var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
                var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
                var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                await stepContext.Context.SendActivityAsync(message, cancellationToken);
            }

            // Restart the main dialog with a different message the second time around
            return await stepContext.ReplaceDialogAsync(InitialDialogId, processDetails, cancellationToken);
        }
    }
}
