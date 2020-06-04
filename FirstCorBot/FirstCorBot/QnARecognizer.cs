using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Recognizers;
using Microsoft.Extensions.Configuration;

namespace ItAuth.Service
{
    public class QnARecognizer
    {
        private readonly IConfiguration _configuration;
        public QnAMaker QnAMaker;

        public QnARecognizer(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;

            QnAMaker = new QnAMaker(new QnAMakerEndpoint()
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAEndpointKey"],
                Host = _configuration["QnAEndpointHostName"]
            }, null, httpClientFactory.CreateClient());
        }

        public virtual async Task<QueryResult[]> GetAnswersAsync(ITurnContext turnContext, QnAMakerOptions options = null)
            => await QnAMaker.GetAnswersAsync(turnContext, options);
    }
}
