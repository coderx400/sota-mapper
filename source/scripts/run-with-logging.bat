@echo off

rem ****************************************************************************
rem * use this to run SotAMapper with logging enabled for debugging purposes.
rem * when logging is enabled, a lot file, SotAMapper_log.txt will be written
rem * in the same directory as the .exe with verbose logging
rem ****************************************************************************

pushd ..
start SotAMapper.exe -enablelogging
