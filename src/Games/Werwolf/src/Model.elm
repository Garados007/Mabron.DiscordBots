module Model exposing (..)

import Data
import Dict exposing (Dict)
import Network exposing (NetworkResponse(..))

type alias Model =
    { game: Maybe Data.GameUserResult
    , roles: Maybe (Dict String Data.RoleTemplate)
    , errors: List String
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
            }
        RespError error ->
            { model
            | errors = error :: model.errors
            }
        RespNoError -> model
