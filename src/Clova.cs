using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CEK.CSharp;
using CEK.CSharp.Models;

namespace MangaClova
{
    public static class Clova
    {
        [FunctionName("Clova")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // リクエストのボディの JSON を検証して C# のクラスに変換。
            var clovaClient = new ClovaClient();
            var clovaRequest = await clovaClient.GetRequest(req.Headers["SignatureCEK"], req.Body);

            // 返事を作成
            var clovaResponse = new CEKResponse();

            switch (clovaRequest.Request.Type)
            {
                case RequestType.LaunchRequest:
                    // 起動時の処理
                    clovaResponse.AddText("こんにちは。占ってって言うと占います。");
                    clovaResponse.ShouldEndSession = false; // スキルを終わらせないように設定する
                    break;
                case RequestType.SessionEndedRequest:
                    // 終了時の処理。今回は無し。case 自体不要だけど説明用に今回は追加してる
                    break;
                case RequestType.IntentRequest:
                    // インテントの処理
                    switch (clovaRequest.Request.Intent.Name)
                    {
                        case "FortuneTellingIntent":
                            // 占いのインテント
                            var result = new[]{ "大吉", "中吉", "小吉", "末吉" }[new Random().Next(4)];
                            clovaResponse.AddText($"占いの結果は {result} です。");
                            break;
                        default:
                            // 認識できなかったインテント
                            clovaResponse.AddText("こめんなさい。よくわかりませんでした。占ってって言うと占います。");
                            clovaResponse.ShouldEndSession = false; // スキルを終わらせないように設定する
                            break;
                    }
                    break;
            }

            // レスポンスとして作成した返事の内容を返す
            return new OkObjectResult(clovaResponse);
        }
    }
}
