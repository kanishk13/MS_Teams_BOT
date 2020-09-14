// <copyright company="Microsoft Corporation" file="TeamsBot.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BotService.Bots
{
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using MS.GTA.BOTService.BusinessLibrary.Interfaces;
    using MS.GTA.BOTService.Common.Models;
    using MS.GTA.BOTService.Data.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Bot handler class
    /// </summary>
    public class TeamsBot<T> : TeamsActivityHandler where T : Dialog
    {
        /// <summary>
        /// Conversation State
        /// </summary>
        protected readonly BotState ConversationState;

        /// <summary>
        /// Dialog
        /// </summary>
        protected readonly Dialog Dialog;

        /// <summary>
        /// User state
        /// </summary>
        protected readonly BotState UserState;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger<TeamsBot<T>> logger;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly IJobApplicationQuery jobApplicationQuery;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly IJobApplicationManager jobApplicationManager;

        private string userOID;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsBot{T}" /> class.
        /// </summary>
        public TeamsBot(ConversationState conversationState, UserState userState, T dialog, ILogger<TeamsBot<T>> logger, IJobApplicationManager jobApplicationManager, IJobApplicationQuery jobApplicationQuery)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            this.logger = logger;
            this.jobApplicationQuery = jobApplicationQuery;
            this.jobApplicationManager = jobApplicationManager;
        }

        /// <summary>
        /// Main handler
        /// </summary>
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        /// <summary>
        /// Message activity handler
        /// </summary>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Running dialog with Message Activity.");

            if (!string.IsNullOrWhiteSpace(turnContext.Activity?.Text))
            {
                //var replyText = $"You have said: {turnContext.Activity.Text}";
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                this.userOID = turnContext.Activity.From.AadObjectId;
                if(turnContext.Activity?.Text != "logout")
                {
                    await this.GetWelcomeCard(turnContext, cancellationToken);
                }
            }
            else
            {
                var ValueReceived = JsonConvert.DeserializeObject<CareerRequest>(turnContext.Activity.Value.ToString());

                switch (ValueReceived?.Action)
                {
                    case "App": await SendApplicationsSummaryCard(turnContext, cancellationToken); break;
                    case "Interview": await Interviewcard(turnContext, cancellationToken); break;
                    case "Resume": await Resumecard(turnContext, cancellationToken); break;

                }
            }
        }

        /// <summary>
        /// members added handler
        /// </summary>
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to MS Recruit!"), cancellationToken);
                    // Todo - Add teams id {turnContext.Activity.From.Id} to database
                }
            }
        }

        /// <summary>
        /// teams signin verify handler
        /// </summary>
        protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Running dialog with signin/verifystate from an Invoke Activity.");

            // The OAuth Prompt needs to see the Invoke Activity in order to complete the login process.

            // Run the Dialog with the new Invoke Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        private async Task GetWelcomeCard(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = new AdaptiveCard("1.2");

            var text2 = new AdaptiveTextBlock()
            {
                Text = $"Hello {turnContext.Activity.From.Name}, Welcome to GTA!",
                Size = AdaptiveTextSize.Medium,
                Color = AdaptiveTextColor.Default,
                Weight = AdaptiveTextWeight.Bolder,
            };

            var text3 = new AdaptiveTextBlock()
            {
                Text = "MS Recruit 1.0 is here to simplify your experience with hiring great talent. Please use the below options to get updates on your requisitions, interviews and assessments.",
                Size = AdaptiveTextSize.Medium,
                Color = AdaptiveTextColor.Default,
                Wrap = true,
                Weight = AdaptiveTextWeight.Normal,
            };

            card.Body.Add(text2); card.Body.Add(text3);

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Applications Summary",
                Data = new JObject { { "Action", "App" } },
            });

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Upcoming Interviews",
                Data = new JObject { { "Action", "Interview" } },
            });
            /*
            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Pending Feedbacks",
                Data = new JObject { { "Action", "Feedback" } },
            });  */
            var card1 = convertAdaptivetoAttacment(card);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(card1), cancellationToken);
        }


        private async Task GetCard(ITurnContext turnContext, CancellationToken cancellationToken, string userOID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userOID))
                {
                    throw new InvalidOperationException($"User OID cannot be null or Empty");
                }

                var jobOpeningSummaries = await this.jobApplicationQuery.GetActiveJobApplications(userOID);

                var card = new AdaptiveCard("1.0")
                {
                    Title = jobOpeningSummaries[0].ExternalJobOpeningID
                };

                var commandListCard = new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };

                var response = turnContext.Activity.CreateReply();
                response.Attachments = new List<Attachment>() { commandListCard };
                response.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                await turnContext.SendActivityAsync(response, cancellationToken);
            }
            catch (Exception e)
            {
                throw;
            }
        }


        private async Task SendApplicationsSummaryCard(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            var jobOpeningSummaries = await this.jobApplicationQuery.GetActiveJobApplications("24c4e3d5-821b-4b24-8be7-eaf530706f1d");

            string[] summaryNames = { "Total Applications", "Dispositioned Applications", "Applications In Review", "Interview Applications", "Applications in Assessment" };

            IMessageActivity replyincarousel;
            var attachments = new List<Attachment>();

            jobOpeningSummaries?.ForEach(jo =>
            {
                var card = new AdaptiveCard("1.2");

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = jo.ExternalJobOpeningID + ": " + jo.PositionTitle,
                    Size = AdaptiveTextSize.ExtraLarge,
                    Color = AdaptiveTextColor.Accent,
                    Weight = AdaptiveTextWeight.Bolder,
                });

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = "Summary",
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Bolder,
                });

                string[] summarydata = { jo.TotalApplications.ToString(), jo.DispositionedApplications.ToString(), jo.ReviewApplications?.Count.ToString(), jo.InterviewApplications.ToString(), jo.AssessmentApplications.ToString() };

                for (int i = 0; i < summaryNames.Length; i++)
                {
                    List<AdaptiveElement> AdaptiveStaticElement = new List<AdaptiveElement>
                    {
                        new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text= summaryNames[i],
                                        Weight= AdaptiveTextWeight.Bolder,
                                        Separator=true,
                                        Size = AdaptiveTextSize.Small,
                                        Wrap=true,
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text= summarydata[i],
                                        Weight= AdaptiveTextWeight.Bolder,
                                        Separator=true,
                                        Size = AdaptiveTextSize.Small,
                                    }
                                }
                            }
                        }
                    }
                    };

                    AdaptiveContainer adaptiveStaticContainer = new AdaptiveContainer();

                    adaptiveStaticContainer.Items = AdaptiveStaticElement;
                    card.Body.Add(adaptiveStaticContainer);
                }


                //  var subcard = new AdaptiveCard("1.2");

                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = "Applications For Review",
                    Size = AdaptiveTextSize.ExtraLarge,
                    Color = AdaptiveTextColor.Accent,
                });

                List<AdaptiveElement> AdaptiveStaticElement1 = new List<AdaptiveElement>
                {
                new AdaptiveColumnSet()
                {
                    Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Cadidate Name",
                                    Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                    Wrap=true,
                                   }
                            }
                        },
                         new AdaptiveColumn()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Email",
                                    Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                }
                            }
                        },
                          new AdaptiveColumn()
                        {
                              // Width=AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Resume",
                                    Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                }
                            }
                        },
                          new AdaptiveColumn()
                        {
                            //  Width=AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "Current Stage",
                                    Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                    Wrap = true,
                                }
                            }
                        },
                             new AdaptiveColumn()
                        {
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= "",
                                    Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                }
                            }
                        }
                    }
                }
            };
                AdaptiveContainer adaptiveStaticContainer1 = new AdaptiveContainer();

                adaptiveStaticContainer1.Items = AdaptiveStaticElement1;
                card.Body.Add(adaptiveStaticContainer1);
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "<",
                });

                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = ">",
                });

                int reviewApplicationCount = 0;
                jo.ReviewApplications?.ForEach((reviewApplication) =>
                {
                    {
                        List<AdaptiveElement> AdaptiveDynamicElements = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet()
                        {
                             Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Stretch,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text= reviewApplication.Candidate?.FullName?.GivenName + " " + reviewApplication.Candidate?.FullName?.Surname ,
                                    Weight= AdaptiveTextWeight.Lighter,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                }
                            }
                        },
                         new AdaptiveColumn()
                        {
                            Width = AdaptiveColumnWidth.Stretch,
                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextBlock()
                                {
                                    Text="["+reviewApplication.Candidate?.EmailPrimary+"]"+"(https://docs.microsoft.com/en-us/adaptive-cards/templating/)",  //"[Matt@gmail.com](https://docs.microsoft.com/en-us/adaptive-cards/templating/)",
                                    Weight= AdaptiveTextWeight.Lighter,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                }
                            },
                        },
                         new AdaptiveColumn()
                        {
                             Width = AdaptiveColumnWidth.Auto,
                            Items = new List<AdaptiveElement>()
                            {

                             new AdaptiveActionSet()
                             {
                                 Actions=new List<AdaptiveAction>()
                                 {
                                     new AdaptiveSubmitAction()
                                     {
                                         Title = "Open",

                                         Data = new JObject { { "Action", "Resume" } },
                                     }
                                 }
                             }
                            },
                        },
                          new AdaptiveColumn()
                         {
                               Width = AdaptiveColumnWidth.Stretch,
                               Items = new List<AdaptiveElement>()
                               {
                                  new AdaptiveTextBlock()
                                  {
                                      Text=reviewApplication.CurrentJobOpeningStage.ToString(),
                                      Weight= AdaptiveTextWeight.Lighter,
                                     Separator=true,
                                     Size = AdaptiveTextSize.Small,
                                  }
                               }
                         },
                         new AdaptiveColumn()
                         {
                             Width = AdaptiveColumnWidth.Auto,
                             Items=new List<AdaptiveElement>()
                             {
                                new AdaptiveActionSet()
                                {
                                    Actions=new List<AdaptiveAction>()
                                    {
                                        new AdaptiveToggleVisibilityAction()
                                        {
                                            Title="v",
                                            Type = AdaptiveToggleVisibilityAction.TypeName,
                                            TargetElements=new List<AdaptiveTargetElement>()
                                            {
                                                new AdaptiveTargetElement()
                                                {
                                                    ElementId = reviewApplicationCount.ToString()
                                                },
                                                new AdaptiveTargetElement()
                                                {
                                                    ElementId="fullname",
                                                }
                                            }
                                        }
                                    }
                                }
                              }
                         }
                    }
                        }
                        };
                        AdaptiveContainer adaptiveDynamicContainer = new AdaptiveContainer();
                        adaptiveDynamicContainer.Items = AdaptiveDynamicElements;


                        List<AdaptiveElement> ToggleElements = new List<AdaptiveElement>
                        {
                            new AdaptiveColumnSet()                {
                                Columns = new List<AdaptiveColumn>()
                    {
                        new AdaptiveColumn()
                        {

                            Items = new List<AdaptiveElement>()
                            {
                                new AdaptiveTextInput()
                                {
                                    Id= "InputText"+reviewApplicationCount,
                                    Placeholder="Comments",
                                },
                                new AdaptiveTextBlock()
                                {
                                    Text="Stage Change",
                                     Weight= AdaptiveTextWeight.Bolder,
                                    Separator=true,
                                    Size = AdaptiveTextSize.Small,
                                },
                                new AdaptiveChoiceSetInput()
                                {
                                    Id="InputCoice" + reviewApplicationCount,
                                    Choices=new List<AdaptiveChoice>()
                                    {
                                        new AdaptiveChoice()
                                        {
                                            Title="Schedule Interview",
                                            Value="1",
                                        },
                                        new AdaptiveChoice()
                                        {
                                               Title="Disposition",
                                               Value="2",
                                        },
                                    }
                                },
                               new AdaptiveActionSet()
                               {
                                Actions=new List<AdaptiveAction>()
                                {
                                    new AdaptiveSubmitAction()
                                    {
                                        Title="Submit",
                                    }
                                }
                               }
                            }
                        },
                    }
                            }
                        };


                        AdaptiveContainer toggleContainer = new AdaptiveContainer();
                        toggleContainer.Id = reviewApplicationCount.ToString();
                        toggleContainer.Items = ToggleElements;
                        toggleContainer.IsVisible = false;

                        // adding new rows in card body  
                        card.Body.Add(adaptiveDynamicContainer);
                        card.Body.Add(toggleContainer);
                    }

                    reviewApplicationCount++;
                });


                attachments.Add(convertAdaptivetoAttacment(card));

            });

            replyincarousel = MessageFactory.Carousel(attachments);
            await turnContext.SendActivityAsync(replyincarousel, cancellationToken);

        }

        private async Task Interviewcard(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var summaries = await this.jobApplicationManager.GetUpcomingInterviews(this.userOID, DateTime.Now);

            string[] staticdata = { "Interview Schedule", "Hello " + turnContext.Activity.From.Name, "Here are some Interview Schedule for next 7 days" };
            var attachments = new List<Attachment>();
            IMessageActivity replyincarousel;

            summaries?.ToList()?.ForEach(summary =>
            {
                var Interviewcard = new AdaptiveCard("1.2");
                for (int i = 0; i < 3; i++)
                {
                    Interviewcard.Body.Add(new AdaptiveTextBlock()
                    {
                        Text = staticdata[i],
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                    });
                }

                //Static Container
                var Staticcontainer = new AdaptiveContainer();
                Staticcontainer.Items = new List<AdaptiveElement>()
                {
                   new AdaptiveTextBlock()
                    {
                        Text = summary.PositionTitle,
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder,
                        Color = AdaptiveTextColor.Accent,
                    },
                   new AdaptiveColumnSet()
                   {
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Full Name",
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Date",
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text="Time",
                                    }
                                }
                            }
                        }
                   }
                };

                Interviewcard.Body.Add(Staticcontainer);

                //Static Container
                summary.ScheduleSummaries?.ToList()?.ForEach(scheduleData =>
                {
                    var Dynamiccontainer = new AdaptiveContainer();
                    Dynamiccontainer.Items = new List<AdaptiveElement>()
                    {
                        new AdaptiveColumnSet()
                        {
                            Columns=new List<AdaptiveColumn>()
                            {
                                new AdaptiveColumn()
                                {
                                    Items=new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = scheduleData.CandidateName,
                                        }
                                    }
                                },
                                new AdaptiveColumn()
                                {
                                    Items=new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = scheduleData.ScheduleStartDateTime?.ToString("MM/dd/yyyy"),
                                        }
                                    }
                                },
                                new AdaptiveColumn()
                                {
                                    Items=new List<AdaptiveElement>()
                                    {
                                        new AdaptiveTextBlock()
                                        {
                                            Text = scheduleData.ScheduleStartDateTime?.ToString("h:mm tt"),
                                        }
                                    }
                                }
                            }
                        }
                    };

                    Interviewcard.Body.Add(Dynamiccontainer);
                });

                attachments.Add(convertAdaptivetoAttacment(Interviewcard));
            });

            replyincarousel = MessageFactory.Carousel(attachments);

            await turnContext.SendActivityAsync(replyincarousel, cancellationToken);
        }

        private static async Task Resumecard(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var card = CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "Resumecard.json"));
            await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
        }

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        private static Attachment convertAdaptivetoAttacment(AdaptiveCard card)
        {
            try
            {
                var json = card.ToJson();
                var myData = new
                {
                    Name = "Matt Hidinger"
                };
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(json);
                string cardJS = template.Expand(myData);
                var adaptiveCardAttachment = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = JsonConvert.DeserializeObject(cardJS),
                };

                return adaptiveCardAttachment;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

}