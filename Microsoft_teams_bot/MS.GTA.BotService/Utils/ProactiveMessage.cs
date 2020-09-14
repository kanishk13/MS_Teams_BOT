// <copyright company="Microsoft Corporation" file="ProactiveMessage.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Class to send proactive message
    /// </summary>
    public class ProactiveMessage
    {
        private ICredentialProvider credentialProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProactiveMessage" /> class.
        /// </summary>
        public ProactiveMessage(ICredentialProvider credentialProvider)
        {
            this.credentialProvider = credentialProvider;
        }

        private async Task<MicrosoftAppCredentials> GetMicrosoftAppCredentialsAsync()
        {
            string botClientID = "";
            // string botClientSecret = "";
            string botClientSecret = await this.credentialProvider.GetAppPasswordAsync(botClientID).ConfigureAwait(false);
            return new MicrosoftAppCredentials(botClientID, botClientSecret);
        }

        /// <summary>
        /// Creates proactive conversation with user
        /// </summary>
        public async Task<ConversationResourceResponse> CreateConversation(ITurnContext turnContext, string userTeamsId)
        {
            var tenantId = turnContext.Activity.Conversation.TenantId;
            var serviceUrl = turnContext.Activity.ServiceUrl;

            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            ConnectorClient client = new ConnectorClient(new Uri(serviceUrl), await GetMicrosoftAppCredentialsAsync());
            var user = new TeamsChannelAccount(userTeamsId);

            var conversationParameter = new ConversationParameters()
            {
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo
                    {
                        Id = tenantId,
                    }
                },
                Members = new List<ChannelAccount>() { user }
            };

            var response = await client.Conversations.CreateConversationAsync(conversationParameter);
            return response;
        }

        /// <summary>
        /// Sends proactive message to user
        /// </summary>
        public async Task SendProactiveMessage(ITurnContext turnContext, string userTeamsId, Activity message)
        {
            ConnectorClient client = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), await GetMicrosoftAppCredentialsAsync());
            var response = await CreateConversation(turnContext, userTeamsId);
            await client.Conversations.SendToConversationAsync(response.Id, message);
        }
    }
}
