module Views.ViewWinners exposing (..)

import Data
import Level exposing (Level, LevelData)

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Dict exposing (Dict)
import Time exposing (Posix)

view : Posix -> Dict String Level -> Data.Game -> List String 
    -> Dict String String -> Html Never
view now levels game winners roles =
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

                    level : LevelData
                    level = Dict.get winner levels
                        |> Maybe.map (Level.getData now)
                        |> Maybe.withDefault
                            { level = 0
                            , xp = 0
                            , maxXp = 0
                            }
                    
                in div [ class "winner" ]
                    [ Html.img
                        [ HA.src img ]
                        []
                    , div [ class "name" ]
                        [ text name ]
                    , div [ class "role" ]
                        [ text role ]
                    , div 
                        [ class "user-info-level" ]
                        [ div [ class "text" ]
                            [ div [] [ text "Level" ]
                            , div [] [ text <| String.fromInt level.level ]
                            ]
                        , div [ class "outer" ]
                            [ div
                                [ HA.style "width"
                                    <| (\x -> String.fromFloat x ++ "%")
                                    <|
                                        if level.xp == level.maxXp
                                        then 100
                                        else 100 * (toFloat level.xp) / (toFloat level.maxXp)
                                ]
                                []
                            ]
                        ]
                    ]
            )
        <| winners
