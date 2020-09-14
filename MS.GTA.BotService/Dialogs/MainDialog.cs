// <copyright company="Microsoft Corporation" file="MainDialog.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MS.GTA.BotService.Bots;

    /// <summary>
    /// Main dialog.
    /// </summary>
    public class MainDialog : LogoutDialog
    {
        public string userID;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger<MainDialog> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainDialog" /> class.
        /// </summary>
        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            this.logger = logger;

            AddDialog(new OAuthPrompt(
                nameof(OAuthPrompt),
                new OAuthPromptSettings
                {
                    ConnectionName = ConnectionName,
                    Text = "Welcome to MS Recruit Bot, Please Sign In",
                    Title = "Sign In",
                    Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
                }));

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                this.PromptStepAsync,
                this.LoginStepAsync,
                this.FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), options: null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse?.Token != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You have logged in successfully!"), cancellationToken);
                // await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Token: Bearer {tokenResponse.Token}"), cancellationToken);                
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool result;
            Boolean.TryParse(stepContext.Result.ToString(), out result);

            if (result)
            {
                return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}