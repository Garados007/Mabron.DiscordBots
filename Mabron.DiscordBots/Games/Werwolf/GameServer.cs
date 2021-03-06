﻿using LiteDB;
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
using System.Text.RegularExpressions;
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
            server.AddWebService(new SvgContent());
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
                    .Add(fact.Location(fact.UrlConstant("roles"), fact.MaxLength())),
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
                RestActionEndpoint.Create<string, ObjectId>(KickUserAsync, "token", "user")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("kick"),
                        fact.UrlArgument<ObjectId>("user", TryParseObjectId),
                        fact.MaxLength()
                    )),
                RestActionEndpoint.Create<string, string?, UrlEncodedData>(MessageAsync, "token", "phase", "message")
                    .Add(fact.Location(
                        fact.UrlConstant("game"),
                        fact.UrlArgument("token"),
                        fact.UrlConstant("chat"),
                        fact.MaxLength()
                    ))
                    .Add(fact.Optional(fact.GetArgument("phase")))
                    .Add(new PostRule("message")),
            });
            server.AddWebService(api);

            // start
            server.Start();
        }

        public static bool TryParseObjectId(string value, out ObjectId id)
        {
            id = new ObjectId();
            try { id = new ObjectId(value); }
            catch { return false; }
            return true;
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

            var theme = new DefaultTheme(null);
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
                var ownRole = game.TryGetRole(user.Id);

                writer.WriteStartObject("game");

                writer.WriteString("leader", game.Leader.ToString());

                if (game.Phase == null)
                    writer.WriteNull("phase");
                else
                {
                    writer.WriteStartObject("phase"); // phase
                    writer.WriteString("lang-id", game.Phase.Current.LanguageId);

                    writer.WriteStartObject("stage");
                    writer.WriteString("lang-id", game.Phase.Stage.LanguageId);
                    writer.WriteString("background-id", game.Phase.Stage.BackgroundId);
                    writer.WriteString("theme", game.Phase.Stage.ColorTheme);
                    writer.WriteEndObject();

                    writer.WriteStartArray("voting"); // voting
                    foreach (var voting in game.Phase.Current.Votings)
                    {
                        if (!Voting.CanViewVoting(game, user, ownRole, voting))
                            continue;
                        voting.WriteToJson(writer, game, user);
                    }
                    writer.WriteEndArray(); //voting
                    writer.WriteEndObject(); // phase
                }

                var winner = game.Winner;
                writer.WriteStartObject("participants");
                foreach (var participant in game.Participants.ToArray())
                {
                    if (participant.Value == null)
                        writer.WriteNull(participant.Key.ToString());
                    else
                    {
                        var seenRole = Role.GetSeenRole(game, winner?.round, user, 
                            participant.Key, participant.Value);

                        writer.WriteStartObject(participant.Key.ToString());
                        writer.WriteStartArray("tags");
                        foreach (var tag in Role.GetSeenTags(game, user, ownRole, participant.Value))
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
                writer.WriteBoolean("all-can-see-role-of-dead", game.AllCanSeeRoleOfDead);
                writer.WriteBoolean("autostart-votings", game.AutostartVotings);
                writer.WriteBoolean("autofinish-votings", game.AutoFinishVotings);
                writer.WriteBoolean("voting-timeout", game.UseVotingTimeouts);
                writer.WriteBoolean("autofinish-rounds", game.AutoFinishRounds);

                writer.WriteStartArray("theme");
                writer.WriteStringValue(game.Theme?.GetType().FullName ?? typeof(DefaultTheme).FullName);
                writer.WriteStringValue(game.Theme?.LanguageTheme ?? "default");
                writer.WriteEndArray();

                writer.WriteEndObject();
                writer.WriteString("user", user.Id.ToString());

                var userConfig = Theme.User!.Query()
                    .Where(x => x.Id == user.Id)
                    .FirstOrDefault();
                if (userConfig != null)
                {
                    writer.WriteStartObject("user-config");
                    writer.WriteString("theme", userConfig.ThemeColor ?? "#ffffff");
                    writer.WriteString("background", userConfig.BackgroundImage ?? "");
                    writer.WriteString("language", string.IsNullOrEmpty(userConfig.Language) ? "de" : userConfig.Language);
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
                    if (game.Phase != null)
                        return "cannot change settings of running game";
                    try
                    {
                        var newLeader = game.Leader;
                        Dictionary<Role, int>? newConfig = null;
                        var leaderIsPlayer = game.LeaderIsPlayer;
                        var deadCanSeeRoles = game.DeadCanSeeAllRoles;
                        var allCanSeeRoleOfDead = game.AllCanSeeRoleOfDead;
                        var autostartVotings = game.AutostartVotings;
                        var autoFinishVotings = game.AutoFinishVotings;
                        var votingTimeout = game.UseVotingTimeouts;
                        var autoFinishRounds = game.AutoFinishRounds;
                        var theme = game.Theme;
                        var themeLang = game.Theme?.LanguageTheme;

                        if (post.Parameter.TryGetValue("leader", out string? value))
                        {
                            newLeader = new ObjectId(value);
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
                            foreach (var (role, newCount) in newConfig.ToArray())
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

                        if (post.Parameter.TryGetValue("all-can-see-role-of-dead", out value))
                        {
                            allCanSeeRoleOfDead = bool.Parse(value);
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

                        if (post.Parameter.TryGetValue("theme-impl", out value))
                        {
                            static Type? LoadType(string name)
                            {
                                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                                {
                                    var type = assembly.GetType(name, false, true);
                                    if (type != null)
                                        return type;
                                }
                                return null;
                            }
                            var type = LoadType(value);
                            if (type == null)
                                return $"theme implementation {value} not found";
                            if (!type.IsSubclassOf(typeof(Theme)))
                                return $"theme implementation {value} is not a valid theme";
                            if (type.FullName != theme?.GetType().FullName)
                            {
                                try
                                {
                                    theme = (Theme)Activator.CreateInstance(type, game)!;
                                }
                                catch (Exception e)
                                {
                                    return $"cannot instantiate theme implementation: {e}";
                                }
                            }
                        }

                        if (post.Parameter.TryGetValue("theme-lang", out value))
                        {
                            themeLang = string.IsNullOrWhiteSpace(value) ? "default" : value;
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
                        game.AllCanSeeRoleOfDead = allCanSeeRoleOfDead;
                        game.AutostartVotings = autostartVotings;
                        game.AutoFinishVotings = autoFinishVotings;
                        game.UseVotingTimeouts = votingTimeout;
                        game.AutoFinishRounds = autoFinishRounds;
                        if (game.Theme != theme)
                        {
                            foreach (var key in game.Participants.Keys)
                                game.Participants[key] = null;
                            game.RoleConfiguration.Clear();
                        }
                        game.Theme = theme;
                        if (game.Theme != null && themeLang != null)
                        {
                            game.Theme.LanguageTheme = themeLang;
                        }
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }
                    game.SendEvent(new Events.SetGameConfig(typeof(DefaultTheme)));
                    return null;
                }

                if (user.Id == game.Leader)
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
                        .Where(x => x.Id == user.Id)
                        .FirstOrDefault();
                    
                    if (userConfig == null)
                        return "user config not found";

                    try
                    {
                        var newTheme = userConfig.ThemeColor;
                        var newBackground = userConfig.BackgroundImage;
                        var newLanguage = userConfig.Language;

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

                        if (post.Parameter.TryGetValue("language", out value))
                        {
                            var check = new Regex("^[\\w\\-]{0,10}$");
                            if (!check.IsMatch(value))
                                return "invalid language";
                            newLanguage = value;
                        }

                        // input is valid use the new data
                        userConfig.ThemeColor = newTheme;
                        userConfig.BackgroundImage = newBackground;
                        userConfig.Language = newLanguage;
                        Theme.User.Update(userConfig);
                    }
                    catch (Exception e)
                    {
                        return e.ToString();
                    }
                    game.SendEvent(new Events.SetUserConfig(user));
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
                if (user.Id != game.Leader)
                    return "you are not the leader of the group";

                if (game.Phase != null)
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
                if (user.Id != game.Leader)
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

                if (!game.Participants.TryGetValue(user.Id, out Role? ownRole))
                    ownRole = null;
                if (ownRole == null || !voting.CanVote(ownRole))
                    return "you are not allowed to vote";

                if (!voting.Started)
                    return "voting is not started";
                
                return voting.Vote(game, user.Id, id);
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

                if (!game.Participants.TryGetValue(user.Id, out Role? ownRole))
                    ownRole = null;
                if ((user.Id != game.Leader || game.LeaderIsPlayer) 
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
                if (user.Id != game.Leader)
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
                if (user.Id != game.Leader)
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
                if (user.Id != game.Leader)
                    return "you are not the leader of the group";

                if (game.Phase == null)
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

        public static async Task<HttpDataSource> KickUserAsync(string token, ObjectId user)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token, ObjectId userId)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                if (user.Id != game.Leader)
                    return "you are not the leader of the group";

                if (!game.Participants.ContainsKey(userId))
                    return "player is not a participant";

                game.Participants!.Remove(userId, out _);
                game.UserCache.Remove(userId, out _);
                game.SendEvent(new Events.RemoveParticipant(userId));

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

        public static async Task<HttpDataSource> MessageAsync(string token, string? phase, UrlEncodedData message)
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            static string? Do(string token, string? phase, string message)
            {
                var result = GameController.Current.GetFromToken(token);
                if (result == null)
                    return "token not found";

                var (game, user) = result.Value;
                var currentPhase = game.Phase?.Current;
                var current = currentPhase?.LanguageId;
                var role = game.TryGetRole(user.Id);
                var allowed = (user.Id == game.Leader && !game.LeaderIsPlayer) ||
                    currentPhase == null ||
                    (current == phase && role != null && currentPhase.CanMessage(game, role));
                game.SendEvent(new Events.ChatEvent(user.Id, phase, message, allowed));

                return null;
            }
            if (message.Parameter.TryGetValue("message", out string? value))
                writer.WriteString("error", Do(token, phase, value));
            else writer.WriteString("error", "no message set");

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
