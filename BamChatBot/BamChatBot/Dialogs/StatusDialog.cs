using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
using BamChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;

namespace BamChatBot.Dialogs
{
    public class StatusDialog : CancelAndHelpDialog
    {
        public StatusDialog() : base(nameof(StatusDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
				ShowProcessStepAsync,
				GetProcessStatusStepAsync
               /* SelectedProcessStepAsync,
                ProcessSelectedProcessStepAsync,
                ExecuteActionStepAsync,
                FinalStepAsync,*/
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
			var processDetails = (ProcessDetails)stepContext.Options;
			new User().GetUserProcess(processDetails);
			return await stepContext.NextAsync(processDetails, cancellationToken);
			
        }

		private async Task<DialogTurnResult> ShowProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var processes = processDetails.Processes;
			var text = "I am trying to locate the list of your automated processes.";
			if (processes.Count > 0)
			{

				var choices = new RPAService().GetListOfProcess(processes);
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text(text + Environment.NewLine + "Here they are."),
					Choices = choices,
					Style = ListStyle.Auto
				}, cancellationToken);

			}
			else
			{
				processDetails.Action = "error";
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}

		}
		private async Task<DialogTurnResult> GetProcessStatusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = (FoundChoice)stepContext.Result;
			var rpaService = new RPAService();
			processDetails.ProcessSelected = rpaService.GetSelectedProcess(processDetails.Processes, result.Value);

			
			var apiRequest = new APIRequest();

			var jobIds = new List<string>();
			var releaseIds = new List<string>();
			//if there was a process started
			if (processDetails.Jobs.Count > 0)
			{
				foreach (var job in processDetails.Jobs)
				{
					foreach (var item in job.Result.Body.Value)
					{
						jobIds.Add(item.Id);
					}
				}
				apiRequest.Ids = jobIds;
				apiRequest.IsJob = true;
			}
			else
			{
				foreach (var release in processDetails.ProcessSelected.Releases)
				{
					releaseIds.Add(release.Sys_id);
				}
				apiRequest.Ids = releaseIds;
				apiRequest.IsJob = false;
			}
			
			var response = rpaService.ProcessStatus(apiRequest);
			var text = string.Empty;
			if (response.IsSuccess)
			{
				var processSatus = JsonConvert.DeserializeObject<List<ProcessStatus>>(response.Content);

				text = processSatus.Count > 1 ? "Here are the status for " : "Here is the latest status for ";
				text += processDetails.ProcessSelected.Name + " process." + Environment.NewLine;
				foreach (var item in processSatus)
				{

					text += "Start Time: " + item.Start + Environment.NewLine +
						"End Time: " + item.End + Environment.NewLine +
						"Status: " + item.State + Environment.NewLine;

				}
			}
			else
			{
				text = response.Content;
			}
			await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = MessageFactory.Text(text)
			}, cancellationToken);
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
		}
    }
}
