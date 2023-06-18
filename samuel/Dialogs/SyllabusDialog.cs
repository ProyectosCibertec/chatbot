// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using samuel.Data;

namespace samuel.Dialogs
{
    public class SyllabusDialog : ComponentDialog
    {
        private readonly IConfiguration _config;
        TableServiceClient tableServiceClient;

        public SyllabusDialog(IConfiguration config)
            : base(nameof(SyllabusDialog))
        {
            _config = config;
            tableServiceClient = new TableServiceClient(_config["Storage:ConnectionString"]);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: _config["Storage:Themes:TableName"]
            );
            List<Attachment> attachments = new List<Attachment>();
            IEnumerable<Theme> themes = tableClient.Query<Theme>();
            foreach (var theme in themes.ToList())
            {
                attachments.Add(
                    new HeroCard(
                        title: theme.Name,
                        text: theme.Content,
                        tap: new CardAction()
                        {
                            Type = ActionTypes.MessageBack,
                            Value = theme.Id
                        }
                    )
                    .ToAttachment()
                );
            }
            var activity = MessageFactory.Carousel(attachments);
            await stepContext.Context.SendActivityAsync(activity);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ContinueDialogAsync(cancellationToken);
        }
    }
}
