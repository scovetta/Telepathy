@echo off
doskey /macrofile=alias
doskey root=cd %~dp0
doskey src=cd %~dp0src
doskey test=cd %~dp0test
doskey soa=cd %~dp0src\soa
doskey build=cd %~dp0 $T build.bat
doskey rebuild=cd %~dp0 $T rebuild.bat
doskey unittest=cd %~dp0 $T unittest.bat