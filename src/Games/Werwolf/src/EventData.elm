module EventData exposing (..)

import Data exposing (..)
import Level exposing (LevelData)
import Json.Decode as JD exposing (Decoder)
import Json.Decode.Pipeline exposing (required)
import Iso8601
import Dict exposing (Dict)
import Time exposing (Posix)

type EventData
    = AddParticipant String GameUser
    | AddVoting GameVoting
    | GameEnd (Maybe (List String))
    | GameStart (Dict String (Maybe GameParticipant))
    | NextPhase (Maybe String)
    | OnLeaderChanged String
    | OnRoleInfoChanged (Maybe String) GameParticipant
    | PlayerNotification String (List String)
    | RemoveParticipant String
    | RemoveVoting String
    | SetGameConfig EventGameConfig
    | SetUserConfig UserConfig
    | SetVotingTimeout String (Maybe Posix)
    | SetVotingVote String String String -- voting option voter

type alias EventGameConfig =
    { config: Dict String Int
    , leaderIsPlayer: Bool
    , deadCanSeeAllRoles: Bool
    , autostartVotings: Bool
    , autofinishVotings: Bool
    , votingTimeout: Bool
    , autofinishRound: Bool
    }

decodeEventData : Decoder EventData
decodeEventData =
    JD.andThen
        (\key ->
            case key of
                "AddParticipant" ->
                    JD.succeed GameUser
                    |> required "name" JD.string
                    |> required "img" JD.string
                    |> required "stats"
                        ( JD.succeed GameUserStats
                            |> required "win-games" JD.int
                            |> required "killed" JD.int
                            |> required "loose-games" JD.int
                            |> required "leader" JD.int
                        )
                    |> required "stats"
                        (JD.succeed LevelData
                            |> required "level" JD.int
                            |> required "current-xp" JD.int
                            |> required "max-xp" JD.int
                        )
                    |> JD.map2 AddParticipant
                        (JD.field "id" JD.string)
                "AddVoting" ->
                    JD.succeed GameVoting
                    |> required "id" JD.string
                    |> required "lang-id" JD.string
                    |> required "started" JD.bool
                    |> required "can-vote" JD.bool
                    |> required "max-voter" JD.int
                    |> required "timeout" 
                        (JD.nullable Iso8601.decoder)
                    |> required "options"
                        (JD.succeed GameVotingOption
                            |> required "name" JD.string
                            |> required "user" (JD.list JD.string)
                            |> JD.dict
                        )
                    |> JD.map AddVoting
                    |> JD.field "voting"
                "GameEnd" ->
                    JD.map GameEnd
                    <| JD.field "winner"
                    <| JD.nullable
                    <| JD.list
                    <| JD.string
                "GameStart" ->
                    JD.succeed GameParticipant
                    |> required "tags" (JD.list JD.string)
                    |> required "role" (JD.nullable JD.string)
                    |> JD.nullable
                    |> JD.dict
                    |> JD.field "participants"
                    |> JD.map GameStart
                "NextPhase" ->
                    JD.map NextPhase
                    <| JD.field "phase"
                    <| JD.nullable
                    <| JD.field "lang-id"
                    <| JD.string
                "OnLeaderChanged" ->
                    JD.succeed OnLeaderChanged
                    |> required "leader" JD.string
                "OnRoleInfoChanged" ->
                    JD.succeed GameParticipant
                    |> required "tags" (JD.list JD.string)
                    |> required "role" (JD.nullable JD.string)
                    |> JD.map2 OnRoleInfoChanged
                        (JD.field "id" <| JD.nullable JD.string)
                "PlayerNotification" ->
                    JD.succeed PlayerNotification
                    |> required "text-id" JD.string
                    |> required "player" (JD.list JD.string)
                "RemoveParticipant" ->
                    JD.succeed RemoveParticipant
                    |> required "id" JD.string
                "RemoveVoting" ->
                    JD.succeed RemoveVoting
                    |> required "id" JD.string
                "SetGameConfig" ->
                    JD.succeed EventGameConfig
                    |> required "config" (JD.dict JD.int)
                    |> required "leader-is-player" JD.bool
                    |> required "dead-can-see-all-roles" JD.bool
                    |> required "autostart-votings" JD.bool
                    |> required "autofinish-votings" JD.bool
                    |> required "voting-timeout" JD.bool
                    |> required "autofinish-rounds" JD.bool
                    |> JD.map SetGameConfig
                "SetUserConfig" ->
                    JD.succeed UserConfig
                    |> required "theme" JD.string
                    |> required "background" JD.string 
                    |> JD.field "user-config"
                    |> JD.map SetUserConfig
                "SetVotingTimeout" ->
                    JD.succeed SetVotingTimeout
                    |> required "id" JD.string
                    |> required "timeout" (JD.nullable Iso8601.decoder)
                "SetVotingVote" ->
                    JD.succeed SetVotingVote
                    |> required "voting" JD.string
                    |> required "option" JD.string 
                    |> required "voter" JD.string
                _ -> JD.fail <| "unknown event " ++ key
        )
    <| JD.field "$type" JD.string