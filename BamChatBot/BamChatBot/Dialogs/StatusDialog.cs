using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BamChatBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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
            var config = new Config();
            var result = config.ProcessStatus(apiRequest);
            var processSatus =  JsonConvert.DeserializeObject<List<ProcessStatus>>(result.Content);

            var text = processSatus.Count > 1 ? "Here are the status for " : "Here is the latest status for ";
            text += processDetails.ProcessSelected.Name + " process." + Environment.NewLine;
            foreach (var item in processSatus)
            {
                text += "Start Time: " + item.Start + Environment.NewLine + 
                    "End Time: " + item.End + Environment.NewLine + 
                    "Status: " + item.State + Environment.NewLine;
               
            }
             await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(text)
            }, cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), processDetails, cancellationToken);
        }

    }

       
}
