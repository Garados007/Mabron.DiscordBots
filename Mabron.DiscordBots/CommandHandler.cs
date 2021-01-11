using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
    }
}
