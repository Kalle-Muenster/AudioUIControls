@if "%ECHO_STATE%"=="" (@echo off ) else (@echo %ECHO_STATE% )

:: Prepare locations
set _name_=AudioUIControls
if "%DotNetVersionString%"=="" goto ERROR

set _call_=%CD%
cd %~dp0
set _here_=%CD%
set _root_=%CD%

:: Set VersionNumber
set AudioUIControlsVersionNumber=00000001
set AudioUIControlsVersionString=0.0.0.1


:: Set Dependencies
if "%ConsolaBinRoot%"=="" (
set ConsolaBinRoot=.\..\..\Consola\bin\%DotNetVersionString%
)
if "%Int24TypesBinRoot%"=="" (
set Int24TypesBinRoot=.\..\..\Int24Types\bin\%DotNetVersionString%
)
if "%ControlledValuesBinRoot%"=="" (
set ControlledValuesBinRoot=.\..\..\ControlledValues\bin\%DotNetVersionString%
)
if "%MotorsportBinRoot%"=="" (
set MotorsportBinRoot=.\..\..\TaskAssist-Motorsport\bin\%DotNetVersionString%
)
if "%WaveFileHandlingBinRoot%"=="" (
set WaveFileHandlingBinRoot=.\..\..\AudioDataHandling\bin\%DotNetVersionString%
)

:: Set parameters and solution files
if "%DotNetVersionString%"=="dot48" call %_root_%\Args "%~1" "%~2" "%~3" "%~4" MidiGui48.sln
if "%DotNetVersionString%"=="core5" call %_root_%\Args "%~1" "%~2" "%~3" "%~4" MidiGui50.sln

:: Do the Build
cd %_here_%
call MsBuild %_target_% %_args_%
cd %_call_%

if "%DotNetVersionString%"=="dot48" (
echo @UserControlTestContainer.exe "%_build_%\MidiControls.dll">"%_build_%\TestContainer.cmd"
)

:: Cleanup Environment
call %_root_%\Args ParameterCleanUp

goto DONE

:ERROR
echo.
echo Variable 'DotNetVersionString' must be set
echo.
:DONE


