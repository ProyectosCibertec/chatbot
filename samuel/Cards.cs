// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public static class Cards
    {
        public static Attachment CreateWelcomeCard()
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "welcomeCard.json" };
            var samuelRobotPath = Path.Combine(Environment.CurrentDirectory, @"Resources", "Images", "samuel-personaje.png");
            var imageData = Convert.ToBase64String(File.ReadAllBytes(samuelRobotPath));
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardJson);
            var myData = new
            {
                SamuelRobotUrl = imageData
            };
            var welcomeCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(template.Expand(myData)),
            };
            return welcomeCardAttachment;
        }
    }
}
