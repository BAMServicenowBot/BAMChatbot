using BamChatBot.Models;
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
	public class SupportDialog : CancelAndHelpDialog
	{
		public SupportDialog() : base(nameof(SupportDialog))
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
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
			var message = "You would like to contact RPA Support, is that correct?";
			var processDetails = (ProcessDetails)stepContext.Options;
			if (processDetails.Action == "typed")
			{
				message = "Typing is not a valid option for this prompt" + Environment.NewLine + "Please CLICK one of the options below!";
				processDetails.Action = string.Empty;
			}
			//var confirmChoices = rpaService.GetConfirmChoices();
			var noOption = JsonConvert.SerializeObject(new PromptOption { Id = "Confirm", Value = "No" });
			var valueRPA = JsonConvert.SerializeObject(new PromptOption { Id = "rpaSuport", Value = "RPASupport@bayview.com" });
			var confirmChoices = new List<Choice>
						{ new Choice
							{
								Value = "RPASupport@bayview.com",
								Action = new CardAction(ActionTypes.PostBack, "Yes", null, "Yes", "openEmail", value: valueRPA, null) },
								new Choice
							{
								Value = "No",
								Action = new CardAction(ActionTypes.PostBack, "No", null, "No", "No", value: noOption, null)
							}
						};
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = (Activity)ChoiceFactory.SuggestedAction(confirmChoices, message),
				RetryPrompt = MessageFactory.Text("Typing is not a valid option for this prompt" + Environment.NewLine + "Please CLICK one of the options below!")
			}, cancellationToken);
		}

		private async Task<DialogTurnResult> LastStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			processDetails.Action = "typed";
			var result = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "Confirm" && promptOption.Id != "rpaSuport")
				{
					processDetails.Action = "pastMenu";
					//return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				result = promptOption.Value;
			}
			if(result == "RPASupport@bayview.com" || result.ToLower() == "no" || result.ToLower() == "n")
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
			return await stepContext.ReplaceDialogAsync(nameof(SupportDialog), processDetails, cancellationToken);

		}

		
	}
}
