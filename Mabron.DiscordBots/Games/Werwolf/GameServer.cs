using System.Text.RegularExpressions;
using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default;
using MaxLib.WebServer;
using MaxLib.WebServer.Api.Rest;
using MaxLib.WebServer.Post;
using MaxLib.WebServer.Services;
using MaxLib.WebServer.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mabron.DiscordBots.Games.Werwolf
{
    public static class GameServer
    {
        private static Server? server;
        private static WebSocketService? webSocket;

        class PostRule : ApiRule
        {
            public string Target { get; }

            public PostRule(string target)
                => Target = target;

            public override bool Check(RestQueryArgs args)
            {
                if (args.Post.Data is UrlEncodedData data)
                {
                    args.ParsedArguments[Target] = data;
                    return true;
                }
                else return false;
            }
        }

        public static void Start()
        {
            Theme.SetupDB();

            var port = Program.Config?[0]["game.werwolf.server.port"]?.Int16 ?? 6712;
            var config = new WebServerSettings(port, 5000);
            server = new Server(config);
            server.AddWebService(new HttpRequestParser());
            server.AddWebService(new HttpHeaderSpecialAction());
            server.AddWebService(new Http404Service());
            server.AddWebService(new HttpResponseCreator());
            server.AddWebService(new HttpSender());

            // init file storage
            var searcher = new HttpDocumentFinder();
            if (System.Diagnostics.Debugger.IsAttached)
                searcher.Add(new HttpDocumentFinder.Rule("/content/", "../../../content/", false, true));
            else searcher.Add(new HttpDocumentFinder.Rule("/content/", "content/", false, true));
            server.AddWebService(searcher);
            server.AddWebService(new HttpDirectoryMapper(false));
            server.AddWebService(new DisallowRootAccess());
            server.AddWebService(new GameService());

            // init ws
            webSocket = new WebSocketService();
            webSocket.Add(new GameWebSocketEndpoint());
            server.AddWebService(webSocket);

            // init api
            var api = new RestApiService("api");
            var fact = new ApiRuleFactory();
            api.RestEndpoints.AddRange(new[]
            {
                RestActionEndpoint.Create(RolesAsync)
                    .Add(fact.Location(fact.UrlConstant("roles"))),
                RestActionEndpoint.Create<string>(GameAsync, "token")
                    .Add(fact.Location(
                        fact.UrlConstant("game"), 
                        fact.UrlArgument("token"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, UrlEncodedData>(SetConfigAsync, "token", "post")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("config"),
                        fact.MaxLength()
                    ))
                    .Add(new PostRule("post")),
                RestActionEndpoint.Create<string, UrlEncodedData>(SetUserConfigAsync, "token", "post")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("user"),
                        fact.UrlConstant("config"),
                        fact.MaxLength()
                    ))
                    .Add(new PostRule("post")),
                RestActionEndpoint.Create<string>(GameStartAsync, "token")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("start"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, ulong>(VotingStartAsync, "token", "vid")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("voting"),
                        fact.UrlArgument<ulong>("vid", ulong.TryParse),
                        fact.UrlConstant("start"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, int, ulong>(VoteAsync, "token", "id", "vid")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("voting"),
                        fact.UrlArgument<ulong>("vid", ulong.TryParse),
                        fact.UrlConstant("vote"),
                        fact.UrlArgument<int>("id", int.TryParse),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, ulong>(VotingWaitAsync, "token", "vid")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("voting"),
                        fact.UrlArgument<ulong>("vid", ulong.TryParse),
                        fact.UrlConstant("wait"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, ulong>(FinishVotingAsync, "token", "vid")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("voting"),
                        fact.UrlArgument<ulong>("vid", ulong.TryParse),
                        fact.UrlConstant("finish"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string>(NextRoundAsync, "token")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("next"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string>(FinishGameAsync, "token")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("stop"),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, ulong>(KickUserAsync, "token", "user")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("kick"),
                        fact.UrlArgument<ulong>("user", ulong.TryParse),
                        fact.MaxLength()
                    )),
            });
            server.AddWebService(api);

            // start
            server.Start();
        }

        public static void Stop()
        {
            server?.Stop();
            GameController.Current.Dispose();
            webSocket?.Dispose();
        }

        private static async Task<HttpDataSource> RolesAsync()
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            var theme = new DefaultTheme();
            writer.WriteStartArray(theme.GetType().FullName ?? "");
            foreach (var template in theme.GetRoleTemplates())
            {
                writer.WriteStringValue(template.GetType().Name);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> GameAsync(string token)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            var result = GameController.Current.GetFromToken(token);
            if (result != null)
            {
                var (game, user) = result.Value;
                var ownRole = game.TryGetRole(user.DiscordId);

                writer.WriteStartObject("game");

                writer.WriteString("leader", game.Leader.ToString());

                writer.WriteBoolean("running", game.IsRunning);

                new Events.NextPhase(game.Phase?.Current)
                    .WriteContent(writer, game, user);

                var winner = game.Winner;
                writer.WriteStartObject("participants");
                foreach (var participant in game.Participants.ToArray())
                {
                    if (participant.Value == null)
                        writer.WriteNull(participant.Key.ToString());
                    else
                    {
                        var seenRole = (game.Leader == user.DiscordId && !game.LeaderIsPlayer) || 
                                participant.Key == user.DiscordId || 
                                (winner != null && winner.Value.round == game.ExecutionRound) ||
                                (ownRole != null && game.DeadCanSeeAllRoles && !ownRole.IsAlive)?
                            participant.Value :
                            ownRole != null ?
                            participant.Value.ViewRole(ownRole) :
                            null;

                        writer.WriteStartObject(participant.Key.ToString());
                        writer.WriteBoolean("alive", participant.Value.IsAlive);
                        writer.WriteBoolean("major", participant.Value.IsMajor);
                        writer.WriteStartArray("tags");
                        foreach (var tag in participant.Value.GetTags(
                            game,
                            (game.Leader == user.DiscordId && !game.LeaderIsPlayer) ||
                            (ownRole != null && game.DeadCanSeeAllRoles && !ownRole.IsAlive)
                            ? null
                            : ownRole
                        ))
                            writer.WriteStringValue(tag);
                        writer.WriteEndArray();
                        writer.WriteString("role", seenRole?.GetType().Name);
                        writer.WriteEndObject();
                    }
                }
                writer.WriteEndObject();

                writer.WriteStartObject("user");
                foreach (var (id, entry) in game.UserCache.ToArray())
                {
                    writer.WriteStartObject($"{id}");
                    writer.WriteString("name", entry.Username);
                    writer.WriteString("img", entry.Image);
                    writer.WriteStartObject("stats");
                    writer.WriteNumber("win-games", entry.StatsWinGames);
                    writer.WriteNumber("killed", entry.StatsKilled);
                    writer.WriteNumber("loose-games", entry.StatsLooseGames);
                    writer.WriteNumber("leader", entry.StatsLeader);
                    writer.WriteNumber("level", entry.Level);
                    writer.WriteNumber("current-xp", entry.CurrentXP);
                    writer.WriteNumber("max-xp", entry.LevelMaxXP);
                    writer.WriteEndObject();
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();

                if (winner != null)
                {
                    writer.WriteStartArray("winner");
                    foreach (var item in winner.Value.winner.ToArray())
                        writer.WriteStringValue(item.ToString());
                    writer.WriteEndArray();
                }
                else writer.WriteNull("winner");

                writer.WriteStartObject("config");
                foreach (var (role, amount) in game.RoleConfiguration.ToArray())
                {
                    writer.WriteNumber(role.GetType().Name, amount);
                }
                writer.WriteEndObject();

                writer.WriteBoolean("leader-is-player", game.LeaderIsPlayer);
                writer.WriteBoolean("dead-can-see-all-roles", game.DeadCanSeeAllRoles);
                writer.WriteBoolean("autostart-votings", game.AutostartVotings);
                writer.WriteBoolean("autofinish-votings", game.AutoFinishVotings);
                writer.WriteBoolean("voting-timeout", game.UseVotingTimeouts);
                writer.WriteBoolean("autofinish-rounds", game.AutoFinishRounds);

                writer.WriteEndObject();
                writer.WriteString("user", user.DiscordId.ToString());

                var userConfig = Theme.User!.Query()
                    .Where(x => x.DiscordId == user.DiscordId)
                    .FirstOrDefault();
                if (userConfig != null)
                {
                    writer.WriteStartObject("user-config");
                    writer.WriteString("theme", userConfig.ThemeColor ?? "#ffffff");
                    writer.WriteString("background", userConfig.BackgroundImage ?? "");
                    writer.WriteEndObject();
                }
                else writer.WriteNull("user-config");
            }
            else
            {
                writer.WriteNull("game");
                writer.WriteNull("user");
                writer.WriteNull("user-config");
            }

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> SetConfigAsync(string token, UrlEncodedData post)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            var result = GameController.Current.GetFromToken(token);
            if (result != null)
            {
                var (game, user) = result.Value;

                static string? Set(GameRoom game, GameUser user, UrlEncodedData post)
                {
                    if (game.IsRunning)
                        return "cannot change settings of running game";
                    try
                    {
                        var newLeader = game.Leader;
                        Dictionary<Role, int>? newConfig = null;
                        var leaderIsPlayer = game.LeaderIsPlayer;
                        var deadCanSeeRoles = game.DeadCanSeeAllRoles;
                        var autostartVotings = game.AutostartVotings;
                        var autoFinishVotings = game.AutoFinishVotings;
                        var votingTimeout = game.UseVotingTimeouts;
                        var autoFinishRounds = game.AutoFinishRounds;

                        if (post.Parameter.TryGetValue("leader", out string? value))
                        {
                            newLeader = ulong.Parse(value);
                            if (newLeader != game.Leader && !game.Participants.ContainsKey(newLeader))
                                return "new leader is not a member of the group";
                        }

                        if (post.Parameter.TryGetValue("config", out value))
                        {
                            var roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Split(':', StringSplitOptions.RemoveEmptyEntries))
                                .Where(x => x.Length == 2)
                                .Select(x => (name: x[0], count: int.Parse(x[1])))
                                .ToDictionary(x => x.name, x => x.count);
                            newConfig = new Dictionary<Role, int>();
                            var known = (game.Theme?.GetRoleTemplates() ?? Enumerable.Empty<Role>())
                                .ToDictionary(x => x.GetType().Name);
                            foreach (var (role, count) in roles)
                                if (known.TryGetValue(role, out Role? entry))
                                {
                                    if (newConfig.ContainsKey(entry))
                                        newConfig[entry] += count;
                                    else newConfig.Add(entry, count);
                                }
                                else return $"unknown role '{role}'";
                            foreach (var (role, newCount) in newConfig)
                            {
                                var count = newCount;
                                if (!game.Theme!.CheckRoleUsage(role, ref count, 
                                    game.RoleConfiguration.TryGetValue(role, out int oldValue)
                                    ? oldValue : 0, out string? error
                                ))
                                    return error;
                                if (newCount != count)
                                    newConfig[role] = count;
                            }
                        }

                        if (post.Parameter.TryGetValue("leader-is-player", out value))
                        {
                            leaderIsPlayer = bool.Parse(value);
                        }

                        if (post.Parameter.TryGetValue("dead-can-see-all-roles", out value))
                        {
                            deadCanSeeRoles = bool.Parse(value);
                        }

                        if (post.Parameter.TryGetValue("autostart-votings", out value))
                        {
                            autostartVotings = bool.Parse(value);
                        }

                        if (post.Parameter.TryGetValue("autofinish-votings", out value))
                        {
                            autoFinishVotings = bool.Parse(value);
                        }

                        if (post.Parameter.TryGetValue("voting-timeout", out value))
                        {
                            votingTimeout = bool.Parse(value);
                        }

                        if (post.Parameter.TryGetValue("autofinish-rounds", out value))
                        {
                            autoFinishRounds = bool.Parse(value);
                        }

                        if (autoFinishVotings && votingTimeout)
                            return "you cannot have 'auto finish votings' and 'voting timeout' activated at the same time.";

                        if (leaderIsPlayer)
                        {
                            autostartVotings = true;
                            votingTimeout = true;
                            autoFinishVotings = false;
                            autoFinishRounds = true;
                        }

                        // input is valid use the new data
                        if (game.Leader != newLeader)
                        {
                            if (!game.LeaderIsPlayer)
                            {
                                game.Participants.TryAdd(game.Leader, null);
                                game.Participants!.Remove(newLeader, out _);
                            }
                            game.Leader = newLeader;
                            _ = game.Message?.ModifyAsync(x => x.Embed = GameCommand.GetGameEmbed(game));
                        }
                        if (newConfig != null)
                        {
                            game.RoleConfiguration.Clear();
                            foreach (var (k, p) in newConfig)
                                game.RoleConfiguration.TryAdd(k, p);
                        }
                        game.LeaderIsPlayer = leaderIsPlayer;
                        game.DeadCanSeeAllRoles = deadCanSeeRoles;
                        game.AutostartVotings = autostartVotings;
                        game.AutoFinishVotings = autoFinishVotings;
                        game.UseVotingTimeouts = votingTimeout;
                        game.AutoFinishRounds = autoFinishRounds;
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }
                    return null;
                }

                if (user.DiscordId == game.Leader)
                {
                    writer.WriteString("error", Set(game, user, post));
                }
                else
                {
                    writer.WriteString("error", "you are not the leader of the group");
                }
            }
            else
            {
                writer.WriteString("error", "token not found");
            }

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> SetUserConfigAsync(string token, UrlEncodedData post)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            var result = GameController.Current.GetFromToken(token);
            if (result != null)
            {
                var (game, user) = result.Value;

                static string? Set(GameRoom game, GameUser user, UrlEncodedData post)
                {
                    var userConfig = Theme.User!.Query()
                        .Where(x => x.DiscordId == user.DiscordId)
                        .FirstOrDefault();
                    
                    if (userConfig == null)
                        return "user config not found";

                    try
                    {
                        var newTheme = userConfig.ThemeColor;
                        var newBackground = userConfig.BackgroundImage;

                        if (post.Parameter.TryGetValue("theme", out string? value))
                        {
                            var check = new Regex("^#[0-9a-fA-F]{6}$");
                            if (!check.IsMatch(value))
                                return "invalid theme color";
                            newTheme = value;
                        }

                        if (post.Parameter.TryGetValue("background", out value))
                        {
                            if (value != "" && !Uri.TryCreate(value, UriKind.Absolute, out _))
                                return "invalid url";
                            newBackground = value;
                        }

                        // input is valid use the new data
                        userConfig.ThemeColor = newTheme;
                        userConfig.BackgroundImage = newBackground;
                        Theme.User.Update(userConfig);
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }
                    return null;
                }

                writer.WriteString("error", Set(game, user, post));
            }
            else
            {
                writer.WriteString("error", "token not found");
            }

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> GameStartAsync(string token)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(Utf8JsonWriter writer, string token)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (game.IsRunning)
                    return "game is already running";

                if (!game.FullConfiguration)
                    return "some roles are missing or there are to much roles defined";

                var random = new Random();
                var roles = game.RoleConfiguration
                    .SelectMany(x => Enumerable.Repeat(x.Key, x.Value))
                    .Select(x => x.CreateNew())
                    .ToList();
                var players = game.Participants.Keys.ToArray();

                foreach (var player in players)
                {
                    var index = random.Next(roles.Count);
                    game.Participants[player] = roles[index];
                    roles.RemoveAt(index);
                }

                game.StartGame();
                game.IsRunning = true;
                return null;
            }
            writer.WriteString("error", Do(writer, token));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> VotingStartAsync(string token, ulong vid)
        {

            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token, ulong vid)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (game.LeaderIsPlayer)
                    return "as a player you cannot start a voting";

                var voting = game.Phase?.Current.Votings.Where(x => x.Id == vid).FirstOrDefault();

                if (voting == null)
                    return "no voting exists";

                if (voting.Started)
                    return "voting already started";

                voting.Started = true;
                if (game.UseVotingTimeouts)
                    voting.SetTimeout(game, true);

                return null;
            }
            writer.WriteString("error", Do(token, vid));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> VoteAsync(string token, int id, ulong vid)
        {

            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(Utf8JsonWriter writer, string token, int id, ulong vid)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                var voting = game.Phase?.Current.Votings.Where(x => x.Id == vid).FirstOrDefault();
                if (voting == null)
                    return "no voting exists";

                if (!game.Participants.TryGetValue(user.DiscordId, out Role? ownRole))
                    ownRole = null;
                if (ownRole == null || !voting.CanVote(ownRole))
                    return "you are not allowed to vote";

                if (!voting.Started)
                    return "voting is not started";
                
                return voting.Vote(game, user.DiscordId, id);
            }
            writer.WriteString("error", Do(writer, token, id, vid));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> VotingWaitAsync(string token,  ulong vid)
        {

            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(Utf8JsonWriter writer, string token,  ulong vid)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                var voting = game.Phase?.Current.Votings.Where(x => x.Id == vid).FirstOrDefault();
                if (voting == null)
                    return "no voting exists";

                if (!game.Participants.TryGetValue(user.DiscordId, out Role? ownRole))
                    ownRole = null;
                if ((user.DiscordId != game.Leader || game.LeaderIsPlayer) 
                    && (ownRole == null || !voting.CanVote(ownRole)))
                    return "you are not allowed to vote";

                if (!voting.Started)
                    return "voting is not started";

                if (!game.UseVotingTimeouts)
                    return "there are no timeouts activated for this voting";

                if (!voting.SetTimeout(game, false))
                    return "timeout already reseted. Try later!";

                return null;
            }
            writer.WriteString("error", Do(writer, token, vid));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> FinishVotingAsync(string token, ulong vid)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token, ulong vid)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (game.LeaderIsPlayer)
                    return "as a player you cannot finish a voting";

                var voting = game.Phase?.Current.Votings.Where(x => x.Id == vid).FirstOrDefault();
                if (voting == null)
                    return "no voting exists";

                if (!voting.Started)
                    return "voting is not started";

                voting.FinishVoting(game);

                return null;
            }
            writer.WriteString("error", Do(token, vid));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> NextRoundAsync(string token)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (game.Phase == null)
                    return "there is no current phase";

                game.NextPhase();

                return null;
            }
            writer.WriteString("error", Do(token));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        private static async Task<HttpDataSource> FinishGameAsync(string token)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (!game.IsRunning)
                    return "the game is not running";

                game.StopGame(null);

                return null;
            }
            writer.WriteString("error", Do(token));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }

        public static async Task<HttpDataSource> KickUserAsync(string token, ulong user)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token, ulong userId)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.DiscordId != game.Leader)
                    return "you are not the leader of the group";

                if (!game.Participants.ContainsKey(userId))
                    return "player is not a participant";

                game.Participants!.Remove(userId, out _);
                game.UserCache.Remove(userId, out _);

                return null;
            }
            writer.WriteString("error", Do(token, user));

            writer.WriteEndObject();
            await writer.FlushAsync();
            stream.Position = 0;
            return new HttpStreamDataSource(stream)
            {
                MimeType = MimeType.ApplicationJson,
            };
        }
    }
}
