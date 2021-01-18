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

import Views.ViewThemeEditor

type alias Model =
    { game: Maybe Data.GameUserResult
    , roles: Maybe (Dict String Data.RoleTemplate)
    , errors: List String
    , token: String
    , key: Key
    , now: Posix
    , modal: Modal
    -- local editor
    , editor: Dict String Int
    -- buffer
    , bufferedConfig: Data.UserConfig
    }

type Modal
    = NoModal
    | SettingsModal Views.ViewThemeEditor.Model

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
    }

applyResponse : NetworkResponse -> Model -> Model
applyResponse response model =
    case response of
        RespRoles roles ->
            { model
            | roles = Just roles
            }
        RespGame game ->
            { model
            | game = Just game
            , bufferedConfig = game.userConfig
                |> Maybe.withDefault model.bufferedConfig
            }
        RespError error ->
            { model
            | errors = 
                if List.member error model.errors
                then model.errors
                else error :: model.errors
            }
        RespNoError -> model
