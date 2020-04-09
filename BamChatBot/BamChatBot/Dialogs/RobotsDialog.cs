using AdaptiveCards;
using BamChatBot.Models;
using BamChatBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BamChatBot.Dialogs
{
	public class RobotsDialog : CancelAndHelpDialog
	{
		public RobotsDialog() : base(nameof(RobotsDialog))
		{
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				SelectedBotsStepAsync
			}));

			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var bot = new Robot();
			foreach (var r in processDetails.ProcessSelected.Releases)
			{
				if (r.robots.Count > 1)
				{
					if (r.robots.Any(b => b.Shown == false))
					{
						bot = r.robots.Find(b => b.Shown == false);
						bot.Shown = true;
						break;
					}
					bot = new Robot();
				}

			}

			/*var choices = new[] { "Submit"};
			var card = new AdaptiveCard("1.2.4");
			/*{
				// Use LINQ to turn the choices into submit actions
				Actions = choices.Select(choice => new AdaptiveSubmitAction
				{
					Title = choice,
					Data = choice,  // This will be a string
				}).ToList<AdaptiveAction>(),
			};*/
			/*card.Body.Add(new AdaptiveTextBlock()
			{
				Text = "Select bot(s) below.",
				Size = AdaptiveTextSize.Default,
				Weight = AdaptiveTextWeight.Bolder
			});

			card.Body.Add(new AdaptiveChoiceSetInput()
			{
				Id = "choiceset1",
				Choices = new List<AdaptiveChoice>()
	{
		new AdaptiveChoice(){
			Title="answer1",
			Value="answer1"
		},
		new AdaptiveChoice(){
			Title="answer2",
			Value="answer2"
		},
		new AdaptiveChoice(){
			Title="answer3",
			Value="answer3"
		}
	},
				Style = AdaptiveChoiceInputStyle.Expanded,
				IsMultiSelect = true
			});
			card.Actions.Add(new AdaptiveSubmitAction()
			{
				Title = "Submit"
			});

			/*var message 

			message.Attachments.Add(new Attachment() { Content = card, ContentType = "application/vnd.microsoft.card.adaptive" });
			await stepContext.Context.PostAsync(message);*/
			if (string.IsNullOrEmpty(bot.id))
			{
				processDetails.ProcessSelected.FirstBot = true;
				//all processed
				foreach (var r in processDetails.ProcessSelected.Releases)
				{
					if (r.robots.Count > 1)
					{
						var robots = new List<string>();
						foreach (var b in r.robots)
						{
							if (b.Selected)
							{
								robots.Add(b.id);
							}
						}
						if (robots.Count == 0)
						{
							foreach (var re in processDetails.ProcessSelected.Releases)
							{
								if (re.robots.Count > 1)
								{
									foreach (var b in re.robots)
									{
										b.Shown = false;
									}
								}
							}
							processDetails.ProcessSelected.ReEnterBot = true;
							return await stepContext.ReplaceDialogAsync(nameof(RobotsDialog), processDetails, cancellationToken);
						}
						r.u_robots = string.Join(',', robots);
					}
				}
				//if needs params 
				if (processDetails.ProcessSelected.Releases.Any(r => r.parameters_required == true))
				{
					var rpaService = new RPAService();
					//set all params for this conversation to false(maybe was interrupted by a notification)
					rpaService.DeactivatedConversationFlow(string.Empty, stepContext.Context.Activity.Conversation.Id);
					rpaService.SaveConversationFlow(processDetails.ProcessSelected, stepContext.Context.Activity.Conversation.Id);
					return await stepContext.ReplaceDialogAsync(nameof(ParametersProcessDialog), processDetails, cancellationToken);
				}
				else
				{
					return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);
				}
			}
			else
			{
				var message = string.Empty;
				if (processDetails.ProcessSelected.FirstBot)
				{
					if (processDetails.ProcessSelected.ReEnterBot)
					{
						message = "You have to select at least one Bot."+Environment.NewLine;
						processDetails.ProcessSelected.ReEnterBot = false;
					}
					message += "Select the Bot(s) to trigger " + processDetails.ProcessSelected.Name + " process. For each one select " + '"' + "Yes or No" + '"';
					processDetails.ProcessSelected.FirstBot = false;
				}
				processDetails.ProcessSelected.Bot = bot;

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), message + Environment.NewLine + bot.name)
				}, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> SelectedBotsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();

			foreach (var r in processDetails.ProcessSelected.Releases)
			{
				if (r.robots.Count > 1)
				{
					foreach (var b in r.robots)
					{
						if (b.id == processDetails.ProcessSelected.Bot.id)
						{
							b.Shown = true;
							if (result == "Yes")
							{
								b.Selected = true;
								processDetails.ProcessSelected.Bot.Selected = true;
							}
						}
					}
				}

			}

			return await stepContext.ReplaceDialogAsync(nameof(RobotsDialog), processDetails, cancellationToken);
		}
	}


}
