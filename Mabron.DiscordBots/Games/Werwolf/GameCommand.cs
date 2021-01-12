using System.Collections.Generic;
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
                Context.Message.Author
            );
            var game = GameController.Current.GetGame(id)!;
            using var typing = Context.Channel.EnterTypingState();
            var message = game.Message = await Context.Channel.SendMessageAsync(
                embed: GetGameEmbed(game)
            );
            CommandHandler.AddReactionHandler(message.Id, 
                async (reaction, added) => 
                {
                    var user = (SocketUser)reaction.User;
                    if (user.IsBot || !added)
                        return;
                    if (reaction.Emote.Name == "\u2705")
                    {
                        if (game.AddParticipant(user))
                            await SendInvite(game, user);
                    }
                    else if (reaction.Emote.Name == "\u274C")
                    {
                        game.RemoveParticipant(user);
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
            await SendInvite(game, Context.Message.Author);
        }

        public static Embed GetGameEmbed(GameRoom game)
        {
            string GetName(ulong id)
            {
                if (game.UserCache.TryGetValue(id, out SocketUser? user))
                    return user.Username;
                user = Program.DiscordClient?.GetUser(id);
                if (user != null)
                {
                    game.UserCache.Add(id, user);
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
                    Value = $"{game.Participants.Count + 1}",
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
                    "teilzunehmen. Reagiere mit :x: um wieder auszutreten.",
                Fields = fields
            }.Build();
        }
    
        async Task SendInvite(GameRoom game, SocketUser user)
        {
            var urlBase = Program.Config?[0]["game.werwolf.urlbase"].String ?? "http://localhost/";
            var url = $"{urlBase}game/{GameController.Current.GetUserToken(game, user)}";
            await user.SendMessageAsync(
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