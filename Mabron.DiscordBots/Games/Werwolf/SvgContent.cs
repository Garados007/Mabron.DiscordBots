using MaxLib.WebServer;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class SvgContent : WebService
    {
        public SvgContent() 
            : base(ServerStage.ProcessDocument)
        {
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            return task.Request.Location.StartsUrlWith(new[] { "content" })
                && task.Request.Location.DocumentPath.EndsWith(".svg")
                && task.Document.DataSources.Count == 1
                && task.Document.DataSources[0].MimeType == MimeType.TextPlain;
        }

        public override Task ProgressTask(WebProgressTask task)
        {
            task.Document.DataSources[0].MimeType = "image/svg+xml";
            return Task.CompletedTask;
        }
    }
}
