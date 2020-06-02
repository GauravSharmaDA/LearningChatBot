using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirstCorBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace ItAuth.Service.ItAuth
{
    public class CreateTicketDialog : CancelAndHelpDialog
    {
        public static string AskTheQuestion = "Please choose the type of required access:";
        public CreateTicketDialog(string id) : base(id)
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            var steps = new WaterfallStep[]
            {
                InitialStep,
                CaptureServerName,
                FinalStep
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new List<string>
                {
                    "DBAccess",
                    "RemoteAccess"
                };
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(AskTheQuestion),
                RetryPrompt = MessageFactory.Text(AskTheQuestion),
                Choices = ChoiceFactory.ToChoices(options)
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> CaptureServerName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var customerIntentFlow = (ItAuthFlow)stepContext.Options;
            customerIntentFlow.Access = Enum.Parse<TypeOfAccess>(((FoundChoice)stepContext.Result).Value); 

            var promptMessage = MessageFactory.Text("Please enter the name of server.", "Please enter the name of server.", InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }


        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (stepContext?.Result != null)
            {
                var customerIntentFlow = (ItAuthFlow)stepContext.Options;
                customerIntentFlow.ServerName = (string)stepContext.Result;

                return await stepContext.EndDialogAsync(customerIntentFlow, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
