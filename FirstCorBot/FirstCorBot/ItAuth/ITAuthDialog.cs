using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirstCorBot.Dialogs;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.AI.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace ItAuth.Service.ItAuth
{
    public class ItAuthDialog : CancelAndHelpDialog
    {
        private readonly LuisIntentRecognizer _luisRecognizer;
        private readonly QnARecognizer _qnAMakerRecognizer;

        public static string GreetTheCustomer = "Hi, How can I help you today?";
        public static string RepeatMessage = "Sorry, I didn't get that. Can you please try a different word?";
        public ItAuthDialog(LuisIntentRecognizer luisRecognizer, QnARecognizer qnAMakerRecognizer) : base(nameof(ItAuthDialog))
        {
            _luisRecognizer = luisRecognizer;
            _qnAMakerRecognizer = qnAMakerRecognizer;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new CreateTicketDialog(nameof(CreateTicketDialog)));
            AddDialog(new EnquireTicketDialog(nameof(EnquireTicketDialog)));
            var steps = new WaterfallStep[]
            {
                InitialStep,
                SaveInitialIntent,
                FinalStep
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), steps));
            this.InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var customerIntentFlow = (ItAuthFlow)stepContext.Options;

            if (customerIntentFlow.Intent == OperationIntent.NoIntent)
            {
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(GreetTheCustomer),
                    RetryPrompt = MessageFactory.Text(RepeatMessage)
                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }

            return await stepContext.NextAsync(customerIntentFlow.Intent, cancellationToken);
        }

        private async Task<DialogTurnResult> SaveInitialIntent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new QnAMakerOptions { Top = 1 };
            var response = await _qnAMakerRecognizer.GetAnswersAsync(stepContext.Context, options);
            if (response != null && response.Length > 0)
            {
                var suggestedReply = MessageFactory.Text(response[0].Answer);
                suggestedReply.SuggestedActions = new SuggestedActions();
                suggestedReply.SuggestedActions.Actions = new List<CardAction>();
                for (int i = 0; i < response[0].Context.Prompts.Length; i++)
                {
                    var promptText = response[0].Context.Prompts[i].DisplayText;
                    suggestedReply.SuggestedActions.Actions.Add(new CardAction() { Title = promptText, Type = ActionTypes.ImBack, Value = promptText });
                }
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                await stepContext.Context.SendActivityAsync(suggestedReply, cancellationToken);
                return await InitialStep(stepContext, cancellationToken);
            }
            else
            {
                var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
                if (luisResult.Intents.ContainsKey("LogTicketIntent"))
                    return await stepContext.BeginDialogAsync(nameof(CreateTicketDialog), stepContext.Options, cancellationToken);
                if (luisResult.Intents.ContainsKey("EnquireTicket"))
                    return await stepContext.BeginDialogAsync(nameof(EnquireTicketDialog), stepContext.Options, cancellationToken);
            }

            
            GreetTheCustomer = RepeatMessage;
            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
            return await InitialStep(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext?.Result != null)
            {
                var customerIntentFlow = (ItAuthFlow)stepContext.Result;
                return await stepContext.EndDialogAsync(customerIntentFlow, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    public class RecoginizerResult : IRecognizerConvert
    {
        [JsonProperty("intents")]
        public IEnumerable<RecognitionIntent> Intents { get; set; }

        [JsonProperty("entities")]
        public IEnumerable<RecognitionEntity<string>> Entities { get; set; }

        public void Convert(dynamic result)
        {
          //  var app = JsonConvert.DeserializeObject<ContactDialogBot>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

        }
    }

    public class RecognitionIntent
    {
        public string Name { get; set; }
        public double Score { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
    public class RecognitionEntity<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }


}
