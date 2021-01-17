using LiteDB;
using Mabron.DiscordBots.Games.Werwolf.Themes.Default;
using MaxLib.WebServer;
using MaxLib.WebServer.Api.Rest;
using MaxLib.WebServer.Post;
using MaxLib.WebServer.Services;
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
            searcher.Add(new HttpDocumentFinder.Rule("/content/", "content/", false, true));
            server.AddWebService(searcher);
            server.AddWebService(new HttpDirectoryMapper(false));
            server.AddWebService(new DisallowRootAccess());
            server.AddWebService(new GameService());

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
        }

        private static async Task<HttpDataSource> RolesAsync()
        {
            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();

            foreach (var template in new DefaultTheme().GetRoleTemplates())
            {
                writer.WriteStartObject(template.GetType().Name);
                writer.WriteString("name", template.Name);
                writer.WriteString("description", template.Description);
                writer.WriteEndObject();
            }

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
                if (!game.Participants.TryGetValue(user.DiscordId, out Role? ownRole))
                    ownRole = null;

                static bool CanViewVoting(GameRoom game, GameUser user, Role? ownRole, Voting voting)
                {
                    if (game.Leader == user.DiscordId)
                        return true;
                    if (ownRole == null)
                        return false;
                    return voting.CanView(ownRole);
                }

                writer.WriteStartObject("game");

                writer.WriteString("leader", game.Leader.ToString());

                writer.WriteBoolean("running", game.IsRunning);

                if (game.Phase == null)
                    writer.WriteNull("phase");
                else
                {
                    writer.WriteStartObject("phase"); // phase
                    writer.WriteString("name", game.Phase.Name);
                    writer.WriteStartArray("voting"); // voting
                    foreach (var voting in game.Phase.Votings)
                    {
                        if (!CanViewVoting(game, user, ownRole, voting))
                            continue;
                        writer.WriteStartObject(); // {}
                        writer.WriteString("id", voting.Id.ToString());
                        writer.WriteString("name", voting.Name);
                        writer.WriteBoolean("started", voting.Started);
                        writer.WriteBoolean("can-vote", ownRole != null && voting.CanVote(ownRole));
                        writer.WriteNumber("max-voter", voting.GetVoter(game).Count());
                        writer.WriteStartObject("options"); // options
                        foreach (var (id, option) in voting.Options)
                        {
                            writer.WriteStartObject(id.ToString()); // id
                            writer.WriteString("name", option.Name);
                            writer.WriteStartArray("user");
                            foreach (var vuser in option.Users)
                                writer.WriteStringValue(vuser.ToString());
                            writer.WriteEndArray();
                            writer.WriteEndObject(); // id
                        }
                        writer.WriteEndObject(); // options
                        writer.WriteEndObject(); // {}
                    }
                    writer.WriteEndArray(); //voting
                    writer.WriteEndObject(); // phase
                }

                writer.WriteStartObject("participants");
                foreach (var participant in game.Participants.ToArray())
                {
                    if (participant.Value == null)
                        writer.WriteNull(participant.Key.ToString());
                    else
                    {
                        var seenRole = game.Leader == user.DiscordId || participant.Key == user.DiscordId ||
                                (ownRole != null && game.DeadCanSeeAllRoles && !ownRole.IsAlive)?
                            participant.Value :
                            ownRole != null ?
                            participant.Value.ViewRole(ownRole) :
                            null;
                        var loved =
                            (game.Leader == user.DiscordId ||
                                participant.Key == user.DiscordId ||
                                (ownRole != null && game.DeadCanSeeAllRoles && !ownRole.IsAlive)
                            ) ? participant.Value is BaseRole participantRole && participantRole.IsLoved :
                            (ownRole != null && participant.Value.ViewLoved(ownRole));

                        writer.WriteStartObject(participant.Key.ToString());
                        writer.WriteBoolean("alive", participant.Value.IsAlive);
                        writer.WriteBoolean("major", participant.Value.IsMajor);
                        writer.WriteBoolean("loved", loved);
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
                    writer.WriteEndObject();
                }
                writer.WriteEndObject();

                writer.WriteStartObject("config");
                foreach (var (role, amount) in game.RoleConfiguration.ToArray())
                {
                    writer.WriteNumber(role.GetType().Name, amount);
                }
                writer.WriteEndObject();

                writer.WriteBoolean("dead-can-see-all-roles", game.DeadCanSeeAllRoles);
                writer.WriteBoolean("autostart-votings", game.AutostartVotings);
                writer.WriteBoolean("autofinish-votings", game.AutoFinishVotings);

                writer.WriteEndObject();
                writer.WriteString("user", user.DiscordId.ToString());
            }
            else
            {
                writer.WriteNull("game");
                writer.WriteNull("user");
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
                        var deadCanSeeRoles = game.DeadCanSeeAllRoles;
                        var autostartVotings = game.AutostartVotings;
                        var autoFinishVotings = game.AutoFinishVotings;

                        if (post.Parameter.TryGetValue("leader", out string? value))
                        {
                            newLeader = ulong.Parse(value);
                            if (newLeader != game.Leader && !game.Participants.ContainsKey(newLeader))
                                return "new leader is not a member of the group";
                        }

                        if (post.Parameter.TryGetValue("config", out value))
                        {
                            var roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            newConfig = new Dictionary<Role, int>();
                            var known = (game.Theme?.GetRoleTemplates() ?? Enumerable.Empty<Role>())
                                .ToDictionary(x => x.GetType().Name);
                            foreach (var role in roles)
                                if (known.TryGetValue(role, out Role? entry))
                                {
                                    if (newConfig.ContainsKey(entry))
                                        newConfig[entry]++;
                                    else newConfig.Add(entry, 1);
                                }
                                else return $"unknown role '{role}'";
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

                        // input is valid use the new data
                        if (game.Leader != newLeader)
                        {
                            game.Participants.TryAdd(game.Leader, null);
                            game.Participants!.Remove(newLeader, out _);
                            game.Leader = newLeader;
                            _ = game.Message?.ModifyAsync(x => x.Embed = GameCommand.GetGameEmbed(game));
                        }
                        if (newConfig != null)
                        {
                            game.RoleConfiguration.Clear();
                            foreach (var (k, p) in newConfig)
                                game.RoleConfiguration.TryAdd(k, p);
                        }
                        game.DeadCanSeeAllRoles = deadCanSeeRoles;
                        game.AutostartVotings = autostartVotings;
                        game.AutoFinishVotings = autoFinishVotings;
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

                game.IsRunning = true;
                game.NextPhase();
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

                var voting = game.Phase?.Votings.Where(x => x.Id == vid).FirstOrDefault();

                if (voting == null)
                    return "no voting exists";

                if (voting.Started)
                    return "voting already started";

                voting.Started = true;

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
                var voting = game.Phase?.Votings.Where(x => x.Id == vid).FirstOrDefault();
                if (voting == null)
                    return "no voting exists";

                if (!game.Participants.TryGetValue(user.DiscordId, out Role? ownRole))
                    ownRole = null;
                if (ownRole == null || !voting.CanVote(ownRole))
                    return "you are not allowed to vote";

                if (!voting.Started)
                    return "voting is not started";

                if (voting.Options.Any(x => x.option.Users.Contains(user.DiscordId)))
                    return "already voted";

                var option = voting.Options
                    .Where(x => x.id == id)
                    .Select(x => x.option)
                    .FirstOrDefault();

                if (option == null)
                    return "option not found";

                option.Users.Add(user.DiscordId);

                if (game.AutoFinishVotings && voting.GetVoter(game).Count() == voting.Options.Sum(x => x.option.Users.Count))
                {
                    var vote = voting.GetResult();
                    if (vote != null)
                    {
                        voting.Execute(game, vote.Value);
                        game.Phase!.RemoveVoting(voting);

                        if (new WinCondition().Check(game))
                        {
                            game.StopGame();
                        }
                    }
                    else
                    {
                        game.Phase!.ExecuteMultipleWinner(voting, game);
                    }
                }

                return null;
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

                var voting = game.Phase?.Votings.Where(x => x.Id == vid).FirstOrDefault();
                if (voting == null)
                    return "no voting exists";

                if (!voting.Started)
                    return "voting is not started";

                var vote = voting.GetResult();
                if (vote != null)
                {
                    voting.Execute(game, vote.Value);
                    game.Phase!.RemoveVoting(voting);

                    if (new WinCondition().Check(game))
                    {
                        game.StopGame();
                    }
                }
                else
                {
                    game.Phase!.ExecuteMultipleWinner(voting, game);
                }

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

                game.StopGame();

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
