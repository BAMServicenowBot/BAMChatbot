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
		protected readonly IStatePropertyAccessor<User> _userAccessor;
		public StatusDialog(IStatePropertyAccessor<User> userAccessor) : base(nameof(StatusDialog))
        {
			_userAccessor = userAccessor;
			AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
				GetRunningProcessStepAsync,
				ShowRunningProcessStepAsync,
				AnotherProcessStatusStepAsync

			}));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
			var processDetails = (ProcessDetails)stepContext.Options;
			new User().GetUserRunningProcess(processDetails);
			return await stepContext.NextAsync(processDetails, cancellationToken);
			
        }

		private async Task<DialogTurnResult> GetRunningProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var processes = processDetails.Processes;
			var text = "Here are your running processes.";
			if (processDetails.LoadMore)
			{
				text = string.Empty;
				processDetails.LoadMore = false;
			}
			if (processes.Count > 0)
			{
				var rpaService = new RPAService();
				var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);
				var result = rpaService.GetListOfProcess(processes, _user.LastIndex);
				var choices = result.Choices;
				var rpaSupportChoice = rpaService.GetRPASupportOption();
				choices.Add(rpaSupportChoice);
				//save index
				_user.LastIndex = result.LastIndex;
				await this._userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);

				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text(text + Environment.NewLine + "Click the process you would like to get the status for."),
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

		private async Task<DialogTurnResult> ShowRunningProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{

			var processDetails = (ProcessDetails)stepContext.Options;
			var result = (FoundChoice)stepContext.Result;
			var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);
			if (result.Value == "rpaSupport")
			{
				_user.LastIndex = 0;
				await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
			else if (result.Value == "Load_More")
			{
				processDetails.LoadMore = true;
				return await stepContext.ReplaceDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
			}
			else
			{
				_user.LastIndex = 0;
				await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
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
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text(text + Environment.NewLine + "Do you want to check the status of another process?"),
					Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
				}, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> AnotherProcessStatusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) {

			var processDetails = (ProcessDetails)stepContext.Options;
			var result = (FoundChoice)stepContext.Result;
			if (result.Value == "Yes")
			{
				//restart this Dialog
				return await stepContext.ReplaceDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
			}
			else//go back to main Dialog
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
			
		}
    }
}
