// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.Extensions.Logging;
using samuel.States;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace samuel.Bots
{
    public class SamuelBot<T> : DialogBot<T> where T : Dialog
    {
        private readonly BotState _userState;
        private IStatePropertyAccessor<WelcomeUserState> welcomeUserStateAccessor;

        public SamuelBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
            _userState = userState;
            welcomeUserStateAccessor = _userState.CreateProperty<WelcomeUserState>(nameof(WelcomeUserState));
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var didBotWelcomeUser = await welcomeUserStateAccessor.GetAsync(turnContext, () => new WelcomeUserState(), cancellationToken);
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    didBotWelcomeUser.didBotWelcomeUser = true;
                    await SendIntroCardAsync(turnContext, cancellationToken);
                }
            }
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private static async Task SendIntroCardAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.Attachments.Add(Cards.CreateWelcomeCard());
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}
