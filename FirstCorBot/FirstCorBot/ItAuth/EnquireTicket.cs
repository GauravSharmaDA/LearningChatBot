using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirstCorBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ItAuth.Service.ItAuth
{
    public class EnquireTicketDialog : CancelAndHelpDialog
    {
        public EnquireTicketDialog(string id) : base(id)
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            var steps = new WaterfallStep[]
            {
                InitialStep,
                FinalStep
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var options = new List<string>
            {
                "SPS00001",
                "SPS00002",
                "SPS00003"
            };
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Which ticket you would you like to check status for?"),
                RetryPrompt = MessageFactory.Text("Which ticket you would you like to check status for?"),
                Choices = ChoiceFactory.ToChoices(options),
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            if (stepContext?.Result != null)
            {
                var customerIntentFlow = (ItAuthFlow)stepContext.Options;
                customerIntentFlow.SelectedTicketNumber = (string)((FoundChoice)stepContext.Result).Value;

                return await stepContext.EndDialogAsync(customerIntentFlow, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
