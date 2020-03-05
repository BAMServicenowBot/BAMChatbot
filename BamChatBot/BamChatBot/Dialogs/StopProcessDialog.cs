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
using Microsoft.Bot.Schema;

namespace BamChatBot.Dialogs
{
	public class StopProcessDialog : CancelAndHelpDialog
	{
		protected readonly IStatePropertyAccessor<User> _userAccessor;
		public StopProcessDialog(IStatePropertyAccessor<User> userAccessor) : base(nameof(StopProcessDialog))
		{
			_userAccessor = userAccessor;
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ShowProcessStepAsync,
				ConfirmStopProcessStepAsync,
				StopProcessStepAsync

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

		private async Task<DialogTurnResult> ShowProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var processes = processDetails.Processes;
			var text = "Here are your bots in progress. ";
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
				//add one choice for rpa support
				var rpaSupportChoice = rpaService.GetRPASupportOption();
				choices.Add(rpaSupportChoice);
				//save index
				_user.LastIndex = result.LastIndex;
				await this._userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.HeroCard(choices, text + "Which one would you like to stop?")
					/*Prompt = MessageFactory.Text(text+ "Which one would you like to stop?"),
					Choices = choices,
					Style = ListStyle.Auto*/
				}, cancellationToken);

			}
			else
			{
				processDetails.Action = "error";
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> ConfirmStopProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();
			var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);
			switch (result)
			{
				case "Load_More":
					processDetails.LoadMore = true;
					return await stepContext.ReplaceDialogAsync(nameof(StopProcessDialog), processDetails, cancellationToken);

				case "RPASupport@bayview.com":
					_user.LastIndex = 0;
					await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
					processDetails.Action = string.Empty;
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);

				default:
					var rpaService = new RPAService();
					processDetails.ProcessSelected = rpaService.GetSelectedProcess(processDetails.Processes, result);
					if (!string.IsNullOrEmpty(processDetails.ProcessSelected.Sys_id))
					{
						_user.LastIndex = 0;
						await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
						return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
						{
							Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), "You have selected " + processDetails.ProcessSelected.Name + ". Stop this process?")
							/*Prompt = MessageFactory.Text("You have selected " + processDetails.ProcessSelected.Name + ". Stop this process?"),
							Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })*/
						}, cancellationToken);
					}
					else
					{
						return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
					}
			}
		}

		private async Task<DialogTurnResult> StopProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var msg = string.Empty;
			var result = stepContext.Result.ToString();
			var processDetails = (ProcessDetails)stepContext.Options;
			var rpaService = new RPAService();
			if (result == "Yes")
			{
				var response = rpaService.StopProcess(processDetails.ProcessSelected.Sys_id);
				if (response.IsSuccess)
				{
					if (!string.IsNullOrEmpty(response.Content))
					{
						msg = response.Content;
					}
					else
					{
						msg = "Process " + processDetails.ProcessSelected.Name + " has been successfully stopped.";
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
			else if (result == "No")
			{
				return await stepContext.ReplaceDialogAsync(nameof(StopProcessDialog), processDetails, cancellationToken);
			}
			else
			{
				return await stepContext.ReplaceDialogAsync(nameof(StopProcessDialog), null, cancellationToken);
			}

		}
	}
}
