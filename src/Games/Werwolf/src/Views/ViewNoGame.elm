module Views.ViewNoGame exposing (..)

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)

view : Bool -> Html msg
view isLoading =
    div [ class "frame-status-box" ]
        <| List.singleton
        <| div [ class "frame-status" ]
        <| List.singleton
        <| text
        <| if isLoading
            then "Daten werden geladen ..."
            else "Spiel nicht gefunden."
