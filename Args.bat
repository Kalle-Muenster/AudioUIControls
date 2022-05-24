@if "%ECHO_STATE%"=="" ( @echo off ) else ( @echo %ECHO_STATE% )
if not "%~1"=="ParameterCleanUp" goto CHECK_ARGS

:CLEAN_UP_ARGS
set _target_=
set _build_=
set _clean_=
set _here_=
set _root_=
set _call_=
set _arch_=
set _conf_=
set _args_=
set _name_=
goto DONE

:CHECK_ARGS
set _clean_=

:CHECK_ARCH
if "%~1"=="x86" set _arch_=x86
if "%~2"=="x86" set _arch_=x86
if "%~3"=="x86" set _arch_=x86

:CHECK_CONF
if "%~1"=="Debug" set _conf_=Debug
if "%~2"=="Debug" set _conf_=Debug
if "%~3"=="Debug" set _conf_=Debug

:CHECK_CLEAN
if "%~1"=="Clean" set _clean_= /t:Clean
if "%~2"=="Clean" set _clean_= /t:Clean
if "%~3"=="Clean" set _clean_= /t:Clean

:CHECK_TEST
if "%~6"=="" goto SET_DEFAULTS
if "%~1"=="Test" set _target_=%~6
if "%~2"=="Test" set _target_=%~6
if "%~3"=="Test" set _target_=%~6

:CHECK_FOURTH
if "%~4"=="x86" set _arch_=x86
if "%~4"=="Debug" set _conf_=Debug
if "%~4"=="Clean" set _clean_= /t:Clean
if "%~4"=="Test" set _target_=%~6

:SET_DEFAULTS
if "%_arch_%"=="" set _arch_=x64
if "%_conf_%"=="" set _conf_=Release
if "%_target_%"=="" set _target_=%~5

:SET_BUILD_CALL
set _build_=%_root_%\bin\%DotNetVersionString%
if not "%_call_%"=="%_here_%" (
  if not "%_call_%"=="%_root_%" (
    set _build_=%_call_%
  )
)
if "%_clean_%"=="" set %_name_%BinRoot=%_build_%
set _build_=%_build_%\%_arch_%\%_conf_%
set _args_=/p:Configuration=%_conf_%;Platform=%_arch_%;OutDir=%_build_%%_clean_%

:DONE
