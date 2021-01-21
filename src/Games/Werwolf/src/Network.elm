module Network exposing
    ( EditGameConfig
    , EditUserConfig
    , NetworkRequest (..)
    , NetworkResponse (..)
    , editGameConfig
    , editUserConfig
    , executeRequest
    )

import Data
import Http
import Dict exposing (Dict)
import Url

type NetworkRequest
    = GetRoles
    | GetGame String
    | PostGameConfig String EditGameConfig
    | PostUserConfig String EditUserConfig
    | GetGameStart String
    | GetVotingStart String String
    | GetVote String String String
    | GetVotingWait String String
    | GetVotingFinish String String
    | GetGameNext String
    | GetGameStop String
    | GetUserKick String String

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
        PostUserConfig token config -> postUserConfig token config
            |> mapRespError
        GetGameStart token -> getGameStart token
            |> mapRespError
        GetVotingStart token vid -> getVotingStart token vid
            |> mapRespError
        GetVote token vid id -> getVote token vid id
            |> mapRespError
        GetVotingWait token vid -> getVotingWait token vid
            |> mapRespError
        GetVotingFinish token vid -> getVotingFinish token vid
            |> mapRespError
        GetGameNext token -> getGameNext token
            |> mapRespError
        GetGameStop token -> getGameStop token
            |> mapRespError
        GetUserKick token user -> getUserKick token user
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

editGameConfig : EditGameConfig
editGameConfig =
    { newLeader = Nothing
    , newConfig = Nothing
    , newDeadCanSeeAllRoles = Nothing
    , autostartVotings = Nothing
    , autofinishVotings = Nothing
    , votingTimeout = Nothing
    , autofinishRound = Nothing
    }

type alias EditGameConfig =
    { newLeader: Maybe String
    , newConfig: Maybe (Dict String Int)
    , newDeadCanSeeAllRoles: Maybe Bool
    , autostartVotings: Maybe Bool
    , autofinishVotings: Maybe Bool
    , votingTimeout: Maybe Bool
    , autofinishRound: Maybe Bool
    }

convertEditGameConfig : EditGameConfig -> String
convertEditGameConfig config =
    [ Maybe.map
        (\leader -> "leader=" ++ leader
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
            |> (::) "config="
            |> String.concat
        )
        config.newConfig
    , Maybe.map
        (\new -> "dead-can-see-all-roles=" ++
            if new then "true" else "false"
        )
        config.newDeadCanSeeAllRoles
    , Maybe.map
        (\new -> "autostart-votings=" ++
            if new then "true" else "false"        
        )
        config.autostartVotings
    , Maybe.map
        (\new -> "autofinish-votings=" ++
            if new then "true" else "false"
        )
        config.autofinishVotings
    , Maybe.map
        (\new -> "voting-timeout=" ++
            if new then "true" else "false"
        )
        config.votingTimeout
    , Maybe.map
        (\new -> "autofinish-rounds=" ++
            if new then "true" else "false"
        )
        config.autofinishRound
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

editUserConfig : EditUserConfig
editUserConfig =
    { newTheme = Nothing
    , newBackground = Nothing
    }

type alias EditUserConfig =
    { newTheme: Maybe String
    , newBackground: Maybe String
    }

convertEditUserConfig : EditUserConfig -> String
convertEditUserConfig config =
    [ Maybe.map
        (\theme -> "theme=" ++ theme)
        config.newTheme
    , Maybe.map
        (\background -> "background=" ++ Url.percentEncode background)
        config.newBackground
    ]
    |> List.filterMap identity
    |> List.intersperse "&"
    |> String.concat

postUserConfig : String -> EditUserConfig -> Cmd (Response Data.Error)
postUserConfig token config =
    Http.post
        { url = "/api/game/" ++ token ++ "/user/config"
        , body = Http.stringBody "application/x-www-form-urlencoded"
            <| convertEditUserConfig config
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

getVotingStart : String -> String -> Cmd (Response Data.Error)
getVotingStart token vid =
    getErrorReq 
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ vid ++ "/start"

getVote : String -> String -> String -> Cmd (Response Data.Error)
getVote token vid id =
    getErrorReq
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ vid ++ "/vote/" ++ id
        
getVotingWait : String -> String -> Cmd (Response Data.Error)
getVotingWait token vid =
    getErrorReq
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ vid ++ "/wait"

getVotingFinish : String -> String -> Cmd (Response Data.Error)
getVotingFinish token vid =
    getErrorReq
        <| "/api/game/" ++ token ++ "/voting/" 
        ++ vid ++ "/finish"

getGameNext : String -> Cmd (Response Data.Error)
getGameNext token =
    getErrorReq <| "/api/game/" ++ token ++ "/next"

getGameStop : String -> Cmd (Response Data.Error)
getGameStop token =
    getErrorReq <| "/api/game/" ++ token ++ "/stop"

getUserKick : String -> String -> Cmd (Response Data.Error)
getUserKick token uid =
    getErrorReq <| "/api/game/" ++ token ++ "/kick/" ++ uid
