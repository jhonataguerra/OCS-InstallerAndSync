@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem ETAPA 1: Script de Instalacao Silenciosa do OCS Inventory Agent 2.11
rem ARQUITETURA: Pacote Unico 32 bits (Compativel com Windows 7 a 11, 32 e 64 bits)
rem RESTRICAO: 100% Batch / CMD (Zero dependencia de PowerShell)
rem ============================================================================

setlocal enabledelayedexpansion

rem ============================================================================
rem [1] CONFIGURACOES DO AMBIENTE
rem ============================================================================
set "OCS_SERVER_URL=http://192.168.2.48/ocsinventory"
set "OCS_SSL=0"
set "INSTALLER_NAME=OCS-Agent-2.11-Universal.exe"
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
rem [3] VERIFICACAO DE INSTALACAO EXISTENTE (IDEMPOTENCIA)
rem ============================================================================
if "%FORCE_REINSTALL%"=="0" (
    rem Verifica se o servico OCS Inventory ja existe
    sc query "OCS Inventory Service" >nul 2>&1
    if !ERRORLEVEL! equ 0 (
        echo [%DATE% %TIME%] OCS Inventory Service ja esta instalado no sistema. >> "%LOG_FILE%"
        goto END_SUCCESS_ALREADY_INSTALLED
    )

    rem Verifica se o executavel padrao ja existe em Program Files (32 ou 64 bits)
    if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" (
        echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles. >> "%LOG_FILE%"
        goto END_SUCCESS_ALREADY_INSTALLED
    )
    if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" (
        echo [%DATE% %TIME%] Executavel OCSInventory.exe encontrado em ProgramFiles(x86). >> "%LOG_FILE%"
        goto END_SUCCESS_ALREADY_INSTALLED
    )
)

rem ============================================================================
rem [4] LOCALIZACAO DO INSTALADOR UNIFICADO (32 BITS)
rem ============================================================================
set "INSTALLER_PATH=%SCRIPT_DIR%%INSTALLER_NAME%"

if not exist "%INSTALLER_PATH%" (
    echo [%DATE% %TIME%] ERRO CRITICO: Instalador nao encontrado em: "%INSTALLER_PATH%" >> "%LOG_FILE%"
    goto ERROR_MISSING_INSTALLER
)

echo [%DATE% %TIME%] Utilizando instalador: "%INSTALLER_PATH%" >> "%LOG_FILE%"

rem ============================================================================
rem [5] EXECUCAO SILENCIOSA DO INSTALADOR COM PARAMETRO /TAG
rem ============================================================================
rem Parametros OCS Agent:
rem /S           -> Modo silencioso
rem /SERVER=...  -> URL do servidor OCS
rem /TAG=...     -> Tag definida dinamicamente com o Hostname da maquina
rem /NOSPLASH    -> Nao exibe tela inicial
rem /NOW         -> Dispara primeiro inventario imediatamente
rem /NO_SYSTRAY  -> Oculta icone da bandeja do sistema
rem /DEBUG=2     -> Ativa log detalhado no cliente
rem /SSL=0       -> Conexao HTTP direta
echo [%DATE% %TIME%] Executando instalador com TAG=%COMPUTERNAME%... >> "%LOG_FILE%"

start /wait "" "%INSTALLER_PATH%" /S /NOSPLASH /NO_SYSTRAY /SERVER=%OCS_SERVER_URL% /SSL=%OCS_SSL% /DEBUG=2 /TAG=%COMPUTERNAME% /NOW
set "INSTALL_EXIT_CODE=%ERRORLEVEL%"

echo [%DATE% %TIME%] Codigo de saida do instalador: %INSTALL_EXIT_CODE% >> "%LOG_FILE%"

if %INSTALL_EXIT_CODE% neq 0 (
    echo [%DATE% %TIME%] AVISO: Instalador finalizou com codigo diferente de zero (%INSTALL_EXIT_CODE%). >> "%LOG_FILE%"
)

rem ============================================================================
rem [6] FORCAR DISPARO DE INVENTARIO INICIAL
rem ============================================================================
set "OCS_EXE="
if exist "%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe" (
    set "OCS_EXE=%ProgramFiles%\OCS Inventory Agent\OCSInventory.exe"
)
if exist "%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe" (
    set "OCS_EXE=%ProgramFiles(x86)%\OCS Inventory Agent\OCSInventory.exe"
)

if defined OCS_EXE (
    echo [%DATE% %TIME%] Disparando inventario inicial: "%OCS_EXE%" /now >> "%LOG_FILE%"
    start "" "%OCS_EXE%" /now
    echo [%DATE% %TIME%] Instalacao e disparo de inventario concluidos com sucesso. >> "%LOG_FILE%"
) else (
    echo [%DATE% %TIME%] ATENCAO: Executavel nao localizado apos instalacao. >> "%LOG_FILE%"
)

goto FINISH

:END_SUCCESS_ALREADY_INSTALLED
echo [%DATE% %TIME%] Nenhuma acao necessaria. OCS Agent ja esta em execucao. >> "%LOG_FILE%"
goto FINISH

:ERROR_MISSING_INSTALLER
echo [%DATE% %TIME%] Falha na execucao: pacote '%INSTALLER_NAME%' inexistente na pasta do script. >> "%LOG_FILE%"
exit /b 1

:FINISH
echo [%DATE% %TIME%] Finalizando script. >> "%LOG_FILE%"
echo ================================================================ >> "%LOG_FILE%"
exit /b 0
