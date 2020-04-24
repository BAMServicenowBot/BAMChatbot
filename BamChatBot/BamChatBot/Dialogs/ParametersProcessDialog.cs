using BamChatBot.Models;
using BamChatBot.Services;
using BamChatBot.Utils;
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
				if (processDetails.CurrentQuestion.u_last_question_index == 0 && processDetails.AttemptCount==0)
				{
					text = "Process " + processDetails.ProcessSelected.Name + " needs input parameters, please start entering them below.";
				}
				else if(processDetails.AttemptCount != 0 && processDetails.CurrentQuestion.u_last_question_index == 0)
				{
					text = "Input parameters entered are wrong, please re-enter them below.";
				}

				//delete record from SN
				rpaService.DeactivatedConversationFlow(processDetails.CurrentQuestion.sys_id, stepContext.Context.Activity.Conversation.Id);
				if (processDetails.CurrentQuestion.u_type.Contains("Bool"))
				{
					
					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "true", "false" }), text + Environment.NewLine + "For " + processDetails.CurrentQuestion.u_param_name.UppercaseFirst()+ " choose one option below.") }, cancellationToken);
				}
				else
				{
					return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(text + Environment.NewLine + "Enter " + processDetails.CurrentQuestion.u_param_name.UppercaseFirst()+":") }, cancellationToken);
				}
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
			//have entered all params
			if (string.IsNullOrEmpty(processDetails.CurrentQuestion.sys_id))
			{
				var inputs = new List<ConversationFlowInput>();
		
					//all parameters are entered
				foreach (var r in processDetails.ProcessSelected.Releases)
				{
					if (r.parameters_required)
					{
						//get params
						var response = rpaService.GetConversationFlowInputs(stepContext.Context.Activity.Conversation.Id, r.sys_id);
						var result = new List<ConversationFlowInput>();
						if(response.IsSuccess)
						result = JsonConvert.DeserializeObject<List<ConversationFlowInput>>(response.Content);
						inputs.AddRange(result);
						var processParametersList = new List<ProcessParameters>();
						
						foreach(var p in result)
						{
							if (p.u_is_object)
							{
								foreach(var o in r.parameters)
								{
									var objectParam = o.obj.Find(obj => obj.parmName == p.paramName);
									if (objectParam != null)
									{
										objectParam.value = p.u_value;
										break;
									}
									else
									{
										foreach(var a in o.obj)
										{
											objectParam = a.array.Find(arr => arr.parmName == p.paramName && string.IsNullOrEmpty(arr.value));
											if (objectParam != null)
											{
												objectParam.value = p.u_value;
												break;
											}
											else
											{
												objectParam = a.array.Find(arr => a.parmName + '[' + arr.parmName +']'== p.paramName && string.IsNullOrEmpty(arr.value));
												if (objectParam != null)
												{
													objectParam.value = p.u_value;
													break;
												}
											}
										}
									}
								}
							}
							else if (p.u_is_array)
							{
								foreach (var a in r.parameters)
								{
									var arrParam = a.array.Find(arr => arr.parmName == p.paramName && string.IsNullOrEmpty(arr.value));
									if (arrParam != null)
									{
										arrParam.value = p.u_value;
										break;
									}
									else
									{
										arrParam = a.array.Find(arr => a.parmName+'['+ arr.parmName+']' == p.paramName && string.IsNullOrEmpty(arr.value));
										if (arrParam != null)
										{
											arrParam.value = p.u_value;
											break;
										}
									}
								}
							}
							else
							{
								var _param = r.parameters.Find(pp => pp.parmName == p.paramName);
								if (_param != null)
								{
									_param.value = p.u_value;
								}
							}
							
						}
					}
				}
				/*var message = string.Empty;
				foreach (var i in inputs)
				{
					message += i.paramName + ": " + i.u_value + Environment.NewLine;
				}

				return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
				{
					Prompt = (Activity)ChoiceFactory.SuggestedAction(ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }), "You have entered " +Environment.NewLine+ message + "Is that correct?")

				}, cancellationToken);*/
				//clear data in SN table
				rpaService.DeleteConversationFlowInputs(stepContext.Context.Activity.Conversation.Id);
				return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);


			}
			else
			{
			
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
			if (result.ToLower() == "yes")
			{
				//clear data in SN table
				rpaService.DeleteConversationFlowInputs(stepContext.Context.Activity.Conversation.Id);
				//return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);
			}
			//else
			//{
				return await stepContext.ReplaceDialogAsync(nameof(StartProcessSharedDialog), processDetails, cancellationToken);
			//}
		}
	}
}
