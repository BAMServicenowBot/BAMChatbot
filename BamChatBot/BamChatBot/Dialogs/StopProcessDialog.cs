using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
using Newtonsoft.Json;
using BamChatBot.Services;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BamChatBot.Dialogs
{
    public class StopProcessDialog : CancelAndHelpDialog
	{
		public StopProcessDialog() : base(nameof(StopProcessDialog))
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ShowProcessStepAsync,
				StopProcessStepAsync

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

		private async Task<DialogTurnResult> StopProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var msg = string.Empty;
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = (FoundChoice)stepContext.Result;
			var rpaService = new RPAService();
			processDetails.ProcessSelected = rpaService.GetSelectedProcess(processDetails.Processes, result.Value);
			var response = rpaService.StopProcess(processDetails.ProcessSelected.Sys_id);
			if (response.IsSuccess)
			{
				if (!string.IsNullOrEmpty(response.Content))
				{
					msg = response.Content;
				}
				else
				{
					msg = "Process " + processDetails.ProcessSelected.Name + " has been stopped sucessfully";
				}
			}
			else
			{
				msg = response.Error;
			}

			await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
			{
				Prompt = MessageFactory.Text(msg)
			}, cancellationToken);
			return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
		}
		}
}
