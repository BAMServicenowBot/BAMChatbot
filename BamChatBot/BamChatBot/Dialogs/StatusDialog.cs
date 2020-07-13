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
using Microsoft.Bot.Schema;
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
				//var _user = await _userAccessor.GetAsync(stepContext.Context, () => new User(), cancellationToken);
				var response = rpaService.GetUser(stepContext.Context.Activity.Conversation.Id);
				var user = new List<User>();
				if (response.IsSuccess)
					user = JsonConvert.DeserializeObject<List<User>>(response.Content);
				var result = rpaService.GetListOfProcess(processes, Convert.ToInt32(user[0].u_last_index));
				var choices = result.Choices;
				//var rpaSupportChoice = rpaService.GetRPASupportOption();
				var menuValue = JsonConvert.SerializeObject(new PromptOption { Id = "menu", Value = "menu"});
				var mainMenu = new Choice
				{
					Value = "menu",//RPASupport@bayview.com
					Action = new CardAction(ActionTypes.PostBack, "**Main Menu**", null, "**Main Menu**", "**Main Menu**", value: menuValue, null)

				};
				//choices.Add(rpaSupportChoice);
				choices.Add(mainMenu);
				//save index
				user[0].u_last_index = result.LastIndex.ToString();
				rpaService.UpdateUser(user[0], stepContext.Context.Activity.Conversation.Id);
				//_user.u_last_index = result.LastIndex;
				//await this._userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.HeroCard(choices, text + Environment.NewLine + "Click the process you would like to get the status for.")
					/*Prompt = MessageFactory.Text(text + Environment.NewLine + "Click the process you would like to get the status for."),
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

		private async Task<DialogTurnResult> ShowRunningProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var rpaService = new RPAService();
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "availableProcesses" && promptOption.Id != "menu" )
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				result = promptOption.Value;
			}
			var _response = rpaService.GetUser(stepContext.Context.Activity.Conversation.Id);
			var user = new List<User>();
			if (_response.IsSuccess)
				user = JsonConvert.DeserializeObject<List<User>>(_response.Content);
			if (result.ToLower() == "menu" || result.ToLower() == "m"/*"RPASupport@bayview.com"*/)
			{
				//save index
				user[0].u_last_index = "0";
				rpaService.UpdateUser(user[0], stepContext.Context.Activity.Conversation.Id);
				//_user.u_last_index = 0;
				//await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}
			else if (result.ToLower() == "load_more")
			{
				processDetails.LoadMore = true;
				return await stepContext.ReplaceDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
			}
			else
			{
				processDetails.ProcessSelected = rpaService.GetSelectedProcess(processDetails.Processes, result);
				//check if a process was selected, or something was written
				if (!string.IsNullOrEmpty(processDetails.ProcessSelected.Sys_id))
				{
					//save index
					user[0].u_last_index = "0";
					rpaService.UpdateUser(user[0], stepContext.Context.Activity.Conversation.Id);
					//_user.u_last_index = 0;
					//await _userAccessor.SetAsync(stepContext.Context, _user, cancellationToken);
					var apiRequest = new APIRequest
					{
						ProcessId = processDetails.ProcessSelected.Sys_id
					};

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
							releaseIds.Add(release.sys_id);
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
							var include = "Total Transactions Processed: " + item.TotalTransactions + Environment.NewLine;/* +
								"Run Time: " + item.Runtime + Environment.NewLine;*/
							if(item.ProcessType== "procedural")
							{
								include = string.Empty;
							}
							var endTime = item.End!=null ? "End Time: " + item.End + Environment.NewLine : string.Empty;
							var startTime = item.Start != null ? "Start Time: " + item.Start + Environment.NewLine : string.Empty;

							text += 
								"Status: " + item.State.label + Environment.NewLine +
								startTime+
								endTime +
								include +
								"Total Transactions Successful: " +Convert.ToInt32(item.TotalTransSuccessful) + Environment.NewLine +
								"Total Exceptions: " + Convert.ToInt32(item.TotalExceptions);
						}
						
					}
					else
					{
						text = response.Content;
					}
					var choices = rpaService.GetConfirmChoices();
					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
					{
						Prompt = (Activity)ChoiceFactory.SuggestedAction(choices, text + Environment.NewLine + "Do you want to check the status of another process?")
						/*Prompt = MessageFactory.Text(text + Environment.NewLine + "Do you want to check the status of another process?"),
						Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })*/
					}, cancellationToken);
				}
				else
				{
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
				}

			}

		}

		private async Task<DialogTurnResult> AnotherProcessStatusStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{

			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();
			var promptOption = new PromptOption();
			try
			{
				promptOption = JsonConvert.DeserializeObject<PromptOption>(stepContext.Result.ToString());
			}
			catch (Exception) { }

			if (!string.IsNullOrEmpty(promptOption.Id))
			{
				if (promptOption.Id != "Confirm")
				{
					processDetails.Action = "pastMenu";
					return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				}
				result = promptOption.Value;
			}
			if (result.ToLower() == "yes" || result.ToLower() == "y")
			{
				//restart this Dialog
				return await stepContext.ReplaceDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
			}
			else if (result.ToLower() == "no" || result.ToLower() == "n")//go back to main Dialog
			{
				processDetails.Action = string.Empty;
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
			}
			else//go back to main Dialog with null option
			{
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
			}

		}
	}
}
