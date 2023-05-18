// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.Extensions.Logging;

namespace samuel.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger _logger;
        private readonly UserState _userState;

        public MainDialog(UserState userState, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _logger = logger;
            _userState = userState;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("MainDialog.ShowCardStepAsync");
            var activity = stepContext.Context.Activity;
            if (activity.Text.Equals("Temario"))
            {
                await stepContext.BeginDialogAsync(nameof(SyllabusDialog), null, cancellationToken);
            } 
            else if (activity.Text.Equals("Ejercicios"))
            {
                await stepContext.BeginDialogAsync(nameof(ExercisesDialog), null, cancellationToken);
            }
            else if (activity.Text.Equals("QnA"))
            {
                await stepContext.BeginDialogAsync(nameof(QnADialog), null, cancellationToken);
            }
            else
            {
                await SendMainMenuMessage(stepContext, cancellationToken);
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static async Task SendMainMenuMessage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = new HeroCard
            {
                Text = "¿Qué necesitas?",
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, title: "Temario", value: "Temario"),
                    new CardAction(ActionTypes.ImBack, title: "Ejercicios", value: "Ejercicios"),
                    new CardAction(ActionTypes.ImBack, title: "QnA", value: "QnA"),
                },
            };
            var reply = MessageFactory.Attachment(card.ToAttachment());
            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
        }
    }
}
