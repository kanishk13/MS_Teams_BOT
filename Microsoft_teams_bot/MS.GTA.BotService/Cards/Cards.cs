// <copyright company="Microsoft Corporation" file="Cards.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Cards
{
    using System;
    using System.IO;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to create adaptive cards
    /// </summary>
    public static class Cards
    {
        /// <summary>
        /// Creates provide feedback adaptive card
        /// </summary>
        public static Attachment CreateProvideFeedbackAdaptiveCardAttachment()
        {
            string[] paths = { AppDomain.CurrentDomain.BaseDirectory, "Cards", "provideFeedbackCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }
    }
}
