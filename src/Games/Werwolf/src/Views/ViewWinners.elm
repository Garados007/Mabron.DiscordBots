module Views.ViewWinners exposing (..)

import Data

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Dict exposing (Dict)

view : Data.Game -> List String -> Dict String String -> Html Never
view game winners roles =
    div [ class "winner-box" ]
        <| List.map
            (\winner ->
                let
                    img : String
                    img = Dict.get winner game.user
                        |> Maybe.map .img
                        |> Maybe.withDefault ""
                    
                    name : String
                    name = Dict.get winner game.user
                        |> Maybe.map .name
                        |> Maybe.withDefault winner
                    
                    role : String
                    role = Dict.get winner game.participants
                        |> Maybe.andThen identity
                        |> Maybe.andThen .role
                        |> Maybe.andThen (\key -> Dict.get key roles)
                        |> Maybe.withDefault "???"
                in div [ class "winner" ]
                    [ Html.img
                        [ HA.src img ]
                        []
                    , div [ class "name" ]
                        [ text name ]
                    , div [ class "role" ]
                        [ text role ]
                    ]
            )
        <| winners
