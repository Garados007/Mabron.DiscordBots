port module Main exposing (..)

import Data
import EventData exposing (EventData)
import Model exposing (Model)
import Network exposing (NetworkResponse)
import Language exposing (Language)
import Debug.Extra

import Views.ViewUserList
import Views.ViewRoomEditor
import Views.ViewNoGame
import Views.ViewGamePhase
import Views.ViewErrors
import Views.ViewSettingsBar
import Views.ViewThemeEditor
import Views.ViewModal
import Views.ViewWinners
import Views.ViewPlayerNotification
import Views.ViewRoleInfo

import Browser
import Browser.Navigation
import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Url exposing (Url)
import Url.Parser exposing ((</>))
import Task
import Time exposing (Posix)
import Maybe.Extra
import Dict
import Views.ViewGamePhase
import Maybe.Extra
import Level
import Styles

import Json.Decode as JD
import Json.Encode as JE
import WebSocket
import Maybe
import Level
import Styles
import Styles
import Views.ViewModal
import Model

port receiveSocketMsg : (JD.Value -> msg) -> Sub msg
port sendSocketCommand : JE.Value -> Cmd msg

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
    | WrapSelectModal Model.Modal
    | WrapThemeEditor Views.ViewThemeEditor.Msg
    | CloseModal
    | WsMsg (Result JD.Error WebSocket.WebSocketMsg)

main : Program () Model Msg
main = Browser.application
    { init = \() -> init
    , view = \model ->
        { title = "Werwolf"
        , body = view model
            <| Model.getLanguage model
        }
    , update = \msg model ->
        let
            (newModel, cmd) = update msg model
        in Tuple.pair
            { newModel
            | styles = Styles.pushState
                    newModel.now
                    newModel.styles
                <| Maybe.withDefault 
                    model.bufferedConfig
                <| Maybe.Extra.orElse
                    ( Maybe.andThen .userConfig model.game)
                <| Maybe.Extra.orElse
                    ( Maybe.andThen .game model.game
                        |> Maybe.andThen .phase
                        |> Maybe.map .stage
                        |> Maybe.andThen
                            (\stage ->
                                if stage.backgroundId == ""
                                then Nothing
                                else Just
                                    { theme = stage.theme
                                    , background = stage.backgroundId
                                    , language = ""
                                    }   
                            )
                    )
                <| case model.modal of
                    Model.SettingsModal conf ->
                        Just conf.config
                    _ -> Nothing
            }
            cmd
    , subscriptions = subscriptions
    , onUrlRequest = \rurl ->
        case rurl of
            Browser.Internal url ->
                SetUrl url
            Browser.External _ ->
                Noop
    , onUrlChange = SetUrl
    }

init : Url -> Browser.Navigation.Key -> (Model, Cmd Msg)
init url key =
    let
        token : String
        token = getId url |> Maybe.withDefault ""

    in  ( Model.init token key
        , Cmd.batch
            [ Task.perform identity
                <| Task.succeed Init
            , WebSocket.send sendSocketCommand
                <| WebSocket.Connect
                    { name = "wss"
                    , address =
                        "wss://" ++ url.host ++
                        (Maybe.map
                            ((++) ":" << String.fromInt)
                            url.port_
                            |> Maybe.withDefault ""
                        ) ++
                        "/ws/" ++ token
                        -- "wss://localhost:8000"
                    , protocol = ""
                    }
            ]
        )

view : Model -> Language -> List (Html Msg)
view model lang =
    [ Html.node "link"
        [ HA.attribute "rel" "stylesheet"
        , HA.attribute "property" "stylesheet"
        , HA.attribute "href" "/content/games/werwolf/css/style.css"
        ] []
    , Html.node "link"
        [ HA.attribute "rel" "stylesheet"
        , HA.attribute "property" "stylesheet"
        , HA.attribute "href" "/content/vendor/flag-icon-css/css/flag-icon.min.css"
        ] []
    , Styles.view model.now model.styles
    , tryViewGamePhase model lang
        |> Maybe.Extra.orElseLazy
            (\() -> tryViewGameFrame model lang)
        |> Maybe.Extra.orElseLazy
            (\() -> Just
                <| Views.ViewNoGame.view lang
                <| model.game == Nothing || model.roles == Nothing
            )
        |> Maybe.withDefault (text "")
    , case model.modal of
        Model.NoModal -> text ""
        Model.SettingsModal conf ->
            Views.ViewModal.viewExtracted CloseModal WrapThemeEditor
                ( Language.getTextOrPath lang
                    [ "modals", "theme-editor", "title" ]
                )
                <| List.singleton
                <| Views.ViewThemeEditor.view
                    lang
                    model.langInfo.icons
                    conf
        Model.WinnerModal game list ->
            Html.map (always CloseModal)
                <| Views.ViewModal.viewOnlyClose 
                    ( Language.getTextOrPath lang
                        [ "modals", "winner", "title" ]
                    )
                <| List.singleton
                <| Views.ViewWinners.view
                    lang
                    model.now model.levels
                    game list
        Model.PlayerNotification notification ->
            Html.map (always CloseModal)
                <| Views.ViewModal.viewOnlyClose
                    ( Language.getTextOrPath lang
                        [ "modals", "player-notification", "title" ]
                    )
                <| List.map
                    (\(nid,player) ->
                        Views.ViewPlayerNotification.view
                            lang
                            (Maybe.andThen .game model.game)
                            nid
                            player
                    )
                <| Dict.toList notification
        Model.RoleInfo roleKey ->
            Html.map (always CloseModal)
                <| Views.ViewModal.viewOnlyClose
                    ( Language.getTextOrPath lang
                        [ "theme", "roles", roleKey ]
                    )
                <| List.singleton
                <| Views.ViewRoleInfo.view lang roleKey
    , Views.ViewErrors.view model.errors
        |> Html.map WrapError
    -- , Debug.Extra.viewModel model
    ]

viewEvents : List (Bool, String) -> Html msg
viewEvents events =
    div [ class "event-list" ]
        <| List.map
            (\(used, content) ->
                div [ HA.classList
                        [ ("event-item", True)
                        , ("used", used)
                        ]
                    ]
                    [ text content ]
            )
            events

tryViewGameFrame : Model -> Language -> Maybe (Html Msg)
tryViewGameFrame model lang =
    Maybe.Extra.andThen2
        (\result roles ->
            Maybe.map2
                (viewGameFrame model lang roles)
                result.game
                result.user
        )
        model.game
        model.roles

viewGameFrame : Model
    -> Language
    -> Data.RoleTemplates
    -> Data.Game
    -> String
    -> Html Msg
viewGameFrame model lang roles game user =
    div [ class "frame-game-outer" ]
        [ div [ class "frame-game-left" ]
            [ Html.map WrapUser
                <| Views.ViewUserList.view
                    lang
                    model.now model.levels
                    model.token game user
            ]
        , div [ class "frame-game-body" ]
            [ Html.map WrapSelectModal
                <| Views.ViewSettingsBar.view model
            , Html.map WrapEditor
                <| Views.ViewRoomEditor.view
                    lang
                    roles
                    model.theme
                    game
                    (user == game.leader)
                    model.editor
            ]
        , viewEvents model.events
        ]

tryViewGamePhase : Model -> Language -> Maybe (Html Msg)
tryViewGamePhase model lang =
    Maybe.andThen
        (\result ->
            Maybe.Extra.andThen2
                (\game user ->
                    Maybe.map
                        (\phase -> 
                            viewGamePhase model lang game user phase
                        )
                        game.phase
                )
                result.game
                result.user
        )
        model.game

viewGamePhase : Model
    -> Language
    -> Data.Game
    -> String
    -> Data.GamePhase
    -> Html Msg
viewGamePhase model lang game user phase =
    div [ class "frame-game-outer" ]
        [ div [ class "frame-game-left" ]
            [ Html.map WrapUser
                <| Views.ViewUserList.view
                    lang
                    model.now model.levels
                    model.token game user
            ]
        , div [ class "frame-game-body", class "top" ]
            [ Html.map WrapSelectModal
                <| Views.ViewSettingsBar.view model
            , Html.map WrapPhase
                <| Views.ViewGamePhase.view
                    lang
                    model.now
                    model.token
                    game
                    phase
                    (user == game.leader)
                    user
            ]
        , viewEvents model.events
        ]

update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
    case msg of
        Response resp ->
            Tuple.mapSecond
                (List.map
                    (Network.executeRequest
                        >> Cmd.map Response
                    )
                    >> Cmd.batch
                )
            <| Model.applyResponse resp model
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
                    , Network.executeRequest
                        <| Network.GetLangInfo
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
        WrapEditor (Views.ViewRoomEditor.ShowRoleInfo roleKey) ->
            Tuple.pair
                { model | modal = Model.RoleInfo roleKey }
                Cmd.none
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
        WrapSelectModal modal ->
            Tuple.pair { model | modal = modal } Cmd.none
        WrapThemeEditor sub ->
            case model.modal of
                Model.SettingsModal editor ->
                    let 
                        (newEditor, newEvent) = Views.ViewThemeEditor.update sub editor
                        sendEvents = List.filterMap
                            (\event ->
                                case event of
                                    Views.ViewThemeEditor.Send req -> Just req
                            )
                            newEvent
                    in Tuple.pair
                        { model | modal = Model.SettingsModal newEditor }
                        <| Cmd.batch
                        <| List.map
                            ( Cmd.map Response
                                << Network.executeRequest
                                << Network.PostUserConfig model.token
                            )
                            sendEvents
                _ -> (model, Cmd.none)
        CloseModal ->
            ({ model | modal = Model.NoModal }, Cmd.none)
        WsMsg (Ok (WebSocket.Data d)) ->
            let

                decodedData : Result String EventData
                decodedData = d.data
                    |> JD.decodeString EventData.decodeEventData
                    |> Result.mapError JD.errorToString

                formatedRaw : String
                formatedRaw = d.data
                    |> JD.decodeString JD.value
                    |> Result.toMaybe
                    |> Maybe.map (JE.encode 2)
                    |> Maybe.withDefault d.data
                    
            in case decodedData of
                Ok data ->
                    Tuple.mapSecond
                        (List.map
                            (Network.executeRequest
                                >> Cmd.map Response
                            )
                            >> Cmd.batch
                        )
                    <| Model.applyEventData data
                        { model
                        | events = (True, formatedRaw) :: model.events
                        }
                Err err -> Tuple.pair 
                    { model
                    | events = (False, formatedRaw) :: model.events
                    , errors = (++) model.errors
                        <| List.singleton 
                        <| "Socket error: " ++ err
                    }
                    Cmd.none
        WsMsg (Ok _) -> (model, Cmd.none)
        WsMsg (Err err) ->
            Tuple.pair
                { model
                | errors = (++) model.errors
                    <| List.singleton
                    <| Debug.toString err
                }
                Cmd.none

subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.batch
        [ Time.every
            (   if (Dict.values model.levels
                        |> List.any Level.isAnimating
                    ) 
                    || Styles.isAnimating model.now model.styles
                then 50
                else 1000
            )
            SetTime  
        -- , Time.every 1000 SetTime
        -- [ Sub.none
        -- , if model.game 
        --         |> Maybe.map 
        --             (\result -> result.game /= Nothing &&
        --                 result.user /= Nothing
        --             )
        --         |> Maybe.withDefault True
        --     then Time.every 3000 (always FetchData)
        --     else Time.every 20000 (always FetchData)
        , Time.every 60000 (always FetchData)
        , receiveSocketMsg
            <| WebSocket.receive WsMsg
        ]

getId : Url -> Maybe String
getId =
    Url.Parser.parse
        <| Url.Parser.s "game"
        </> Url.Parser.string
