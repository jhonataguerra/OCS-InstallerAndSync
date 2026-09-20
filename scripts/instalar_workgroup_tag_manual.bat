@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem SCRIPT: instalar_workgroup_tag_manual.bat
rem CARACTERISTICAS:
rem   - Solicita a digitacao da TAG (ex: PACO-123456, VIC-00987, etc.) no prompt.
rem   - Caso o usuario tecle ENTER sem digitar nada, assume o %COMPUTERNAME%.
rem   - IP/Host do servidor OCS configuravel diretamente na variavel SERVER_HOST.
rem   - Instala o OCS Agent com a TAG digitada.
rem ============================================================================

setlocal enabledelayedexpansion

:: =============================================================================
:: [1] CONFIGURACOES DO SERVIDOR OCS (MODIFIQUE O IP SE NECESSARIO)
:: =============================================================================
set "SERVER_HOST=200.x.x.x"

set "OCS_AGENT_URL=http://%SERVER_HOST%/ocsinventory"
set "CADASTRO_API_URL=http://%SERVER_HOST%/cadastro_api/cadastrar.php"
set "SCRIPT_DIR=%~dp0"
set "TARGET_APP_DIR=%ProgramFiles%\InventarioPatrimonio"

:: =============================================================================
:: [2] VERIFICACAO DE ELEVACAO ADMINISTRATIVA
:: =============================================================================
openfiles >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERRO] Este script precisa ser executado com privilegios de Administrador!
    echo Clique com o botao direito no arquivo e selecione: "Executar como Administrador".
    echo.
    pause
    exit /b 1
)

:: =============================================================================
:: [3] SOLICITACAO DA TAG MANUAL
:: =============================================================================
cls
echo =======================================================================
echo   INSTALACAO OCS INVENTORY (WORKGROUP COM DEFINICAO DE TAG MANUAL)
echo =======================================================================
echo Servidor OCS: %OCS_AGENT_URL%
echo Hostname Atual: %COMPUTERNAME%
echo -----------------------------------------------------------------------
echo.
set "CUSTOM_TAG="
set /p "CUSTOM_TAG=Digite a TAG para este computador (Ex: VIC-123456 ou tecle ENTER para %COMPUTERNAME%): "

if "%CUSTOM_TAG%"=="" (
    set "CUSTOM_TAG=%COMPUTERNAME%"
)

echo.
echo >> Utilizando TAG: [ %CUSTOM_TAG% ]
echo.

:: =============================================================================
:: [4] INSTALACAO DO OCS INVENTORY AGENT COM A TAG DIGITADA
:: =============================================================================
echo [1/4] Instalando / Verificando OCS Inventory Agent com a TAG [%CUSTOM_TAG%]...
if exist "%SCRIPT_DIR%install_ocs_agent.bat" (
    call "%SCRIPT_DIR%install_ocs_agent.bat" "%OCS_AGENT_URL%" "%CUSTOM_TAG%"
) else (
    echo [AVISO] Script install_ocs_agent.bat nao encontrado na mesma pasta.
)

:: =============================================================================
:: [5] COPIA DO APLICATIVO CADASTRO DE PATRIMONIO
:: =============================================================================
echo [2/4] Copiando aplicativo CadastroPatrimonio...
if not exist "%TARGET_APP_DIR%" (
    mkdir "%TARGET_APP_DIR%" >nul 2>&1
)

set "CADASTRO_SRC=%SCRIPT_DIR%CadastroPatrimonio.exe"
if not exist "%CADASTRO_SRC%" (
    if exist "%SCRIPT_DIR%..\client_app\CadastroPatrimonio.exe" (
        set "CADASTRO_SRC=%SCRIPT_DIR%..\client_app\CadastroPatrimonio.exe"
    )
)

if exist "%CADASTRO_SRC%" (
    copy /y "%CADASTRO_SRC%" "%TARGET_APP_DIR%\CadastroPatrimonio.exe" >nul
    echo       Aplicativo copiado para: %TARGET_APP_DIR%\CadastroPatrimonio.exe
) else (
    echo [ERRO] Executavel CadastroPatrimonio.exe nao encontrado!
)

if exist "%SCRIPT_DIR%CadastroPatrimonio.exe.config" (
    copy /y "%SCRIPT_DIR%CadastroPatrimonio.exe.config" "%TARGET_APP_DIR%\" >nul
) else if exist "%SCRIPT_DIR%..\client_app\CadastroPatrimonio.exe.config" (
    copy /y "%SCRIPT_DIR%..\client_app\CadastroPatrimonio.exe.config" "%TARGET_APP_DIR%\" >nul
)

:: =============================================================================
:: [6] GRAVACAO DE CONFIGURACOES E CHAVE RUN NO REGISTRO DO WINDOWS
:: =============================================================================
echo [3/4] Gravando configuracoes no Registro do Windows...

:: Salva Endpoint da API Externa para o executavel de cadastro
reg add "HKLM\Software\OCS_Inventario" /v "ApiEndpointUrl" /t REG_SZ /d "%CADASTRO_API_URL%" /f >nul 2>&1

:: Registra para ser executado no logon de qualquer usuario
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Run" /v "CadastroPatrimonio" /t REG_SZ /d "\"%TARGET_APP_DIR%\CadastroPatrimonio.exe\"" /f >nul 2>&1

echo       Registro configurado com sucesso.

:: =============================================================================
:: [7] DISPARAR APLICATIVO NA SESSAO ATUAL
:: =============================================================================
echo [4/4] Inicializando Cadastro de Patrimonio...
if exist "%TARGET_APP_DIR%\CadastroPatrimonio.exe" (
    start "" "%TARGET_APP_DIR%\CadastroPatrimonio.exe"
)

echo.
echo =======================================================================
echo   INSTALACAO WORKGROUP CONCLUIDA COM SUCESSO!
echo   TAG Definida: %CUSTOM_TAG%
echo =======================================================================
echo.
pause
exit /b 0
