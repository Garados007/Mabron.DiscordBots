module Main exposing (..)

import Data
import Model exposing (Model)
import Network exposing (NetworkResponse)

import Views.ViewUserList
import Views.ViewRoomEditor
import Views.ViewNoGame
import Views.ViewGamePhase
import Views.ViewErrors

import Browser
import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Url exposing (Url)
import Url.Parser exposing ((</>))
import Task
import Debug.Extra
import Time exposing (Posix)
import Maybe.Extra
import Dict exposing (Dict)
import Views.ViewGamePhase
import Maybe.Extra

type Msg
    = Response NetworkResponse
    | SetUrl Url
    | SetTime Posix
    | FetchData
    | Noop
    | Init
    | WrapUser Views.ViewUserList.Msg
    | WrapEditor Views.ViewRoomEditor.Msg
    | WrapPhase Views.ViewGamePhase.Msg
    | WrapError Int

main : Program () Model Msg
main = Browser.application
    { init = \() url key ->
        ( Model.init
            (getId url |> Maybe.withDefault "")
            key
        , Task.perform identity
            <| Task.succeed Init
        )
    , view = \model ->
        { title = "Werwolf"
        , body = 
            [ Html.node "link"
                [ HA.attribute "rel" "stylesheet"
                , HA.attribute "property" "stylesheet"
                , HA.attribute "href" "/content/games/werwolf/css/style.css"
                ] []
            , tryViewGamePhase model
                |> Maybe.Extra.orElseLazy
                    (\() -> tryViewGameFrame model)
                |> Maybe.Extra.orElseLazy
                    (\() -> Just
                        <| Views.ViewNoGame.view
                        <| model.game == Nothing || model.roles == Nothing
                    )
                |> Maybe.withDefault (text "")
            , Views.ViewErrors.view model.errors
                |> Html.map WrapError
            , Debug.Extra.viewModel model
            ]
        }
    , update = update
    , subscriptions = subscriptions
    , onUrlRequest = \rurl ->
        case rurl of
            Browser.Internal url ->
                SetUrl url
            Browser.External url ->
                Noop
    , onUrlChange = SetUrl
    }

tryViewGameFrame : Model -> Maybe (Html Msg)
tryViewGameFrame model =
    Maybe.Extra.andThen2
        (\result roles ->
            Maybe.map2
                (viewGameFrame model roles)
                result.game
                result.user
        )
        model.game
        model.roles

viewGameFrame : Model
    -> Dict String Data.RoleTemplate
    -> Data.Game
    -> String
    -> Html Msg
viewGameFrame model roles game user =
    div [ class "frame-game-outer" ]
        [ div [ class "frame-game-left" ]
            [ Html.map WrapUser
                <| Views.ViewUserList.view model.token game user roles
            ]
        , div [ class "frame-game-body" ]
            [ Html.map WrapEditor
                <| Views.ViewRoomEditor.view
                    roles
                    game
                    (user == game.leader)
                    model.editor
            ]
        ]

tryViewGamePhase : Model -> Maybe (Html Msg)
tryViewGamePhase model =
    Maybe.Extra.andThen2
        (\result roles ->
            Maybe.Extra.andThen2
                (\game user ->
                    Maybe.map
                        (\phase -> 
                            viewGamePhase model.token roles game user phase
                        )
                        game.phase
                )
                result.game
                result.user
        )
        model.game
        model.roles

viewGamePhase : String
    -> Dict String Data.RoleTemplate
    -> Data.Game
    -> String
    -> Data.GamePhase
    -> Html Msg
viewGamePhase token roles game user phase =
    div [ class "frame-game-outer" ]
        [ div [ class "frame-game-left" ]
            [ Html.map WrapUser
                <| Views.ViewUserList.view token game user roles
            ]
        , div [ class "frame-game-body", class "top" ]
            [ Html.map WrapPhase
                <| Views.ViewGamePhase.view
                    token
                    game
                    phase
                    (user == game.leader)
                    user
            ]
        ]

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        Response resp ->
            Tuple.pair
                (Model.applyResponse resp model)
                Cmd.none
        SetUrl url ->
            Tuple.pair
                { model
                | token = getId url
                    |> Maybe.withDefault model.token
                }
                <| Task.perform identity
                <| Task.succeed Init
        Noop -> (model, Cmd.none)
        Init ->
            Tuple.pair model
                <| Cmd.map Response
                <| Cmd.batch
                    [ Network.executeRequest
                        <| Network.GetGame model.token
                    , Network.executeRequest
                        <| Network.GetRoles
                    ]
        SetTime now ->
            Tuple.pair
                { model | now = now }
                Cmd.none
        FetchData ->
            Tuple.pair model
                <| Cmd.map Response
                <| Network.executeRequest
                <| Network.GetGame model.token
        WrapUser (Views.ViewUserList.Noop) ->
            Tuple.pair model Cmd.none
        WrapUser (Views.ViewUserList.Send req) ->
            Tuple.pair model
                <| Cmd.map Response
                <| Network.executeRequest req
        WrapEditor (Views.ViewRoomEditor.SetBuffer buffer req) ->
            Tuple.pair
                { model | editor = buffer }
                <| Cmd.map Response
                <| Network.executeRequest
                <| Network.PostGameConfig model.token req
        WrapEditor (Views.ViewRoomEditor.SendConf req) ->
            Tuple.pair model
                <| Cmd.map Response
                <| Network.executeRequest
                <| Network.PostGameConfig model.token req
        WrapEditor Views.ViewRoomEditor.StartGame ->
            Tuple.pair model
                <| Cmd.map Response
                <| Network.executeRequest
                <| Network.GetGameStart model.token
        WrapEditor Views.ViewRoomEditor.Noop ->
            Tuple.pair model Cmd.none
        WrapPhase Views.ViewGamePhase.Noop ->
            Tuple.pair model Cmd.none
        WrapPhase (Views.ViewGamePhase.Send req) ->
            Tuple.pair model
                <| Cmd.map Response
                <| Network.executeRequest req
        WrapError index ->
            Tuple.pair
                { model 
                | errors = 
                    List.filterMap
                        (\(ind, entry) ->
                            if ind /= index
                            then Just entry
                            else Nothing
                        )
                    <| List.indexedMap Tuple.pair
                    <| model.errors
                }
                Cmd.none

subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.batch
        -- [ Time.every 1000 SetTime
        [ Sub.none
        , if model.game 
                |> Maybe.map 
                    (\result -> result.game /= Nothing &&
                        result.user /= Nothing
                    )
                |> Maybe.withDefault True
            then Time.every 3000 (always FetchData)
            else Time.every 20000 (always FetchData)
        ]

getId : Url -> Maybe String
getId =
    Url.Parser.parse
        <| Url.Parser.s "game"
        </> Url.Parser.string
