// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.12.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration; //added
using System.Net.Http; //added
using Microsoft.Bot.Builder.AI.QnA;

namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        // コンストラクタで QnA Maker への接続情報と HttpClient を作るための IHttpClientFactory を受け取る
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public EchoBot(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // QnA Maker に接続するためのクライアントを作る
            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                // appsetting.json に書いた設定項目 
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAEndpointKey"],
                Host = _configuration["QnAEndpointHostName"]
            },
                options: null,
                httpClient: _httpClientFactory.CreateClient()
            );
            // QnA Maker から一番マッチした質問の回答を受け取る
            var options = new QnAMakerOptions { Top = 1 };

            // デバッグ用にオウム返し
            //await turnContext.SendActivityAsync(
            //MessageFactory.Text(text: $"質問は『{turnContext.Activity.Text}』だね！")
            //);

            var response = await qnaMaker.GetAnswersAsync(turnContext, options);

            // 回答が存在したら応答する
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(
                        activity: MessageFactory.Text(response[0].Answer),
                        cancellationToken: cancellationToken
                    );
            }
            else
            {
                await turnContext.SendActivityAsync(
                        activity: MessageFactory.Text("ん？よくわからないけどおすすめはこれだよ"),
                        cancellationToken: cancellationToken
                    );
                //url貼れるのかテスト
                await turnContext.SendActivityAsync(
                MessageFactory.Text(text: $"https://line.me/S/sticker/9166427?lang=ja&ref=lsh_stickerDetail")
                );
            }
            //default
            //var replyText = $"Echo: {turnContext.Activity.Text}";
            //await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            //
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "お話ししよ〜";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
