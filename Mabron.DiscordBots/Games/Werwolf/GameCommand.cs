using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Mabron.DiscordBots.Games.Werwolf
{
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
            var message = (SocketUserMessage)await ReplyAsync(
                embed: GetGameEmbed(game)
            );
            CommandHandler.AddReactionHandler(message.Id, 
                async (reaction, added) => 
                {
                    var user = (SocketUser)reaction.User;
                    if (user.IsBot)
                        return;
                    if (added)
                    {
                        if (game.AddParticipant(user))
                            await SendInvite(game, user);
                    }
                    else
                    {
                        game.RemoveParticipant(user);
                    }
                    await message.ModifyAsync(
                        properties =>
                        {
                            properties.Embed = GetGameEmbed(game);
                        }
                    );
                });

            await message.AddReactionAsync(Emote.Parse(":white_check_mark:"));
            await message.AddReactionAsync(Emote.Parse(":x:"));
            await SendInvite(game, Context.Message.Author);
        }

        Embed GetGameEmbed(GameRoom game)
        {
            var fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder
            {
                IsInline = false,
                Name = "Teilnehmer",
                Value = $"{game.Participants.Count + 1}",
            });
            fields.Add(new EmbedFieldBuilder
            {
                IsInline = true,
                Name = $"<@{game.Leader}>",
                Value = "Leiter",
            });
            foreach (var user in game.Participants.Keys)
                fields.Add(new EmbedFieldBuilder
                {
                    IsInline = true,
                    Name = $"<@{user}>",
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
            var url = $"{urlBase}/game/{GameController.Current.GetUserToken(game, user)}";
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