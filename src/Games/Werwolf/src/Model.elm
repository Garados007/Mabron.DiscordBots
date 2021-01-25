module Model exposing
    ( Model
    , Modal (..)
    , applyResponse
    , init
    )

import Data
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
    , langs: Dict Language.ThemeKey Language
    , theme: Maybe Language.ThemeKey
    }

type Modal
    = NoModal
    | SettingsModal Views.ViewThemeEditor.Model
    | WinnerModal Data.Game (List String)

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
    , langs = Dict.empty
    , theme = Nothing
    }

applyResponse : NetworkResponse -> Model -> (Model, Maybe Network.NetworkRequest)
applyResponse response model =
    case response of
        RespRoles roles ->
            Tuple.pair
                { model
                | roles = Just roles
                }
                Nothing
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
                Nothing
        RespError error ->
            Tuple.pair
                { model
                | errors = 
                    if List.member error model.errors
                    then model.errors
                    else error :: model.errors
                }
                Nothing
        RespNoError -> (model, Nothing)
        RespLangInfo info ->
            Tuple.pair
                { model
                | langInfo = info
                , theme = Language.firstTheme info
                }
            <| Maybe.map Network.GetLang
            <| Language.firstTheme info
        RespLang key info ->
            Tuple.pair
                { model
                | langs = Dict.insert key info model.langs
                }
                Nothing
