@echo off

rem ****************************************************************************
rem * assumes VS bin directory (with devenv.exe) and 7-zip install directory
rem * are both in the PATH and available
rem ****************************************************************************

pushd ..

for %%x in (bin obj) do (
   if exist %%x (
      rmdir /s /q %%x
   )
)

devenv SotAMapper.sln /rebuild Release 
if %ERRORLEVEL% GEQ 1 goto finished
if exist bin\Release\SotAMapper.pdb del /f /q bin\Release\SotAMapper.pdb

if exist ..\source.zip del /f /q ..\source.zip
7z a ..\source.zip *.* -mx9 -r -x!bin -x!obj -x!.vs -x!*.vspscc -x!*.vssscc -x!*.user
move ..\source.zip bin\Release

:finished
popd
pause
