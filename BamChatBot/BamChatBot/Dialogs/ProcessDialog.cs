using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BamChatBot.Dialogs
{
    public class ProcessDialog: CancelAndHelpDialog
    {
        public ProcessDialog(): base(nameof(ProcessDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new StatusDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                SelectedProcessStepAsync,
                ProcessSelectedProcessStepAsync,
                ExecuteActionStepAsync,
                ContactRPASpport
               // FinalStepAsync,*/
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

       

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var processDetails = (ProcessDetails)stepContext.Options;
            var snConfig = new Config();
            var result = snConfig.GetApiResult("userProcesses");
            if (!result.IsSuccess)
            {
                processDetails.Error = JsonConvert.DeserializeObject<ProcessModel>(result.Content).Error;
            }
            else
            {
                var processes = JsonConvert.DeserializeObject<List<ProcessModel>>(result.Content);
                processDetails.Processes = processes;
            }
            
            return await stepContext.NextAsync(processDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> SelectedProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var processDetails = (ProcessDetails)stepContext.Options;
            var processes = processDetails.Processes;
            var text = "I am trying to locate the list of your automated processes.";
            if (processes.Count > 0)
            {
                //initialize list of Choices
                var options = new PromptOptions()
                {
                    Choices = new List<Choice>()
                };
                if (processes.Count > 10)
                {
                    //split 
                }
                foreach (var process in processes)
                {
                    options.Choices.Add(new Choice
                    {
                        Value = process.Sys_id,
                        Action = new CardAction(ActionTypes.MessageBack, process.Name, null, process.Name, process.Name, null, null)

                    });
                }
              
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(text+Environment.NewLine+"Here they are."),
                    Choices = options.Choices,
                    Style = ListStyle.Auto
                }, cancellationToken);

            }
            else
            {
                processDetails.Action = "error";
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> ProcessSelectedProcessStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           var processDetails = (ProcessDetails)stepContext.Options;
           var result =  (FoundChoice)stepContext.Result;
           
            foreach (var item in processDetails.Processes)
            {
                if (result.Value == item.Sys_id)
                {
                    processDetails.ProcessSelected = item;
                }
            }

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("You chose " +processDetails.Action+" "+result.Synonym+", is that correct?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" })
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ExecuteActionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var processDetails = (ProcessDetails)stepContext.Options;
            var selectedProcess = processDetails.ProcessSelected;
            selectedProcess.StartedBy = "chat_bot";
            var action = (FoundChoice)stepContext.Result;
            processDetails.ConfirmAction = action.Value;
            var config = new Config();

            if (action.Value == "Yes") { 
            if (processDetails.Action == "start")
            {
                    //save activity id for when process finish
                  var activityId =  stepContext.Context.Activity.Id;

                var response = config.StartProcess(selectedProcess, activityId);
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
            else
            {
                return await stepContext.BeginDialogAsync(nameof(StatusDialog), processDetails, cancellationToken);
            }
            }
            else
            {
                //start over
                return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ContactRPASpport(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //this method only is call if there was an error
            var processDetails = (ProcessDetails)stepContext.Options;
            //set action to something else, to set the right message on MainDialog
            processDetails.Action = "error";
          return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
        }
        }
}
