using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Mabron.DiscordBots
{
    class CommandHandler
    {
        readonly DiscordSocketClient client;
        CommandService commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
            => (this.client, this.commands) = (client, commands);

        public async Task InstallCommandsAsync()
        {
            client.MessageReceived += Client_MessageReceived;
            client.ReactionAdded += Client_ReactionAdded;
            client.ReactionRemoved += Client_ReactionRemoved;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task Client_MessageReceived(SocketMessage msg)
        {
            if (msg is SocketUserMessage message)
            {
                int argPos = 0;

                if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot)
                    return;

                var context = new SocketCommandContext(client, message);

                await commands.ExecuteAsync(context, argPos, null);
            }
        }

        static readonly Dictionary<ulong, Func<SocketReaction, bool, Task>> reactionHandler
            = new Dictionary<ulong, Func<SocketReaction, bool, Task>>();
        
        public static void AddReactionHandler(ulong id, Func<SocketReaction, bool, Task> handler)
        {
            reactionHandler[id] = handler;
        }

        public static void RemoveReactionHandler(ulong id)
        {
            reactionHandler.Remove(id);
        }

        private async Task Client_ReactionAdded(
            Cacheable<IUserMessage, ulong> msg, 
            ISocketMessageChannel channel, 
            SocketReaction reaction)
        {
            if (reactionHandler.TryGetValue(msg.Id, out Func<SocketReaction, bool, Task>? handler))
            {
                await handler(reaction, true);
            }
        }

        private async Task Client_ReactionRemoved(
            Cacheable<IUserMessage, ulong> msg, 
            ISocketMessageChannel channel, 
            SocketReaction reaction)
        {
            if (reactionHandler.TryGetValue(msg.Id, out Func<SocketReaction, bool, Task>? handler))
            {
                await handler(reaction, false);
            }
        }
    }
}
