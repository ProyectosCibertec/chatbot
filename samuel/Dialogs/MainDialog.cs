// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace samuel.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        private readonly UserState _userState;
        private readonly IConfiguration _config;

        public MainDialog(UserState userState, ILogger<MainDialog> logger, IConfiguration config)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;
            _config = config;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new SyllabusDialog(_config));
            AddDialog(new ExercisesDialog());
            AddDialog(new QnADialog(_config));
            AddDialog(new HelpDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                ConfirmationStepAsync,
                FinalStepAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ShowCardStepAsync");
            var activity = stepContext.Context.Activity;
            switch (activity.Text)
            {
                case "Temario":
                    return await stepContext.BeginDialogAsync(nameof(SyllabusDialog), null, cancellationToken);
                case "Ejercicios":
                    return await stepContext.BeginDialogAsync(nameof(ExercisesDialog), null, cancellationToken);
                case "QnA":
                    return await stepContext.BeginDialogAsync(nameof(QnADialog), null, cancellationToken);
                case "Ayuda":
                    return await stepContext.BeginDialogAsync(nameof(HelpDialog), null, cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Lo siento, no logro entender qué es lo que necesitas, por favor selecciona una de las opciones"), cancellationToken: cancellationToken);
                    await SendMainMenuMessage(stepContext, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Si", Synonyms = new List<string>() { "Si" } },
                new Choice() { Value = "No", Synonyms = new List<string>() { "No" } }
            };
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("¿Quieres regresar al menú?"),
                Choices = cardOptions
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Si":
                    await SendMainMenuMessage(stepContext, cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Adios!"), cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private static async Task SendMainMenuMessage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "Por favor, escoge una de las opciones",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, title: "Temario", value: "Temario"),
                    new CardAction(ActionTypes.ImBack, title: "Ejercicios", value: "Ejercicios"),
                    new CardAction(ActionTypes.ImBack, title: "QnA", value: "QnA"),
                    new CardAction(ActionTypes.ImBack, title: "Ayuda", value: "Ayuda"),
                },
            };
            var reply = MessageFactory.Attachment(card.ToAttachment());
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
    }
}
