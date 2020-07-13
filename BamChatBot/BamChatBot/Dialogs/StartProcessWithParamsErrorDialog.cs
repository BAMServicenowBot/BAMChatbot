using BamChatBot.Models;
using BamChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BamChatBot.Dialogs
{
	public class StartProcessWithParamsErrorDialog : CancelAndHelpDialog
	{
		public StartProcessWithParamsErrorDialog() : base(nameof(StartProcessWithParamsErrorDialog))
		{
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				FinalStepAsync
			}));
			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}


		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var value = JsonConvert.SerializeObject(new PromptOption { Id = "rpaSuport", Value = "RPASupport@bayview.com" });
			var choices = new List<Choice>
	 {
  new Choice
		   {
	Value = "rpaSupport",//RPASupport@bayview.com
Action = new CardAction(ActionTypes.PostBack, "To Contact RPA Support click here", null, "To Contact RPA Support click here", "openEmail", value: value, null)
		 }
};
			/*var rpaService = new RPAService();
			choices.Add(rpaService.GetRPASupportOption());*/
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, "You have made 3 attempts, please contact RPA Support or type menu to go back to main menu.")
			},
				cancellationToken);
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();
			processDetails.Action = string.Empty;
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "rpaSuport")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				result = promptOption.Value;
			}
			if (result.ToLower() == "menu" || result.ToLower() == "m")
			{
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}
			else
			{
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}

		}

	}
}
