using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Shop
{
    [Group("shop")]
    public class ShopCommand : ModuleBase<SocketCommandContext>
    {
        public static Database DB { get; }

        static ShopCommand()
        {
            DB = new Database();
        }

        public static void Dispose()
        { 
            DB.Dispose();
        }

        private ChatRoom GetRoom()
        {
            var room = DB.ChatRooms.Query()
                .Where(x => x.Id == Context.Channel.Id)
                .FirstOrDefault();
            if (room != null)
                return room;
            room = new ChatRoom
            {
                Id = Context.Channel.Id,
                Active = false
            };
            DB.ChatRooms.Insert(room);
            return room;
        }

        [Command("enable")]
        [Summary("Enable events for this text channel")]
        public async Task EnableAsync()
        {
            var room = GetRoom();
            room.Active = true;
            DB.ChatRooms.Update(room);
            await ReplyAsync("The bot is now enabled for this text channel and will post new events");
        }

        [Command("disable")]
        [Summary("Disable events for this text channel")]
        public async Task DisableAsync()
        {
            var room = GetRoom();
            room.Active = false;
            DB.ChatRooms.Update(room);
            await ReplyAsync("The bot is now disabled for this text channel and won't post any events");
        }

        [Command("require")]
        [Summary("Get the list of required keywords")]
        public async Task RequireAsync()
        {
            var room = GetRoom();
            await ReplyAsync($"**Required keywords:**\n" +
                string.Join(", ", room.Required.Select(x => $"`{x}`"))
            );
        }

        [Command("require")]
        [Summary("Set the required keywords in the game names")]
        public async Task RequireAsync(string keyword)
        {
            var room = GetRoom();
            if (room.Required.Contains(keyword))
            {
                await ReplyAsync($"Keyword `{keyword}` already added.");
                return;
            }
            room.Required.Add(keyword);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"keyword `{keyword}` added to the required list");
        }

        [Command("remove required")]
        [Summary("Remove a required keyword for game names.")]
        public async Task RemoveRequiredAsync(string keyword)
        {
            var room = GetRoom();
            if (!room.Required.Contains(keyword))
            {
                await ReplyAsync($"Keyword `{keyword}` was already removed.");
                return;
            }
            room.Required.Remove(keyword);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"keyword `{keyword}` removed from the required list");
        }

        [Command("ignore")]
        [Summary("Get the list of ignored keywords")]
        public async Task IgnoreAsync()
        {
            var room = GetRoom();
            await ReplyAsync($"**Ignored keywords:**\n" +
                string.Join(", ", room.Ignores.Select(x => $"`{x}`"))
            );
        }

        [Command("ignore")]
        [Summary("Set the ignored keywords in the game names")]
        public async Task IgnoreAsync(string keyword)
        {
            var room = GetRoom();
            if (room.Ignores.Contains(keyword))
            {
                await ReplyAsync($"Keyword `{keyword}` already added.");
                return;
            }
            room.Ignores.Add(keyword);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"keyword `{keyword}` added to the ignored list");
        }

        [Command("remove ignored")]
        [Summary("Remove an ignored keyword for game names.")]
        public async Task RemoveIgnoredAsync(string keyword)
        {
            var room = GetRoom();
            if (!room.Ignores.Contains(keyword))
            {
                await ReplyAsync($"Keyword `{keyword}` was already removed.");
                return;
            }
            room.Ignores.Remove(keyword);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"keyword `{keyword}` removed from the ignored list");
        }

        [Command("wishlist")]
        [Summary("Get the current wished games")]
        public async Task WishlistAsync()
        {
            var room = GetRoom();
            await ReplyAsync($"**Wished games:**\n" +
                string.Join("\n", room.WishList.Select(x => $"<{x}>"))
            );
        }

        [Command("wishlist")]
        [Summary("Set the current wishlist")]
        public async Task WishlistAsync(string link)
        {
            var room = GetRoom();
            if (room.WishList.Contains(link))
            {
                await ReplyAsync($"<{link}> already added to wishlist");
                return;
            }

            var allowed = new Regex(@"^https://www\.fantasywelt\.de/[\w\-]+/?$");
            if (!allowed.IsMatch(link))
            {
                await ReplyAsync($"<{link}> is not a valid link for <https://www.fantasywelt.de>");
                return;
            }

            Uri uri;
            try { uri = new Uri(link); }
            catch
            {
                await ReplyAsync($"<{link}> is not a valid uri.");
                return;
            }
            link = uri.AbsoluteUri;

            room.WishList.Add(link);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"<{link}> now added to wishlist");
        }

        [Command("remove wishlist")]
        [Summary("Remove a game from the wishlist")]
        public async Task RemoveWishlistAsync(string link)
        {
            var room = GetRoom();
            if (!room.WishList.Contains(link))
            {
                await ReplyAsync($"<{link}> was already removed.");
                return;
            }
            room.WishList.Remove(link);
            DB.ChatRooms.Update(room);
            await ReplyAsync($"<{link}> removed from wishlist list");
        }

        [Command("limit")]
        [Summary("Get the current limit")]
        public async Task LimitAsync()
        {
            var room = GetRoom();
            await ReplyAsync($"The current limit is {room.DiscountLimit:#0.00%}.");
        }

        [Command("limit")]
        [Summary("Sets the current limit")]
        public async Task LimitAsync(float limit)
        {
            var room = GetRoom();
            if (limit < 0 || limit > 100)
            {
                await ReplyAsync("The limit value needs to be between 0 and 100");
                return;
            }
            room.DiscountLimit = limit * 0.01f;
            DB.ChatRooms.Update(room);
            await ReplyAsync($"The current limit is set to {room.DiscountLimit:#0.00%}.");
        }

        [Command("info")]
        [Summary("This delivers some informations")]
        public async Task InfoAsync()
        {
            await ReplyAsync("Hello back");
        }
    }
}
