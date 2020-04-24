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
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
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
			var processDetails = (ProcessDetails)stepContext.Options;
			var choices = new List<Choice> { new Choice
							{
								Value = "yes",
								Action = new CardAction(ActionTypes.PostBack, "Yes", null, "Yes", "exitChat", "yes", null)
							 },

				new Choice
				{
					Value = "no",
					Action = new CardAction(ActionTypes.PostBack, "No", null, "No", "No", "no", null)
				} };
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "Would you like to close this Chat window?")
			}, cancellationToken);
		}

		private async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			processDetails.Action = string.Empty;
			var result = stepContext.Result.ToString();
			if( result.ToLower() == "yes")
			{
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Welcome back!" + Environment.NewLine + "Type " + '"' + "Menu" + '"' + " for available options.") }, cancellationToken);
			}
			/*if(result== "exitChat")
			{
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Welcome back!"+Environment.NewLine+"Type " + '"' + "Menu" + '"' + " for available options.") }, cancellationToken);
			//}*/
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
		}
		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
		}
	}
}
