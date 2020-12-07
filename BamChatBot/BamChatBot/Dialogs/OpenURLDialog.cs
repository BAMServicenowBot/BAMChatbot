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
	public class OpenURLDialog : CancelAndHelpDialog
	{
		public OpenURLDialog() : base(nameof(OpenURLDialog))
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
			AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				//NextStepAsync,*/
				FinalStepAsync
			}));
			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}

		
		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var message = string.Empty;
			var processDetails = (ProcessDetails)stepContext.Options;
			
			if (processDetails.Action == "typed")
			{
				message = "Typing is not valid for this menu option." + Environment.NewLine + "Please return to the Main Menu and click on the option you want to select.";
				processDetails.Action = string.Empty;
				var rpaService = new RPAService();
				var choices  = new List<Choice>();
				choices.Add(rpaService.GetMainMenuOption());
				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, message),
				}, cancellationToken);
			}
			
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			
			
			
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var option = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			var processDetails = (ProcessDetails)stepContext.Options;
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "mainMenu")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				option = promptOption.Value;
			}
			if (option.ToLower() == "main menu" || option.ToLower() == "m" || option.ToLower() == "menu")
			{

				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
		}

	}
}
