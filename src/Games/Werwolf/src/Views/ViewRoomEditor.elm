module Views.ViewRoomEditor exposing (..)

import Data
import Model exposing (Model)
import Network exposing (NetworkRequest(..), EditGameConfig, editGameConfig)

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Html.Events as HE
import Dict exposing (Dict)
import Maybe.Extra

type Msg
    = SetBuffer (Dict String Int) EditGameConfig
    | SendConf EditGameConfig
    | StartGame
    | Noop

view : Dict String Data.RoleTemplate -> Data.Game -> Bool 
    -> Dict String Int -> Html Msg
view roles game editable buffer =
    let

        handleInput : String -> String -> Msg
        handleInput id value =
            case String.toInt value of
                Nothing -> Noop
                Just new ->
                    SetBuffer
                        (Dict.insert id new buffer)
                        { editGameConfig
                        | newConfig = Just
                            <| Dict.insert id new buffer
                        }
        
        viewSingleRole : String -> Html Msg
        viewSingleRole id =
            div [ class "editor-role-box" ]
                [ div [ class "editor-role-name" ]
                    <| List.singleton
                    <| text
                    <| Maybe.withDefault id
                    <| Maybe.map .name
                    <| Dict.get id roles
                , if editable
                    then Html.input
                        [ HA.type_ "number"
                        , HA.min "0"
                        , HA.step "1"
                        , HA.max "500"
                        , HA.value
                            <| String.fromInt
                            <| Maybe.withDefault 0
                            <| Maybe.Extra.orLazy
                                (Dict.get id buffer)
                                (\() -> Dict.get id game.config)
                        , HE.onInput <| handleInput id
                        ] []
                    else text ""
                , div 
                    [ class "editor-role-count" 
                    , HA.title <| "Der aktuelle Anzahl auf dem Spielserver. Unter "
                        ++ "bestimmten Umständen dauert es kurz bis sich diese Zahl "
                        ++ "aktualisiert."
                    ]
                    <| List.singleton
                    <| text
                    <| String.concat
                        [ "("
                        , String.fromInt
                            <| Maybe.withDefault 0
                            <| Dict.get id game.config
                        , ")"
                        ]
                ]
    
        maxPlayer = (+) 1 <| Dict.size game.participants
        maxRoles = (+) 1 <| List.sum <| Dict.values 
            <| Dict.union buffer game.config

        viewRoleBar : Html msg
        viewRoleBar = 
            div [ HA.classList
                    [ ("editor-bar-box", True)
                    , ("overflow", maxRoles > maxPlayer)
                    ]
                ]
                [ div [ class "editor-bar-fill-outer" ]
                    <| List.singleton
                    <| div
                        [ class "editor-bar-fill-inner" 
                        , HA.style "width" <|
                            (String.fromFloat
                                <| 100 *
                                    (toFloat <| min maxPlayer maxRoles) /
                                    (toFloat <| max maxPlayer maxRoles)
                            ) ++ "%"
                        ] []
                , div
                    [ class "editor-bar-player-box" 
                    , HA.style "left" <|
                        (String.fromFloat
                            <| min 100
                            <| 100 * (toFloat maxPlayer) / (toFloat maxRoles)
                        ) ++ "%"
                    ]
                    [ div [ class "line" ] []
                    , div [ class "number" ] [ text <| String.fromInt <| maxPlayer - 1 ]
                    ]
                , div
                    [ class "editor-bar-roles-box" 
                    , HA.style "left" <|
                        (String.fromFloat
                            <| min 100
                            <| 100 * (toFloat maxRoles) / (toFloat maxPlayer)
                        ) ++ "%"
                    ]
                    [ div [ class "line" ] []
                    , div [ class "number" ] [ text <| String.fromInt <| maxRoles - 1 ]
                    ]
                ]

        viewCheckbox : String -> Bool -> (Bool -> Msg) -> Html Msg
        viewCheckbox title checked onChange =
            Html.label []
                [ Html.input
                    [ HA.type_ "checkbox" 
                    , HA.checked checked
                    , HE.onCheck
                        <| if editable
                            then onChange
                            else always Noop
                    , HA.disabled <| not editable
                    ] []
                , Html.span [] [ text title ]
                ]

    in div [ class "editor" ]
        [ div [ class "editor-roles" ]
            <| List.map viewSingleRole
            <| Dict.keys roles
        , viewRoleBar
        , div [ class "editor-checks" ]
            [ viewCheckbox "Tote können alle Rollen sehen"
                game.deadCanSeeAllRoles
                <| \new -> SendConf
                    { editGameConfig
                    | newDeadCanSeeAllRoles = Just new
                    }
            , viewCheckbox "Votings automatisch starten"
                game.autostartVotings
                <| \new -> SendConf
                    { editGameConfig
                    | autostartVotings = Just new
                    }
            , viewCheckbox "Votings automatisch beenden"
                game.autofinishVotings
                <| \new -> SendConf
                    { editGameConfig
                    | autofinishVotings = Just new
                    }
            ]
        , if editable
            then div 
                [ HA.classList
                    [ ("start-button", True)
                    , Tuple.pair "disabled"
                        <| maxRoles /= maxPlayer
                    ]
                , HE.onClick <| if maxRoles /= maxPlayer 
                    then Noop 
                    else StartGame
                ]
                [ text "Start" ]
            else text ""
        ]

