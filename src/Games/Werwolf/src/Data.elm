module Data exposing
    ( Error
    , Game
    , GameParticipant
    , GamePhase
    , GameUser
    , GameUserResult
    , GameVoting
    , GameVotingOption
    , RoleTemplate
    , decodeError
    , decodeGameUserResult
    , decodeRoleTemplates
    )

import Dict exposing (Dict)
import Json.Decode as JD exposing (Decoder)
import Json.Decode.Pipeline exposing (required)
import Json.Decode exposing (succeed)

type alias RoleTemplate =
    { name: String
    , description: String
    }

decodeRoleTemplates : Decoder (Dict String RoleTemplate)
decodeRoleTemplates =
    JD.succeed RoleTemplate
        |> required "name" JD.string
        |> required "description" JD.string
        |> JD.dict

mapIntDicts : Decoder (Dict String a) -> Decoder (Dict Int a)
mapIntDicts =
    JD.andThen
        (\dict ->
            case Dict.foldl
                (\key value result ->
                    case result of
                        Ok d ->
                            case String.toInt key of
                                Just k -> Ok <| Dict.insert k value d
                                Nothing -> Err
                                    <| "Cannot convert key to int: "
                                    ++ key
                        Err e -> Err e
                )
                (Ok Dict.empty)
                dict
            of
                Ok d -> JD.succeed d
                Err e -> JD.fail e
        )

type alias GameUserResult =
    { game: Maybe Game
    , user: Maybe Int
    }

type alias Game =
    { leader: Int
    , running: Bool
    , phase: Maybe GamePhase
    , participants: Dict Int (Maybe GameParticipant)
    , user: Dict Int GameUser
    , config: Dict String Int
    , deadCanSeeAllRoles: Bool
    }

type alias GamePhase =
    { name: String
    , voting: List GameVoting
    }

type alias GameVoting =
    { id: Int
    , name: String
    , started: Bool
    , canVote: Bool
    , options: Dict Int GameVotingOption
    }

type alias GameVotingOption =
    { name: String
    , user: List Int
    }

type alias GameParticipant =
    { alive: Bool
    , major: Bool
    , role: Maybe String
    }

type alias GameUser =
    { name: String
    , img: String
    }

decodeGameUserResult : Decoder GameUserResult
decodeGameUserResult =
    JD.succeed GameUserResult
        |> required "game"
            (JD.succeed Game
                |> required "leader" JD.int
                |> required "running" JD.bool
                |> required "phase"
                    (JD.succeed GamePhase
                        |> required "name" JD.string
                        |> required "voting"
                            (JD.succeed GameVoting
                                |> required "id" JD.int
                                |> required "name" JD.string
                                |> required "started" JD.bool
                                |> required "can-vote" JD.bool
                                |> required "options"
                                    (JD.succeed GameVotingOption
                                        |> required "name" JD.string
                                        |> required "user" (JD.list JD.int)
                                        |> JD.dict
                                        |> mapIntDicts
                                    )
                                |> JD.list
                            )
                        |> JD.nullable
                    )
                |> required "participants"
                    (JD.succeed GameParticipant
                        |> required "alive" JD.bool
                        |> required "major" JD.bool
                        |> required "role" (JD.nullable JD.string)
                        |> JD.nullable
                        |> JD.dict
                        |> mapIntDicts
                    )
                |> required "user"
                    (JD.succeed GameUser
                        |> required "name" JD.string
                        |> required "img" JD.string
                        |> JD.dict
                        |> mapIntDicts
                    )
                |> required "config" (JD.dict JD.int)
                |> required "dead-can-see-all-roles" JD.bool
                |> JD.nullable
            )
        |> required "user" (JD.nullable JD.int)

type alias Error = Maybe String

decodeError : Decoder (Maybe String)
decodeError =
    JD.field "error"
        <| JD.nullable
        <| JD.string
