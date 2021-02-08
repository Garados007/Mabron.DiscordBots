using MaxLib.WebServer;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public class GameService : WebService
    {
        public GameService() 
            : base(ServerStage.CreateDocument)
        {
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            return task.Request.Location.StartsUrlWith(new[] { "game" })
                && task.Request.Location.DocumentPathTiles.Length == 2;
        }

        public override Task ProgressTask(WebProgressTask task)
        {
            task.Document.DataSources.Add(new HttpFileDataSource("content/index.html")
            {
                MimeType = MimeType.TextHtml,
            });
            return Task.CompletedTask;
        }
    }
}
