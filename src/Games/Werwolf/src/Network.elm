module Network exposing
    ( EditGameConfig
    , NetworkRequest (..)
    , NetworkResponse (..)
    , executeRequest
    )

import Data
import Http
import Json.Decode as JD
import Dict exposing (Dict)

type NetworkRequest
    = GetRoles
    | GetGame String
    | PostGameConfig String EditGameConfig
    | GetGameStart String
    | GetVotingStart String Int
    | GetVote String Int Int
    | GetVotingFinish String Int
    | GetGameNext String
    | GetGameStop String

type NetworkResponse
    = RespRoles (Dict String Data.RoleTemplate)
    | RespGame Data.GameUserResult
    | RespError String
    | RespNoError

mapRespError : Cmd (Response Data.Error) -> Cmd (Response NetworkResponse)
mapRespError =
    Cmd.map
        (Result.map
            ( Maybe.map RespError
                >> Maybe.withDefault RespNoError
            )
        )

executeRequest : NetworkRequest -> Cmd NetworkResponse
executeRequest request =
    Cmd.map
        (\result ->
            case result of
                Ok d -> d
                Err (Http.BadUrl url) -> 
                    RespError <| "Http Error: Bad Url " ++ url
                Err Http.Timeout ->
                    RespError <| "Http Error: Timeout"
                Err Http.NetworkError ->
                    RespError <| "Http Error: Network Error"
                Err (Http.BadStatus status) ->
                    RespError <| "Http Error: Bad Status " ++ String.fromInt status
                Err (Http.BadBody msg) ->
                    RespError <| "Http Error: Bad Body: " ++ msg
        )
    <| case request of
        GetRoles -> getRoles
            |> Cmd.map (Result.map RespRoles)
        GetGame token -> getGame token
            |> Cmd.map (Result.map RespGame)
        PostGameConfig token config -> postGameConfig token config
            |> mapRespError
        GetGameStart token -> getGameStart token
            |> mapRespError
        GetVotingStart token vid -> getVotingStart token vid
            |> mapRespError
        GetVote token vid id -> getVote token vid id
            |> mapRespError
        GetVotingFinish token vid -> getVotingFinish token vid
            |> mapRespError
        GetGameNext token -> getGameNext token
            |> mapRespError
        GetGameStop token -> getGameStop token
            |> mapRespError

type alias Response a = Result Http.Error a

getRoles : Cmd (Response (Dict String Data.RoleTemplate))
getRoles =
    Http.get
        { url = "/api/roles"
        , expect = Http.expectJson identity Data.decodeRoleTemplates
        }

getGame : String -> Cmd (Response Data.GameUserResult)
getGame token =
    Http.get
        { url = "/api/game/" ++ token
        , expect = Http.expectJson identity Data.decodeGameUserResult
        }

type alias EditGameConfig =
    { newLeader: Maybe Int
    , newConfig: Maybe (Dict String Int)
    , newDeadCanSeeAllRoles: Maybe Bool
    }

convertEditGameConfig : EditGameConfig -> String
convertEditGameConfig config =
    [ Maybe.map
        (\leader -> "leader=" ++ String.fromInt leader
        )
        config.newLeader
    , Maybe.map
        (\conf -> 
            Dict.foldl
                (\key value list ->
                    list ++ List.repeat value key
                )
                []
                conf
            |> List.intersperse ","
            |> String.concat
        )
        config.newConfig
    , Maybe.map
        (\new -> "dead-can-see-all-roles=" ++
            if new then "true" else "false"
        )
        config.newDeadCanSeeAllRoles
    ]
    |> List.filterMap identity
    |> List.intersperse "&"
    |> String.concat

postGameConfig : String -> EditGameConfig -> Cmd (Response Data.Error)
postGameConfig token config =
    Http.post
        { url = "/api/game/" ++ token ++ "/config"
        , body = Http.stringBody "application/x-www-form-urlencoded"
            <| convertEditGameConfig config
        , expect = Http.expectJson identity Data.decodeError
        }

getErrorReq : String -> Cmd (Response Data.Error)
getErrorReq url = 
    Http.get
        { url = url
        , expect = Http.expectJson identity Data.decodeError
        }

getGameStart : String -> Cmd (Response Data.Error)
getGameStart token =
    getErrorReq <| "/api/game/" ++ token ++ "/start"

getVotingStart : String -> Int -> Cmd (Response Data.Error)
getVotingStart token vid =
    getErrorReq 
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ String.fromInt vid ++ "/start"

getVote : String -> Int -> Int -> Cmd (Response Data.Error)
getVote token vid id =
    getErrorReq
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ String.fromInt vid ++ "/vote/" ++ String.fromInt id

getVotingFinish : String -> Int -> Cmd (Response Data.Error)
getVotingFinish token vid =
    getErrorReq
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ String.fromInt vid ++ "/finish"

getGameNext : String -> Cmd (Response Data.Error)
getGameNext token =
    getErrorReq <| "/api/game/" ++ token ++ "/next"

getGameStop : String -> Cmd (Response Data.Error)
getGameStop token =
    getErrorReq <| "/api/game/" ++ token ++ "/stop"
