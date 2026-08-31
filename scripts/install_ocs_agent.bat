@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem ETAPA 1: Script de Instalacao Silenciosa do OCS Inventory Agent 2.11
rem ARQUITETURA: Dual - Deteccao automatica de 32 bits (x86) e 64 bits (x64)
rem RESTRICAO: 100% Batch / CMD (Zero dependencia de PowerShell)
rem ============================================================================

setlocal enabledelayedexpansion

rem ============================================================================
rem [1] CONFIGURACOES DO AMBIENTE
rem ============================================================================
set "OCS_SERVER_URL=http://192.168.15.20/ocsinventory"
set "OCS_SSL=0"
set "INSTALLER_32=OCS-Agent-2.11-x86.exe"
set "INSTALLER_64=OCS-Agent-2.11-x64.exe"
set "FORCE_REINSTALL=0"
set "LOG_DIR=%SystemRoot%\Temp"
set "LOG_FILE=%LOG_DIR%\ocs_agent_install.log"
set "SCRIPT_DIR=%~dp0"

rem ============================================================================
rem [2] INICIALIZACAO DO LOG
rem ============================================================================
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >nul 2>&1

echo ================================================================ >> "%LOG_FILE%"
echo [%DATE% %TIME%] INICIANDO VERIFICACAO DO OCS AGENT >> "%LOG_FILE%"
echo [%DATE% %TIME%] Hostname: %COMPUTERNAME% >> "%LOG_FILE%"

rem ============================================================================
rem [3] DETECCAO DE ARQUITETURA DO SISTEMA OPERACIONAL
rem ============================================================================
set "OS_ARCH=x86"

if defined PROCESSOR_ARCHITEW6432 (
    set "OS_ARCH=x64"
    echo [%DATE% %TIME%] Arquitetura detectada: x64 [WOW64 - ARCHITEW6432=!PROCESSOR_ARCHITEW6432!] >> "%LOG_FILE%"
    goto ARCH_DETECTADA
)

if /i "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set "OS_ARCH=x64"
    echo [%DATE% %TIME%] Arquitetura detectada: x64 [nativo - ARCHITECTURE=!PROCESSOR_ARCHITECTURE!] >> "%LOG_FILE%"
    goto ARCH_DETECTADA
)

if /i "%PROCESSOR_ARCHITECTURE%"=="IA64" (
    set "OS_ARCH=x64"
    echo [%DATE% %TIME%] Arquitetura detectada: x64 [Itanium - ARCHITECTURE=!PROCESSOR_ARCHITECTURE!] >> "%LOG_FILE%"
    goto ARCH_DETECTADA
)

echo [%DATE% %TIME%] Arquitetura detectada: x86 [32-bit nativo - ARCHITECTURE=!PROCESSOR_ARCHITECTURE!] >> "%LOG_FILE%"

:ARCH_DETECTADA

rem ============================================================================
rem [4] VERIFICACAO DE INSTALACAO EXISTENTE (IDEMPOTENCIA)
rem ============================================================================
if "%FORCE_REINSTALL%"=="0" (
    if defined TEST_OVERRIDE_SERVICE (
        if "%TEST_OVERRIDE_SERVICE%"=="1" (
            echo [%DATE% %TIME%] OCS Inventory Service ja esta instalado no sistema. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    ) else (
        sc query "OCS Inventory Service" >nul 2>&1
        if !ERRORLEVEL! equ 0 (
            echo [%DATE% %TIME%] OCS Inventory Service ja esta instalado no sistema. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    )

    if defined TEST_OVERRIDE_PF (
        if exist "%TEST_OVERRIDE_PF%\OCS Inventory Agent\OCSInventory.exe" (
            echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    ) else (
        if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" (
            echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    )

    if defined TEST_OVERRIDE_PFX86 (
        if exist "%TEST_OVERRIDE_PFX86%\OCS Inventory Agent\OCSInventory.exe" (
            echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles x86. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    ) else (
        if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" (
            echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles x86. >> "%LOG_FILE%"
            goto END_SUCCESS_ALREADY_INSTALLED
        )
    )
)

rem ============================================================================
rem [5] SELECAO DINAMICA DO INSTALADOR CONFORME ARQUITETURA DETECTADA
rem ============================================================================
if "%OS_ARCH%"=="x64" (
    set "INSTALLER_NAME=!INSTALLER_64!"
    echo [%DATE% %TIME%] Selecao: Instalador 64-bit selecionado [!INSTALLER_64!] >> "%LOG_FILE%"
) else (
    set "INSTALLER_NAME=!INSTALLER_32!"
    echo [%DATE% %TIME%] Selecao: Instalador 32-bit selecionado [!INSTALLER_32!] >> "%LOG_FILE%"
)

set "INSTALLER_PATH=%SCRIPT_DIR%%INSTALLER_NAME%"

if not exist "%INSTALLER_PATH%" (
    echo [%DATE% %TIME%] ERRO CRITICO: Instalador nao encontrado em: "%INSTALLER_PATH%" >> "%LOG_FILE%"
    goto ERROR_MISSING_INSTALLER
)

echo [%DATE% %TIME%] Utilizando instalador: "%INSTALLER_PATH%" >> "%LOG_FILE%"

rem ============================================================================
rem [6] EXECUCAO SILENCIOSA DO INSTALADOR COM PARAMETRO /TAG
rem ============================================================================
echo [%DATE% %TIME%] Executando instalador [Arch: %OS_ARCH%] com /TAG=%COMPUTERNAME%... >> "%LOG_FILE%"

start /wait "" "%INSTALLER_PATH%" /S /NOSPLASH /NO_SYSTRAY /SERVER=%OCS_SERVER_URL% /SSL=%OCS_SSL% /DEBUG=2 /TAG=%COMPUTERNAME% /NOW
set "INSTALL_EXIT_CODE=%ERRORLEVEL%"

echo [%DATE% %TIME%] Codigo de saida do instalador: %INSTALL_EXIT_CODE% >> "%LOG_FILE%"

if %INSTALL_EXIT_CODE% neq 0 (
    echo [%DATE% %TIME%] AVISO: Instalador finalizou com codigo diferente de zero [%INSTALL_EXIT_CODE%]. Verifique o log do agente. >> "%LOG_FILE%"
)

rem ============================================================================
rem [7] FORCAR DISPARO DE INVENTARIO INICIAL
rem ============================================================================
set "OCS_EXE="
if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" (
    set "OCS_EXE=%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe"
)
if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" (
    set "OCS_EXE=%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe"
)

if defined OCS_EXE (
    echo [%DATE% %TIME%] Disparando inventario inicial: "!OCS_EXE!" /now >> "%LOG_FILE%"
    start "" "!OCS_EXE!" /now
    echo [%DATE% %TIME%] Instalacao e disparo de inventario concluidos com sucesso. >> "%LOG_FILE%"
) else (
    echo [%DATE% %TIME%] ATENCAO: OCSInventory.exe nao localizado apos instalacao. Inventario sera enviado na proxima execucao do servico. >> "%LOG_FILE%"
)

goto FINISH

rem ============================================================================
rem ROTULOS DE SAIDA
rem ============================================================================
:END_SUCCESS_ALREADY_INSTALLED
echo [%DATE% %TIME%] Nenhuma acao necessaria. OCS Agent ja esta instalado e em execucao. >> "%LOG_FILE%"
goto FINISH

:ERROR_MISSING_INSTALLER
echo [%DATE% %TIME%] Falha critica: pacote '%INSTALLER_NAME%' nao encontrado na pasta do script. >> "%LOG_FILE%"
echo [%DATE% %TIME%] Verifique se os arquivos '%INSTALLER_32%' e '%INSTALLER_64%' estao na pasta compartilhada. >> "%LOG_FILE%"
exit /b 1

:FINISH
echo [%DATE% %TIME%] Finalizando script. Arquitetura: %OS_ARCH% >> "%LOG_FILE%"
echo ================================================================ >> "%LOG_FILE%"
exit /b 0
