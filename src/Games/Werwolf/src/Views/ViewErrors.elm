module Views.ViewErrors exposing (..)

import Data
import Model

import Html exposing (Html, div, text)
import Html.Attributes as HA exposing (class)
import Html.Events as HE

view : List String -> Html Int
view errors =
    div [ class "error-box" ]
        <| List.indexedMap
            (\index error ->
                div [ class "error-entry" ]
                    [ div 
                        [ class "remove"
                        , HE.onClick index
                        ]
                        [ text "X" ]
                    , div [ class "text" ]
                        [ text error ]
                    ]
            )
        <| errors
