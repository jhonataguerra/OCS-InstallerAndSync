@echo off
setlocal enabledelayedexpansion
if "%~1" neq "" (set "OCS_SERVER_URL=%~1") else if not defined OCS_SERVER_URL set "OCS_SERVER_URL=http://192.168.2.48/ocsinventory"
if "%~2" neq "" (set "OCS_TAG=%~2") else if not defined OCS_TAG set "OCS_TAG=%COMPUTERNAME%"
if not defined OCS_SSL set "OCS_SSL=0"
set "INSTALLER_32=OCS-Agent-2.11-x86.exe"
set "INSTALLER_64=OCS-Agent-2.11-x64.exe"
set "FORCE_REINSTALL=0"
set "LOG_DIR=%SystemRoot%\Temp"
set "LOG_FILE=%LOG_DIR%\ocs_agent_install.log"
set "SCRIPT_DIR=%~dp0"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1
echo ================================================================ >> "%LOG_FILE%"
echo [%DATE% %TIME%] INICIANDO VERIFICACAO DO OCS AGENT >> "%LOG_FILE%"
echo [%DATE% %TIME%] Hostname: %COMPUTERNAME% >> "%LOG_FILE%"
echo [%DATE% %TIME%] Servidor OCS: %OCS_SERVER_URL% >> "%LOG_FILE%"
echo [%DATE% %TIME%] TAG OCS: %OCS_TAG% >> "%LOG_FILE%"
set "OS_ARCH=x86"
if defined PROCESSOR_ARCHITEW6432 (set "OS_ARCH=x64") else if /i "%PROCESSOR_ARCHITECTURE%"=="AMD64" (set "OS_ARCH=x64") else if /i "%PROCESSOR_ARCHITECTURE%"=="IA64" (set "OS_ARCH=x64")
echo [%DATE% %TIME%] Arquitetura detectada: %OS_ARCH% >> "%LOG_FILE%"
if "%FORCE_REINSTALL%"=="0" (
    sc query "OCS Inventory Service" >nul 2>&1
    if !ERRORLEVEL! equ 0 goto END_SUCCESS_ALREADY_INSTALLED
    if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" goto END_SUCCESS_ALREADY_INSTALLED
    if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" goto END_SUCCESS_ALREADY_INSTALLED
)
if "%OS_ARCH%"=="x64" (set "INSTALLER_NAME=%INSTALLER_64%") else (set "INSTALLER_NAME=%INSTALLER_32%")
set "INSTALLER_PATH=%SCRIPT_DIR%%INSTALLER_NAME%"
if not exist "%INSTALLER_PATH%" (
    echo [%DATE% %TIME%] ERRO CRITICO: Instalador nao encontrado em: "%INSTALLER_PATH%" >> "%LOG_FILE%"
    exit /b 1
)
start /wait "" "%INSTALLER_PATH%" /S /NOSPLASH /NO_SYSTRAY /SERVER=%OCS_SERVER_URL% /SSL=%OCS_SSL% /DEBUG=2 /TAG=%OCS_TAG% /NOW
set "INSTALL_EXIT_CODE=%ERRORLEVEL%"
echo [%DATE% %TIME%] Codigo de saida: %INSTALL_EXIT_CODE% >> "%LOG_FILE%"
if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" set "OCS_EXE=%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe"
if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" set "OCS_EXE=%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe"
if defined OCS_EXE start "" "!OCS_EXE!" /now
:END_SUCCESS_ALREADY_INSTALLED
echo [%DATE% %TIME%] Finalizando script. Arquitetura: %OS_ARCH% >> "%LOG_FILE%"
exit /b 0
