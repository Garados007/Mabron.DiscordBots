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
    , UserConfig
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

type alias GameUserResult =
    { game: Maybe Game
    , user: Maybe String
    , userConfig: Maybe UserConfig
    }

type alias Game =
    { leader: String
    , running: Bool
    , phase: Maybe GamePhase
    , participants: Dict String (Maybe GameParticipant)
    , user: Dict String GameUser
    , config: Dict String Int
    , deadCanSeeAllRoles: Bool
    , autostartVotings: Bool
    , autofinishVotings: Bool
    }

type alias GamePhase =
    { name: String
    , voting: List GameVoting
    }

type alias GameVoting =
    { id: String
    , name: String
    , started: Bool
    , canVote: Bool
    , maxVoter: Int
    , options: Dict String GameVotingOption
    }

type alias GameVotingOption =
    { name: String
    , user: List String
    }

type alias GameParticipant =
    { alive: Bool
    , major: Bool
    , loved: Bool
    , role: Maybe String
    }

type alias GameUser =
    { name: String
    , img: String
    }

type alias UserConfig =
    { theme: String
    , background: String
    }

decodeGameUserResult : Decoder GameUserResult
decodeGameUserResult =
    JD.succeed GameUserResult
        |> required "game"
            (JD.succeed Game
                |> required "leader" JD.string
                |> required "running" JD.bool
                |> required "phase"
                    (JD.succeed GamePhase
                        |> required "name" JD.string
                        |> required "voting"
                            (JD.succeed GameVoting
                                |> required "id" JD.string
                                |> required "name" JD.string
                                |> required "started" JD.bool
                                |> required "can-vote" JD.bool
                                |> required "max-voter" JD.int
                                |> required "options"
                                    (JD.succeed GameVotingOption
                                        |> required "name" JD.string
                                        |> required "user" (JD.list JD.string)
                                        |> JD.dict
                                    )
                                |> JD.list
                            )
                        |> JD.nullable
                    )
                |> required "participants"
                    (JD.succeed GameParticipant
                        |> required "alive" JD.bool
                        |> required "major" JD.bool
                        |> required "loved" JD.bool
                        |> required "role" (JD.nullable JD.string)
                        |> JD.nullable
                        |> JD.dict
                    )
                |> required "user"
                    (JD.succeed GameUser
                        |> required "name" JD.string
                        |> required "img" JD.string
                        |> JD.dict
                    )
                |> required "config" (JD.dict JD.int)
                |> required "dead-can-see-all-roles" JD.bool
                |> required "autostart-votings" JD.bool
                |> required "autofinish-votings" JD.bool
                |> JD.nullable
            )
        |> required "user" (JD.nullable JD.string)
        |> required "user-config"
            ( JD.succeed UserConfig
                |> required "theme" JD.string
                |> required "background" JD.string
                |> JD.nullable
            )

type alias Error = Maybe String

decodeError : Decoder (Maybe String)
decodeError =
    JD.field "error"
        <| JD.nullable
        <| JD.string
