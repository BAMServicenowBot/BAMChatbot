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
	public class ParametersProcessDialog : CancelAndHelpDialog
	{
		public ParametersProcessDialog() : base(nameof(ParametersProcessDialog))
		{
			AddDialog(new StartProcessSharedDialog());
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				ShowParametersStepAsync,
				ConfirmStartProcessStepAsync
			}));

			// The initial child Dialog to run.
			InitialDialogId = nameof(WaterfallDialog);
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var text = string.Empty;
			var rpaService = new RPAService();
			var response = rpaService.GetConversationFlow(stepContext.Context.Activity.Conversation.Id);
			var result = JsonConvert.DeserializeObject<List<ConversationFlow>>(response.Content);
			if (result.Count > 0)
			{
				processDetails.CurrentQuestion = result[0];
				if (processDetails.CurrentQuestion.u_last_question_index == 0)
				{
					text = "Process " + processDetails.ProcessSelected.Name + " needs input parameters, please start entering them below.";
				}

				//delete record from SN
				rpaService.DeactivatedConversationFlow(processDetails.CurrentQuestion.sys_id, stepContext.Context.Activity.Conversation.Id);

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(text + Environment.NewLine + "Enter: " + processDetails.CurrentQuestion.u_param_name) }, cancellationToken);
			}
			else
			{
				processDetails.CurrentQuestion = new ConversationFlow();
				//get parameters entered

				return await stepContext.NextAsync(processDetails, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> ShowParametersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var processDetails = (ProcessDetails)stepContext.Options;
			var value = string.Empty;
			try
			{
				processDetails = (ProcessDetails)stepContext.Result;
			}
			catch (Exception)
			{
				value = stepContext.Result.ToString();
			}
			
			var rpaService = new RPAService();
			if (string.IsNullOrEmpty(processDetails.CurrentQuestion.sys_id))
			{
				var inputs = new List<ConversationFlowInput>();
				//clean release params
				/*foreach (var r in processDetails.ProcessSelected.Releases)
				{
					for (var p=0; p<r.Parameters.Count; p++)
					{
						r.Parameters[p] = new ProcessParameters();
					}
				}*/
					//all parameters are entered
					foreach (var r in processDetails.ProcessSelected.Releases)
				{
					if (r.parameters_required)
					{
						//get params
						var response = rpaService.GetConversationFlowInputs(stepContext.Context.Activity.Conversation.Id, r.sys_id);
						var result = JsonConvert.DeserializeObject<List<ConversationFlowInput>>(response.Content);
						inputs.AddRange(result);
						var processParametersList = new List<ProcessParameters>();
						
						foreach(var p in result)
						{
							var _param = r.parameters.Find(pp => pp.parmName == p.paramName);
							_param.value = p.u_value;
							/*var processParameters = new ProcessParameters
							{
								ParmName = p.paramName,
								ParamValue = p.u_value,
								ParmType = p.paramType
							};
							r.Parameters.Add(processParameters);
							processParametersList.Add(processParameters);*/
						}

						//set process params
						/*if (processDetails.ProcessSelected.ProcessParameters.ContainsKey(r.Sys_id))
						{
							processDetails.ProcessSelected.ProcessParameters[r.Sys_id].AddRange(processParametersList);
						}
						else
						{
							processDetails.ProcessSelected.ProcessParameters.Add(r.Sys_id, processParametersList);
						}*/
					}
				}
				var message = string.Empty;
				foreach (var i in inputs)
				{
					message += i.paramName + ": " + i.u_value + Environment.NewLine;
				}

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), "You have entered " +Environment.NewLine+ message + "Is that correct?")

				}, cancellationToken);


			}
			else
			{
				//save input

				var conversationFlowInput = new ConversationFlowInput
				{
					u_chatbot_conversation_flow = processDetails.CurrentQuestion.sys_id,
					u_value = value.ToString(),
					u_conversation_id = stepContext.Context.Activity.Conversation.Id
				};
				rpaService.SaveConversationFlowInput(conversationFlowInput);
				return await stepContext.ReplaceDialogAsync(nameof(ParametersProcessDialog), processDetails, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> ConfirmStartProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var rpaService = new RPAService();
			var processDetails = (ProcessDetails)stepContext.Options;
			var result = stepContext.Result.ToString();
			if (result == "Yes")
			{
				//clear data in SN table
				rpaService.DeleteConversationFlowInputs(stepContext.Context.Activity.Conversation.Id);
				return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);
			}
			else
			{
				return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);
			}
		}
	}
}
