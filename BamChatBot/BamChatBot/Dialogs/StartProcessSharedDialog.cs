using BamChatBot.Models;
using BamChatBot.Services;
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
	public class StartProcessSharedDialog : CancelAndHelpDialog
	{
		public StartProcessSharedDialog() : base(nameof(StartProcessSharedDialog))
		{
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
		{
				IntroStepAsync,
				StartAnotherProcessStepAsync
		}));

			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			/*conversationFlow.AskingForParameters = false;
			await this._conversationFlow.SetAsync(stepContext.Context, conversationFlow);*/
			if (processDetails.ProcessSelected.LastRun.State == "Faulted" || processDetails.ProcessSelected.LastRun.State == "Successful" || processDetails.ProcessSelected.LastRun.State == "Stopped")
			{
				var rpaService = new RPAService();
				var response = new APIResponse();
				if (processDetails.ProcessSelected.Releases.Any(r => r.parameters_required == true))
				{
					response = rpaService.StartProcessWithParams(processDetails.ProcessSelected);
				}
				else
				{
					response = rpaService.StartProcess(processDetails.ProcessSelected);

				}

				var error = false;
				if (string.IsNullOrEmpty(response.Content) || !response.IsSuccess)
				{
					error = true;
				}
				if (error)
				{
					return await stepContext.ReplaceDialogAsync(nameof(StartProcessErrorDialog), processDetails, cancellationToken);
				}
				else
				{
					processDetails.Jobs = JsonConvert.DeserializeObject<List<Job>>(response.Content);

					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
					{
						Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), processDetails.ProcessSelected.Name + " process  has started, you will be notified when it finishes. Do you want to run another process?")
					}, cancellationToken);
				}
			}
			else
			{
				processDetails.Action = "error";
				processDetails.Error = "Cannot start " + processDetails.ProcessSelected.Name + " because is running already.";
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> StartAnotherProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;

			var action = stepContext.Result.ToString();
			if (action == "Yes")
			{
				//start StartProcessDialog Dialog
				return await stepContext.ReplaceDialogAsync(nameof(StartProcessDialog), processDetails, cancellationToken);
			}
			else if (action == "No")//go back to main Dialog
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
			else//go back to main Dialog with null
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}
		}
	}
}
