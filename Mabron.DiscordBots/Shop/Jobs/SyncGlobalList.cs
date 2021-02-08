using Discord;
using Discord.WebSocket;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Shop.Jobs
{
    class SyncGlobalList : IJob
    {
        private async Task WaitForConnection()
        {
            using var canceller = new CancellationTokenSource();
            Task Handler()
            {
                canceller.Cancel();
                return Task.CompletedTask;
            }
            Program.DiscordClient!.Connected += Handler;
            if (Program.DiscordClient!.ConnectionState != ConnectionState.Connected)
            {
                try { await Task.Delay(-1, canceller.Token); }
                catch (TaskCanceledException) { }
            }
            Program.DiscordClient!.Connected -= Handler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (Program.DiscordClient == null || (!Program.Config?[0]["shop.enable"]?.Bool ?? false))
                return;
            await WaitForConnection();
            await RemoveOldMessages();
            var rooms = ShopCommand.DB.ChatRooms.Query()
                .ToArray();
            var allRooms = new List<(ChatRoom room, SocketTextChannel channel)>();
            foreach (var room in rooms) 
            {
                var rawChannel = Program.DiscordClient.GetChannel(room.Id);
                if (rawChannel is SocketTextChannel channel)
                {
                    allRooms.Add((room, channel));
                }
            }

            await foreach (var entry in new FantasyWeltCrawler().CrawlAll(
                new Uri("https://www.fantasywelt.de/Alle-deutschen-Brettspiele")))
            {
                ShopCommand.DB.Results.Upsert(entry);
                await Task.WhenAll(allRooms.Select(async x => await HandleFilter(x.room, x.channel, entry)));
            }

            await Task.WhenAll(allRooms.Select(async x => await HandleWishList(x.room, x.channel)));
        }

        private async Task RemoveOldMessages()
        {
            var channels = new Dictionary<ulong, SocketTextChannel>();
            SentToast toast;
            while ((toast = ShopCommand.DB.SentToasts.Query().FirstOrDefault()) != null)
            {
                if (!channels.TryGetValue(toast.RoomId, out SocketTextChannel? channel))
                {
                    if (!(Program.DiscordClient!.GetChannel(toast.RoomId) is SocketTextChannel rawChannel))
                        continue;
                    channel = rawChannel;
                }
                await channel.DeleteMessageAsync(toast.MessageId);
                ShopCommand.DB.SentToasts.Delete(toast.Id);
            }
        }

        private async Task HandleFilter(ChatRoom room, SocketTextChannel channel, CrawlerResult entry)
        {
            if (!room.Active)
                return;
            ShopCommand.DB.Results.Upsert(entry);
            foreach (var ignore in room.Ignores)
                if (entry.Name.ToLower().Contains(ignore.ToLower()))
                    return;
            var hasOne = room.Required.Count == 0;
            foreach (var item in room.Required)
                if (entry.Name.ToLower().Contains(item.ToLower()))
                {
                    hasOne = true;
                    break;
                }
            if (!hasOne)
                return;
            if (entry.ShopPrice / entry.RetailPrice > 1 - room.DiscountLimit)
                return;
            await Handle(channel, entry);
        }

        private async Task Handle(SocketTextChannel channel, CrawlerResult entry)
        {
            var embed = new EmbedBuilder()
            {
                Title = System.Net.WebUtility.HtmlDecode(entry.Name),
                Url = entry.SourceWebsite,
                Fields =
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Price",
                        Value = $"{entry.ShopPrice:#,#0.00} €",
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Retail",
                        Value = $"{entry.RetailPrice:#,#0.00} €",
                        IsInline = true
                    },
                },
                ImageUrl = entry.ItemImage,
            };
            var message = await channel.SendMessageAsync(embed: embed.Build());
            var toast = new SentToast
            {
                MessageId = message.Id,
                RoomId = message.Channel.Id,
                SentDate = message.CreatedAt.UtcDateTime,
                ShopEntry = entry,
            };
            ShopCommand.DB.SentToasts.Upsert(toast);
        }

        private async Task HandleWishList(ChatRoom room, SocketTextChannel channel)
        {
            var crawler = new FantasyWeltItemCrawler();
            foreach (var wish in room.WishList)
            {
                var entry = ShopCommand.DB.Results.Query()
                    .Where(x => x.SourceWebsite == wish)
                    .FirstOrDefault();
                if (entry == null)
                {
                    try { entry = await crawler.CrawlSingle(new Uri(wish)); }
                    catch (Exception e)
                    {
                        await Console.Out.WriteLineAsync($"[{DateTime.Now:G}] [Error] [Crawl] Error wishlist {wish}: {e}");
                        continue;
                    }
                    if (entry == null)
                        continue;
                    ShopCommand.DB.Results.Upsert(entry);
                }
                await Handle(channel, entry);
            }
        }
    }
}
