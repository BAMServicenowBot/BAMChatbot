using BamChatBot.Models;
using BamChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BamChatBot.Dialogs
{
	public class StartProcessErrorDialog : CancelAndHelpDialog
	{
		public StartProcessErrorDialog() : base(nameof(StartProcessErrorDialog))
		{
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				FinalStepAsync
			}));
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;

			//2nd attempt to start process
			var rpaService = new RPAService();
			var response = rpaService.StartProcess(processDetails.ProcessSelected);
			var error = false;
			if (string.IsNullOrEmpty(response.Content) || !response.IsSuccess)
			{
				error = true;
			}
			if (error)
			{
				var rpaSupportChoice = rpaService.GetRPASupportOption();
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text("There was an issue running " + processDetails.ProcessSelected.Name + " process, please contact RPA Support. "),
					Choices = new List<Choice> { rpaSupportChoice }
				}, cancellationToken);

			}
			else
			{
				processDetails.Jobs = JsonConvert.DeserializeObject<List<Job>>(response.Content);

				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text(processDetails.ProcessSelected.Name + " process  has started, you will be notified when it finishes. Do you want to run another process?"),
					Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
				}, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;

			var action = (FoundChoice)stepContext.Result;
			switch (action.Value)
			{
				case "Yes":
					return await stepContext.ReplaceDialogAsync(nameof(StartProcessDialog), processDetails, cancellationToken);

				default:
					processDetails.Action = string.Empty;
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}

		}

	}
}
