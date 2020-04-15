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
				LastStepAsync
			}));
			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var choices = new List<Choice> { new Choice
							{
								Value = "exitChat",
								Action = new CardAction(ActionTypes.PostBack, "Exit Chat", null, "Exit Chat", "exitChat", "exitChat", null)
							 },

				new Choice
				{
					Value = "menu",
					Action = new CardAction(ActionTypes.PostBack, "Menu", null, "Menu", "menu", "menu", null)
				} };
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "Thank you")
			}, cancellationToken);
		}

		private async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			processDetails.Action = string.Empty;
			var result = stepContext.Result.ToString();

			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);

		}
	}
}
