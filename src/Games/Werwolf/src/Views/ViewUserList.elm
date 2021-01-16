module Views.ViewUserList exposing (view)

import Data
import Model

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Dict exposing (Dict)

view : Data.Game -> Int -> Dict String Data.RoleTemplate -> Html msg
view game myId roles =
    let
        getUserRole : Int -> String
        getUserRole id =
            if id == game.leader
            then "Spielleiter"
            else case Dict.get id game.participants of
                Just Nothing -> "Mitspieler"
                Nothing -> "Spieler"
                Just (Just player) ->
                    String.concat
                        <| List.intersperse " "
                        <| List.filterMap identity
                        [ player.role
                            |> Maybe.map
                                (\rid ->
                                    Dict.get rid roles
                                        |> Maybe.map .name
                                        |> Maybe.withDefault rid
                                )
                            |> Maybe.withDefault "???"
                            |> Just
                        , if player.alive
                            then Nothing
                            else Just "(tot)"
                        , if player.loved
                            then Just "(verliebt)"
                            else Nothing
                        , if player.major
                            then Just "(Bürgermeister)"
                            else Nothing
                        ]

        viewGameUser : Int -> Data.GameUser -> Html msg
        viewGameUser id user =
            div [ HA.classList
                    [ ("user-frame", True)
                    , ("me", myId == id)
                    ]
                ]
                [ div [ class "user-image-box" ]
                    <| List.singleton
                    <| div [ class "user-image" ]
                    <| List.singleton
                    <| Html.img
                        [ HA.src user.img ]
                        []
                , div [ class "user-info-box" ]
                    [ div [ class "user-name" ]
                        [ text user.name ]
                    , div [ class "user-role" ]
                        [ text <| getUserRole id ]
                    ]

                ]



    in Dict.toList game.user
        |> List.map
            (\(id, user) -> viewGameUser id user)
        |> div [ class "user-container" ]
