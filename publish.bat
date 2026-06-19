@setlocal
@echo off

set "AppName=linerate"
set "ProjectFile=.\src\%AppName%\%AppName%.csproj"

pushd "%~dp0"

:: TODO: update the project version in the .csproj file

dotnet publish "%ProjectFile%" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o ./releases/win-x64
if %ERRORLEVEL% NEQ 0 pause & exit /B

if exist "%UserProfile%\Bin" (
	copy .\releases\win-x64\%AppName%.exe "%UserProfile%\Bin\"
	if %ERRORLEVEL% NEQ 0 pause
)

popd
endlocal
