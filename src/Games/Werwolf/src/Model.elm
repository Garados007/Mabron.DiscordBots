module Model exposing
    ( Model
    , Modal (..)
    , applyResponse
    , applyEventData
    , getLanguage
    , init
    )

import Data
import EventData exposing (EventData)
import Dict exposing (Dict)
import Network exposing (NetworkResponse(..))
import Browser.Navigation exposing (Key)
import Time exposing (Posix)
import Level exposing (Level)
import Language exposing (Language, LanguageInfo)

import Views.ViewThemeEditor

type alias Model =
    { game: Maybe Data.GameUserResult
    , roles: Maybe Data.RoleTemplates
    , errors: List String
    , token: String
    , key: Key
    , now: Posix
    , modal: Modal
    -- local editor
    , editor: Dict String Int
    -- buffer
    , bufferedConfig: Data.UserConfig
    , levels: Dict String Level
    , langInfo: LanguageInfo
    , rootLang: Dict String Language
    , themeLangs: Dict Language.ThemeKey Language
    , theme: Maybe Language.ThemeKey
    , events: List (Bool,String)
    }

type Modal
    = NoModal
    | SettingsModal Views.ViewThemeEditor.Model
    | WinnerModal Data.Game (List String)
    | PlayerNotification String (List String)

init : String -> Key -> Model
init token key =
    { game = Nothing
    , roles = Nothing
    , errors = []
    , token = token
    , key = key
    , now = Time.millisToPosix 0
    , modal = NoModal
    , editor = Dict.empty
    , bufferedConfig =
        { theme = "#ffffff"
        , background = ""
        }
    , levels = Dict.empty
    , langInfo =
        { languages = Dict.empty
        , themes = Dict.empty
        }
    , rootLang = Dict.empty
    , themeLangs = Dict.empty
    , theme = Nothing
    , events = []
    }

getLanguage : Model -> Language
getLanguage model =
    let
        rootLang : Language
        rootLang =
            Language.getLanguage model.rootLang
                <| Maybe.map
                    (\(_, _, lang) -> lang)
                <| model.theme
        
        themeLang : Language
        themeLang =
            Language.getLanguage 
                model.themeLangs
                model.theme
    in Language.alternate themeLang rootLang
    
applyResponse : NetworkResponse -> Model -> (Model, List Network.NetworkRequest)
applyResponse response model =
    case response of
        RespRoles roles ->
            Tuple.pair
                { model
                | roles = Just roles
                }
                []
        RespGame game ->
            Tuple.pair
                { model
                | game = Just game
                , bufferedConfig = game.userConfig
                    |> Maybe.withDefault model.bufferedConfig
                , modal =
                    let
                        get : Maybe Data.GameUserResult -> Maybe (Data.Game, List String)
                        get =
                            Maybe.andThen .game
                            >> Maybe.andThen
                                (\game_ ->
                                    Maybe.map 
                                        (Tuple.pair game_)
                                        game_.winner
                                )
                    in case (get model.game, get <| Just game) of
                        (Nothing, Just (game_, list)) -> WinnerModal game_ list
                        _ -> model.modal
                , levels = 
                    case game.game of
                        Just game_ ->
                            Dict.merge
                            (\_ _ dict -> dict)
                            (\key a b dict ->
                                Dict.insert 
                                    key
                                    (Level.updateData model.now b a)
                                    dict
                            )
                            (\key b dict ->
                                Dict.insert
                                    key
                                    (Level.init model.now b)
                                    dict
                            )
                            model.levels
                            ( game_.user
                                |> Dict.map (\_ -> .level)
                            )
                            Dict.empty
                        Nothing -> Dict.empty
                }
                []
        RespError error ->
            Tuple.pair
                { model
                | errors = 
                    if List.member error model.errors
                    then model.errors
                    else error :: model.errors
                }
                []
        RespNoError -> (model, [])
        RespLangInfo info ->
            Tuple.pair
                { model
                | langInfo = info
                , theme = Language.firstTheme info
                }
            <|  ( case Maybe.map Network.GetLang <| Language.firstTheme info of
                    Just x -> (::) x
                    Nothing -> identity
                )
            <| List.map Network.GetRootLang
            <| Dict.keys info.languages
        RespRootLang lang info ->
            Tuple.pair
                { model
                | rootLang = Dict.insert lang info model.rootLang
                }
                []
        RespLang key info ->
            Tuple.pair
                { model
                | themeLangs = Dict.insert key info model.themeLangs
                }
                []

editGame : Model -> (Data.Game -> Data.Game) -> Maybe Data.GameUserResult
editGame model editFunc =
    Maybe.map
        (\gameResult ->
            { gameResult
            | game = Maybe.map editFunc gameResult.game
            }
        )
        model.game

applyEventData : EventData -> Model -> (Model, List Network.NetworkRequest)
applyEventData event model =
    case event of
        EventData.AddParticipant id user -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | user = Dict.insert id user game.user
                , participants = Dict.insert id Nothing game.participants
                }
            }
            []
        EventData.AddVoting voting -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | phase =
                    Maybe.withDefault 
                        (Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                        ) 
                        game.phase 
                    |> \phase -> Just
                        { phase
                        | voting = 
                            if List.isEmpty
                                <| List.filter
                                    (\v -> v.id == voting.id)
                                <| phase.voting
                            then phase.voting ++ [ voting ]
                            else phase.voting
                        }
                }
            }
            []
        EventData.GameEnd winner -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | winner = winner
                , phase = Nothing
                }
            , modal = case winner of
                Just list -> 
                    Maybe.withDefault model.modal
                    <| Maybe.andThen
                        (Maybe.map
                            (\game -> WinnerModal game list)
                        << .game
                        )
                        model.game
                Nothing -> model.modal
            }
            []
        EventData.GameStart newParticipant -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | phase = game.phase
                    |> Maybe.withDefault
                        (Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                        ) 
                    |> Just
                , participants = newParticipant
                , winner = Nothing
                }
            }
            []
        EventData.NextPhase nextPhase -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | phase = case (nextPhase, game.phase) of
                    (Nothing, _) -> Nothing
                    (Just key, Just oldPhase) -> Just
                        { oldPhase
                        | langId = key
                        }
                    (Just key, Nothing) -> Just
                        <| Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                }
            }
            []
        EventData.OnLeaderChanged newLeader -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | leader = newLeader
                }
            }
            []
        EventData.OnRoleInfoChanged Nothing _ -> (model, [])
        EventData.OnRoleInfoChanged (Just id) role -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | participants = Dict.insert id (Just role) game.participants
                }
            }
            []
        EventData.PlayerNotification nid player -> Tuple.pair
            { model
            | modal = PlayerNotification nid player
            }
            []
        EventData.RemoveParticipant id -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | user = Dict.remove id game.user
                , participants = Dict.remove id game.participants
                }
            }
            []
        EventData.RemoveVoting id -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | phase = 
                    Maybe.withDefault
                        (Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                        ) 
                        game.phase 
                    |> \phase -> Just
                        { phase
                        | voting = List.filter
                            (\v -> v.id /= id)
                            phase.voting
                        }
                }
            }
            []
        EventData.SendStage stage -> Tuple.pair
            { model
            | game = editGame model <| \game ->
                { game
                | phase = case game.phase of
                    Just oldPhase -> Just
                        { oldPhase
                        | stage = stage
                        }
                    Nothing -> Just
                        <| Data.GamePhase "" 
                            stage
                            []
                }
            }
            []
        EventData.SetGameConfig newConfig -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | config = newConfig.config
                , participants =
                    if newConfig.leaderIsPlayer == game.leaderIsPlayer
                    then game.participants
                    else if newConfig.leaderIsPlayer
                    then Dict.insert game.leader Nothing game.participants
                    else Dict.remove game.leader game.participants
                , leaderIsPlayer = newConfig.leaderIsPlayer
                , deadCanSeeAllRoles = newConfig.deadCanSeeAllRoles
                , autostartVotings = newConfig.autostartVotings
                , autofinishVotings = newConfig.autofinishVotings
                , votingTimeout = newConfig.votingTimeout
                , autofinishRound = newConfig.autofinishRound
                }
            }
            []
        EventData.SetUserConfig newConfig -> Tuple.pair
            { model
            | game = Maybe.map
                (\gameResult ->
                    { gameResult
                    | userConfig = Just newConfig
                    }
                )
                model.game
            }
            []
        EventData.SetVotingTimeout vid timeout -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | phase = 
                    Maybe.withDefault
                        (Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                        ) 
                        game.phase 
                    |> \phase -> Just
                        { phase
                        | voting = List.map
                            (\v ->
                                if v.id == vid
                                then { v | timeout = timeout}
                                else v
                            )
                            phase.voting
                        }
                }
            }
            []
        EventData.SetVotingVote vid oid voter -> Tuple.pair 
            { model
            | game = editGame model <| \game ->
                { game
                | phase = 
                    Maybe.withDefault
                        (Data.GamePhase "" 
                            (Data.GameStage "" "")
                            []
                        ) 
                        game.phase 
                    |> \phase -> Just
                        { phase
                        | voting = List.map
                            (\v ->
                                if v.id == vid
                                then
                                    { v
                                    | options = Dict.map
                                        (\key value ->
                                            if key == oid
                                            then 
                                                { value
                                                | user = value.user ++ [ voter ]
                                                }
                                            else value
                                        )
                                        v.options
                                    }
                                else v
                            )
                            phase.voting
                        }
                }
            }
            []