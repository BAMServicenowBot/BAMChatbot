using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BamChatBot.Dialogs
{
	public class EndConversationDialog : CancelAndHelpDialog
	{
		public EndConversationDialog() : base(nameof(EndConversationDialog))
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt), CloseChatPromptValidatorAsync));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
			AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				LastStepAsync,
				FinalStepAsync
			}));
			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var message = "Would you like to close this Chat window?";
			var processDetails = (ProcessDetails)stepContext.Options;
			if (processDetails.Action== "typed")
			{
				message = "Typing is not a valid option for this prompt"+Environment.NewLine+"Please CLICK one of the options below!";
				processDetails.Action = string.Empty;
			}
				var choices = new List<Choice> {
				new Choice
							{
								Value = "button",
								Action = new CardAction(ActionTypes.PostBack, "Yes", null, "Yes", "exitChat", value : "button", null)
							 },
				new Choice
				{
					Value = "no",
					Action = new CardAction(ActionTypes.PostBack, "No", null, "No", "No", value:"no", null)
				} };
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, message),
				//Choices = choices,
				RetryPrompt = MessageFactory.Text("Please, select one of below options!")
			}, cancellationToken);
			
		}

		private async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			processDetails.Action = "typed";
			var result = stepContext.Result.ToString();
			if (result == "button")
			{
				processDetails.Action = string.Empty;
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Welcome back!" + Environment.NewLine + "Type " + '"' + "Menu" + '"' + " for available options.") }, cancellationToken);
			}
			else if(result.ToLower() == "no")
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}
			
			return await stepContext.ReplaceDialogAsync(nameof(EndConversationDialog), processDetails, cancellationToken);
		}
		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
		}

		private static Task<bool> CloseChatPromptValidatorAsync(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken)
		{
			
			// This condition is our validation rule. You can also change the value at this point.
			return Task.FromResult(promptContext.Recognized.Succeeded && (promptContext.Recognized.Value.Value == "button" || promptContext.Recognized.Value.Value == "no"));
		}
	}
}
