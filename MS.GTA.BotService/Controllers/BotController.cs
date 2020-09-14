// <copyright company="Microsoft Corporation" file="BotController.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;

    /// <summary>
    /// Controller to handle user requests
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IBot bot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController" /> class.
        /// </summary>
        /// <param name="adapter">The http bot framework adapter instance.</param>
        /// <param name="bot">The instance for <see cref="IBot"/>.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            this.adapter = adapter;
            this.bot = bot;
        }

        /// <summary>
        /// Invokes the bot.
        /// </summary>
        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            await this.adapter.ProcessAsync(Request, Response, this.bot);
        }
    }
}
