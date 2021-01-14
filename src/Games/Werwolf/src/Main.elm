module Main exposing (..)

import Data
import Model exposing (Model)
import Network exposing (NetworkResponse)

import Browser
import Html exposing (Html)

type Msg
    = Response NetworkResponse
    | Noop

main : Program () Model Msg
main = Browser.application
    { init = \() url key ->
        ( Model.init "" key
        , Cmd.none
        )
    , view = \model ->
        { title = "Werwolf"
        , body = List.singleton <| Html.text <| Debug.toString model
        }
    , update = update
    , subscriptions = subscriptions
    , onUrlRequest = \rurl ->
        case rurl of
            Browser.Internal url ->
                Noop
            Browser.External url ->
                Noop
    , onUrlChange = \url -> Noop
    }

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        Response resp ->
            Tuple.pair
                (Model.applyResponse resp model)
                Cmd.none
        Noop -> (model, Cmd.none)

subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none
