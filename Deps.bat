@if "%ECHO_STATE%"=="" ( @echo off ) else ( @echo %ECHO_STATE% )

if "%~1"=="dot48" (
set DotNetVersionString=dot48
echo Set Dotnet Version 4.8
goto END
)
if "%~1"=="core5" (
set DotNetVersionString=core5
echo Set Dotnet Version 5.0
goto END
)

if "%DotNetVersionString%"=="core5" (
set ConsolaProject=C:\WORKSPACE\PROJECTS\GITSPACE\Consola\ConsolaCore5
set Int24TypesProject=C:\WORKSPACE\PROJECTS\GITSPACE\Int24Types\core5
set ControllerProject=C:\WORKSPACE\PROJECTS\GITSPACE\ControlledValues\Core5Dll
set TaskAssistProject=c:\WORKSPACE\PROJECTS\GITSPACE\Motorsports
set WaveLibProject=c:\WORKSPACE\PROJECTS\GITSPACE\AudioDataHandling\dll\%DotNetVersionString%
) else (
set ConsolaProject=C:\WORKSPACE\PROJECTS\GITSPACE\Consola\ConsolaDot48
set Int24TypesProject=C:\WORKSPACE\PROJECTS\GITSPACE\Int24Types\dot48
set ControllerProject=C:\WORKSPACE\PROJECTS\GITSPACE\ControlledValues\DotnetDll
set TaskAssistProject=c:\WORKSPACE\PROJECTS\GITSPACE\Motorsports
set WaveLibProject=c:\WORKSPACE\PROJECTS\GITSPACE\AudioDataHandling\dll\%DotNetVersionString%
)

set ARCH=%~1
set CONF=%~2
set CLEAN=%~3

pushd %ConsolaProject%
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
call Build.cmd "%ARCH%" "%CONF%" Test %CLEAN%
popd

pushd "%Int24TypesProject%"
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
popd

pushd "%ControllerProject%"
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
popd

pushd "%TaskAssistProject%"
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
popd

pushd "%WaveLibProject%"
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
popd

pushd "%~dp0"
call Build.cmd "%ARCH%" "%CONF%" %CLEAN%
popd

:END
set ARCH=
set CONF=
set CLEAN=
