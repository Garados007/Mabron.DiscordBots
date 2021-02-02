@echo off
pushd src\Games\Werwolf
elm make --output ..\..\..\Mabron.DiscordBots\content\index.js src\Main.elm
copy index.html ..\..\..\Mabron.DiscordBots\bin\DebugTest\netcoreapp3.1\content\
popd
