module Language exposing (..)

import Dict exposing (Dict)
import Maybe.Extra
import Json.Decode as JD exposing (Decoder)
import Json.Decode.Pipeline exposing (required)

type Language
    = LanguageText String
    | LanguageNode (Dict String Language)
    | LanguageUnknown

getLanguage : Dict comparable Language -> Maybe comparable -> Language
getLanguage dict =
    Maybe.andThen
        (\k -> Dict.get k dict)
    >> Maybe.withDefault LanguageUnknown

getText : Language -> List String -> Maybe String
getText language path =
    case (language, path) of
        (LanguageText text, []) -> Just text
        (LanguageNode dict, first::other) ->
            Dict.get first dict
                |> Maybe.andThen
                    (\x -> getText x other)
        _ -> Nothing

getTextOrPath : Language -> List String -> String
getTextOrPath language path =
    case getText language path of
        Just x -> x
        Nothing -> String.concat
            <| List.intersperse "." path

decodeLanguage : Decoder Language
decodeLanguage =
    JD.oneOf
        [ JD.map LanguageText JD.string
        , JD.map LanguageNode
            <| JD.dict
            <| JD.lazy
            <| \() -> decodeLanguage
        , JD.succeed LanguageUnknown
        ]

type alias ThemeKey = (String, String, String)

type alias LanguageInfo =
    { languages: Dict String String
    , themes: Dict String (Dict String (Dict String String))
    }

firstTheme : LanguageInfo -> Maybe ThemeKey
firstTheme info =
    Dict.foldl
        (\k1 v1 r1 ->
            case r1 of
                Just r1_ -> Just r1_
                Nothing ->
                    Dict.foldl
                        (\k2 v2 r2 ->
                            case r2 of
                                Just r2_ -> Just r2_
                                Nothing ->
                                    Dict.foldl
                                        (\k3 _ ->
                                            Just << Maybe.withDefault
                                                (k1, k2, k3)
                                        )
                                        Nothing
                                        v2
                        )
                        Nothing
                        v1
        )
        Nothing
        info.themes

getThemeName : LanguageInfo -> ThemeKey -> Maybe String
getThemeName info (impl, ui, lang) =
    info.themes
        |> Dict.get impl
        |> Maybe.andThen (Dict.get ui)
        |> Maybe.andThen (Dict.get lang)

decodeLanguageInfo : Decoder LanguageInfo
decodeLanguageInfo =
    JD.succeed LanguageInfo
        |> required "languages" (JD.dict JD.string)
        |> required "themes" 
            (JD.dict 
                <| JD.dict 
                <| JD.dict JD.string
            )
