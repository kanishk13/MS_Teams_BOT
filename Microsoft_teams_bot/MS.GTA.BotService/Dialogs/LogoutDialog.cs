// <copyright company="Microsoft Corporation" file="LogoutDialog.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Dialogs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Logout dialog.
    /// </summary>
    public class LogoutDialog : ComponentDialog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutDialog" /> class.
        /// </summary>
        public LogoutDialog(string id, string connectionName)
            : base(id)
        {
            ConnectionName = connectionName;
        }

        /// <summary>
        /// Connection Name
        /// </summary>
        protected string ConnectionName { get; }

        /// <summary>
        /// Checks if user wants to logout.
        /// </summary>
        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDialogContent, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await this.InterruptAsync(innerDialogContent, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnBeginDialogAsync(innerDialogContent, options, cancellationToken);
        }

        /// <summary>
        /// Checks if user wants to logout.
        /// </summary>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDialogContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await this.InterruptAsync(innerDialogContent, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDialogContent, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDialogContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (innerDialogContent.Context.Activity.Type == ActivityTypes.Message)
            {
                // Allow logout anywhere in the command
                if (innerDialogContent.Context.Activity.Text.IndexOf("logout", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // The bot adapter encapsulates the authentication processes.
                    var botAdapter = (BotFrameworkAdapter)innerDialogContent.Context.Adapter;
                    await botAdapter.SignOutUserAsync(innerDialogContent.Context, ConnectionName, userId: null, cancellationToken);
                    await innerDialogContent.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."), cancellationToken);
                    return await innerDialogContent.CancelAllDialogsAsync(cancellationToken);
                }
            }

            return null;
        }
    }
}
