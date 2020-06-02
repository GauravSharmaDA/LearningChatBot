using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FirstCorBot.Dialogs;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;

namespace ItAuth.Service.ItAuth
{
    public class ItAuthDialog : CancelAndHelpDialog
    {
        private readonly LuisIntentRecognizer _luisRecognizer;
        public static string GreetTheCustomer = "Hi, How can I help you today?";
        public static string RepeatMessage = "Sorry, I didn't get that. Can you please try a different word?";
        public ItAuthDialog(LuisIntentRecognizer luisRecognizer) : base(nameof(ItAuthDialog))
        {
            _luisRecognizer = luisRecognizer;
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

            var luisResult = await _luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);


            if(luisResult.Intents.ContainsKey("LogTicketIntent"))
                return await stepContext.BeginDialogAsync(nameof(CreateTicketDialog), stepContext.Options, cancellationToken);
            if (luisResult.Intents.ContainsKey("EnquireTicket"))
                return await stepContext.BeginDialogAsync(nameof(EnquireTicketDialog), stepContext.Options, cancellationToken);

            stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
            GreetTheCustomer = RepeatMessage;
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
