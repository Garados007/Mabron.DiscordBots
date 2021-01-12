using MaxLib.WebServer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class DisallowRootAccess : WebService
    {
        public DisallowRootAccess() : base(ServerStage.ParseRequest)
        {
            Priority = WebServicePriority.High;
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            return task.Request.Location.StartsUrlWith(new[] { "content" }) &&
                task.Request.Location.DocumentPathTiles.Contains("..");
        }

        public override Task ProgressTask(WebProgressTask task)
        {
            task.Response.StatusCode = HttpStateCode.BadRequest;
            task.Document.DataSources.Add(new HttpStringDataSource(
                "Your path contains invalid character sequences."
            ));
            task.NextStage = ServerStage.CreateResponse;
            return Task.CompletedTask;
        }
    }
}
