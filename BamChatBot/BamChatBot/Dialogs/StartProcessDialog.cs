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
	public class StartProcessDialog : CancelAndHelpDialog
	{
		public StartProcessDialog() : base(nameof(StartProcessDialog))
		{
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ShowProcessStepAsync,
				StartProcessStepAsync
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

		private async Task<DialogTurnResult> StartProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = (FoundChoice)stepContext.Result;
			var rpaService = new RPAService();
			processDetails.ProcessSelected = rpaService.GetSelectedProcess(processDetails.Processes, result.Value);
			processDetails.ProcessSelected.StartedBy = "chat_bot";
			//save activity id for when process finish
			var activityId = stepContext.Context.Activity.Id;

			var response = rpaService.StartProcess(processDetails.ProcessSelected, activityId);
			if (!string.IsNullOrEmpty(response.Content))
			{
				if (response.IsSuccess)
				{
					processDetails.Jobs = JsonConvert.DeserializeObject<List<Job>>(response.Content);

				}
				else
				{
					processDetails.Error = JsonConvert.DeserializeObject<Job>(response.Content).Error;
				}
				return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
				/*return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{ Prompt = MessageFactory.Text("Process " + processDetails.ProcessSelected.Name + " was started, you will be notified when it finishes. Meanwhile, is there something else can I assist you with?") },
				cancellationToken);*/
			}
			else//there was an error
			{
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text("To continue to run this bot, please contact RPA Support. "),
					Choices = new List<Choice> { new Choice
							{
								Value = "rpaSupport",
								Action = new CardAction(ActionTypes.OpenUrl, "Click Here", value: "https://bayviewdev.service-now.com/bam?id=rpa_new_request&type=incident")
							 } }
				}, cancellationToken);
			}
		}

	}
}
