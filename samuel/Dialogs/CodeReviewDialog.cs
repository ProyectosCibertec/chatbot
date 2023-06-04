// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;

namespace samuel.Dialogs
{
    public class CodeReviewDialog : ComponentDialog
    {
        private readonly IConfiguration _config;

        public CodeReviewDialog(IConfiguration config)
            : base(nameof(CodeReviewDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                GetSourceCodeStepAsync,
                FinalStepAsync,
            }));
            InitialDialogId = nameof(WaterfallDialog);
            _config = config;
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Ingresa el código que te gustaría que analice. Si quieres salir solo escribe \'salir\'") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> GetSourceCodeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result.Equals("salir"))
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            var sourceCode = stepContext.Result;
            string endpoint = _config["OpenAI:Endpoint"];
            string key = _config["OpenAI:key"];
            OpenAIClient client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            Response<ChatCompletions> completionsResponse =
                await client.GetChatCompletionsAsync(
                _config["OpenAI:Deployment"], new ChatCompletionsOptions()
                {
                    Messages =
                    {
            new ChatMessage(ChatRole.System, $"{_config["OpenAI:Prompts:CodeReview"]} \n {sourceCode}"),
                    },
                    Temperature = (float)0.7,
                    MaxTokens = 800,
                    NucleusSamplingFactor = (float)0.95,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                });
            string completion = completionsResponse.Value.Choices[0].Message.Content;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(completion), cancellationToken: cancellationToken);
            return await stepContext.ContinueDialogAsync();
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(CodeReviewDialog), new { ResumeDialogId = "InitialStepAsync" }, cancellationToken);
        }
    }
}
