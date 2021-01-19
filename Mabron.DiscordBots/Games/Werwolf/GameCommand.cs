using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Mabron.DiscordBots.Games.Werwolf
{
    [Group("werwolf")]
    public class GameCommand : ModuleBase<SocketCommandContext>
    {
        [Command("start")]
        [Summary("Create a new Werwolf game")]
        public async Task StartAsync()
        {
            var id = GameController.Current.CreateGame(
                GameUser.Create(Context.Message.Author)
            );
            var game = GameController.Current.GetGame(id)!;
            using var typing = Context.Channel.EnterTypingState();
            var message = game.Message = await Context.Channel.SendMessageAsync(
                embed: GetGameEmbed(game)
            );
            CommandHandler.AddReactionHandler(message.Id, 
                async (reaction, added) => 
                {
                    if (!added)
                        return;
                    var gameUser = GameUser.Get(reaction.UserId);
                    if (gameUser == null)
                    {
                        IUser user = reaction.User.IsSpecified
                            ? (SocketUser)reaction.User
                            : Program.DiscordClient!.GetUser(reaction.UserId);
                        if (user == null)
                        {
                            user = await reaction.Channel.GetUserAsync(reaction.UserId);
                        }
                        if (user == null)
                        {
                            user = Context.Guild.GetUser(reaction.UserId);
                        }
                        if (user == null)
                        {
                            user = await Program.DiscordRestClient!.GetUserAsync(reaction.UserId);
                        }
                        if (user == null)
                        {
                            await reaction.Channel.SendMessageAsync($"Sorry <@!{reaction.UserId}> but it seems I have no stats for you. Do you have written something" +
                                $"in this text channel before? Just write `!werwolf join {GetPublicId(game.Id)}` to join the game. Next time you can use " +
                                $"the reactions like any other user - I promise!.");
                            await message.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                            return;
                        }
                        if (user.IsBot)
                            return;
                        gameUser = GameUser.Create(user);
                    }
                    if (reaction.Emote.Name == "\u2705")
                    {
                        if (game.Participants.Count >= 500)
                        {
                            await reaction.Channel.SendMessageAsync($"I am sorry <@!{reaction.UserId}> but it seems that the game is full.");
                            return;
                        }
                        if (game.AddParticipant(gameUser))
                            await SendInvite(game, gameUser);
                    }
                    else if (reaction.Emote.Name == "\u274C")
                    {
                        game.RemoveParticipant(gameUser);
                    }
                    await message.ModifyAsync(
                        properties =>
                        {
                            properties.Embed = GetGameEmbed(game);
                        }
                    );
                    await message.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                });

            await message.AddReactionAsync(new Emoji("\u2705"));
            await message.AddReactionAsync(new Emoji("\u274C"));
            await SendInvite(game, GameUser.Create(Context.Message.Author));
        }

        [Command("join")]
        [Summary("Join an existing Werwolf game")]
        public async Task JoinAsync(string id)
        {
            var decodedId = GetLocalId(id);
            GameRoom? game = decodedId != null ? GameController.Current.GetGame(decodedId.Value) : null;
            using var typing = Context.Channel.EnterTypingState();
            if (game == null)
            {
                await ReplyAsync($"Game with ID {id} not found.");
                return;
            }
            if (Context.User.IsBot)
                return;
            if (game.Participants.Count >= 500)
            {
                await ReplyAsync("Game is full");
                return;
            }
            var gameUser = GameUser.Create(Context.User);
            if (game.AddParticipant(gameUser))
                await SendInvite(game, gameUser);
            if (game.Message != null)
                await game.Message.ModifyAsync(
                    properties =>
                    {
                        properties.Embed = GetGameEmbed(game);
                    }
                );
        }

        static readonly Regex readUserId = new Regex("^<@!(?<id>\\d+)>$", RegexOptions.Compiled);

        [Command("join")]
        [Summary("Let a user join an existing game")]
        public async Task JoinAsync(string id, string user)
        {
            _ = Task.Run(async () =>
            {
                IUser? realUser = null;
                var match = readUserId.Match(user);
                if (match.Success)
                {
                    var userId = ulong.Parse(match.Groups["id"].Value);
                    realUser = await Context.Channel.GetUserAsync(userId);
                    if (realUser == null)
                        realUser = await Program.DiscordRestClient!.GetUserAsync(userId);
                }

                if (realUser == null)
                {
                    await ReplyAsync($"Sorry cannot find the user. Insert the name with \"\\@user\" so I can find it.");
                    return;
                }

                var decodedId = GetLocalId(id);
                GameRoom? game = decodedId != null ? GameController.Current.GetGame(decodedId.Value) : null;
                using var typing = Context.Channel.EnterTypingState();
                if (game == null)
                {
                    await ReplyAsync($"Game with ID {id} not found.");
                    return;
                }
                if (Context.User.IsBot)
                    return;
                if (realUser.IsBot)
                {
                    await ReplyAsync($"Sorry but the bot <@!{realUser.Id}> is not allowed to enter.");
                    return;
                }
                if (game.Participants.Count >= 500)
                {
                    await ReplyAsync("Game is full");
                    return;
                }
                var gameUser = GameUser.Create(realUser);
                if (game.AddParticipant(gameUser))
                    try { await SendInvite(game, gameUser); }
                    catch (Discord.Net.HttpException exception)
                    {
                        game.RemoveParticipant(gameUser);
                        await ReplyAsync(
                            $"Unfortunately I cannot send <@!{realUser.Id}> a private message. Is (s)he on this server?\n" +
                            $"```\n{exception.GetType()}: {exception.Message}\n```"
                        );
                        return;
                    }
                if (game.Message != null)
                    await game.Message.ModifyAsync(
                        properties =>
                        {
                            properties.Embed = GetGameEmbed(game);
                        }
                    );
            });
            await Task.CompletedTask;
        }

        private static string GetPublicId(int id)
        {
            var bytes = BitConverter.GetBytes(id);
            var mask = new[] { 0b01010101, 0b10101010, 0b11001100, 0b00110011 };
            for (int i = 0; i < 4; ++i)
                bytes[i] = (byte)(bytes[i] ^ mask[i]);
            var encoded = Convert.ToBase64String(bytes);
            return encoded.TrimEnd('=');
        }

        private static int? GetLocalId(string rawId)
        {
            try
            {
                var pad = 4 - (rawId.Length % 4);
                if (pad == 4)
                    pad = 0;
                rawId = rawId.PadRight(rawId.Length + pad, '=');
                var bytes = Convert.FromBase64String(rawId);
                var mask = new[] { 0b01010101, 0b10101010, 0b11001100, 0b00110011 };
                for (int i = 0; i < 4; ++i)
                    bytes[i] = (byte)(bytes[i] ^ mask[i]);
                return BitConverter.ToInt32(bytes, 0);
            }
            catch
            {
                return null;
            }
        }

        public static Embed GetGameEmbed(GameRoom game)
        {
            string GetName(ulong id)
            {
                if (game.UserCache.TryGetValue(id, out GameUser? user))
                    return user.Username;
                var discordUser = Program.DiscordClient?.GetUser(id);
                user = discordUser != null ? GameUser.Create(discordUser) : null;
                if (user != null)
                {
                    game.UserCache.TryAdd(id, user);
                    return user.Username;
                }
                return $"User {id}";
            }

            var fields = new List<EmbedFieldBuilder>
            {
                new EmbedFieldBuilder
                {
                    IsInline = false,
                    Name = "Teilnehmer",
                    Value = $"{game.Participants.Count + 1}/500",
                },
                new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = GetName(game.Leader),
                    //Name = $"<@{game.Leader}>",
                    Value = "Leiter",
                }
            };
            foreach (var user in game.Participants.Keys)
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = GetName(user),
                    Value = "Spieler",
                });
            return new EmbedBuilder
            {
                Title = "Werwölfe von Düsterwald",
                Description = "Reagiere mit :white_check_mark: um an dem Spiel " +
                    "teilzunehmen. Reagiere mit :x: um wieder auszutreten. " +
                    $"Alternativ kannst du auch mit `!werwolf join {GetPublicId(game.Id)}` " +
                    $"antworten.",
                Fields = fields
            }.Build();
        }
    
        async Task SendInvite(GameRoom game, GameUser user)
        {
            var urlBase = Program.Config?[0]["game.werwolf.urlbase"].String ?? "http://localhost/";
            var url = $"{urlBase}game/{GameController.Current.GetUserToken(game, user)}";
            IUser discordUser = Program.DiscordClient!.GetUser(user.DiscordId);
            if (discordUser == null)
            {
                discordUser = await Program.DiscordRestClient!.GetUserAsync(user.DiscordId);
            }
            await discordUser.SendMessageAsync(
                embed: new EmbedBuilder
                {
                    Title = "Willkommen bei Werwölfe von Düsterwald",
                    Description = "Tritt der Spielelobby einfach hier bei um mit den anderen zu " +
                        $"spielen: <{url}>.",
                    Url = url
                }.Build()
            );
        }
    }
}