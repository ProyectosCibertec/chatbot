// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Configuration;

namespace samuel.Dialogs
{
    public class QnADialog : ComponentDialog
    {
        private readonly IConfiguration _config;

        public QnADialog(IConfiguration config)
            : base(nameof(QnADialog))
        {
            _config = config;
            AddDialog(new CodeReviewDialog(_config));
            AddDialog(new GenerateCodeDialog(_config));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                HelpStepAsync,
                SelectedOptionStepAsync,
                FinalStepAsync
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> HelpStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Analiza mi código", Synonyms = new List<string>() { "análisis" } },
                new Choice() { Value = "Genera código", Synonyms = new List<string>() { "genera" } }
            };
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("¡Hola! Te doy la bienvenida a la sección de QnA. ¿En qué quieres que te ayude?"),
                Choices = cardOptions
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> SelectedOptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch (((FoundChoice)stepContext.Result).Value) {
                case "Analiza mi código":
                    return await stepContext.BeginDialogAsync(nameof(CodeReviewDialog), null, cancellationToken);
                case "Genera código":
                    return await stepContext.BeginDialogAsync(nameof(GenerateCodeDialog), null, cancellationToken);
                default:
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
