@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem SCRIPT: instalar_workgroup.bat (Instalacao Fora do Active Directory)
rem CARACTERISTICAS:
rem   - Execucao direta sem perguntas no prompt.
rem   - IP/Host configuravel diretamente na variavel SERVER_HOST abaixo.
rem   - TAG padrao utilizada: %COMPUTERNAME% (Hostname da maquina).
rem ============================================================================

setlocal enabledelayedexpansion

:: =============================================================================
:: [1] CONFIGURACOES EDITAVEIS (MODIFIQUE AQUI SE NECESSARIO)
:: =============================================================================
:: Defina o IP externo ou Host FQDN do servidor OCS:
set "SERVER_HOST=200.x.x.x"

:: Protocolo e portas (ajuste se utilizar HTTPS ou porta alternativa)
set "OCS_AGENT_URL=http://%SERVER_HOST%/ocsinventory"
set "CADASTRO_API_URL=http://%SERVER_HOST%/cadastro_api/cadastrar.php"

:: Diretorios do script e de instalacao local
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

echo =======================================================================
echo   INSTALACAO OCS INVENTORY - WORKGROUP / FORA DO DOMINIO
echo =======================================================================
echo Servidor OCS: %OCS_AGENT_URL%
echo Hostname/TAG: %COMPUTERNAME%
echo -----------------------------------------------------------------------

:: =============================================================================
:: [3] INSTALACAO DO OCS INVENTORY AGENT
:: =============================================================================
echo [1/4] Instalando / Verificando OCS Inventory Agent...
if exist "%SCRIPT_DIR%install_ocs_agent.bat" (
    call "%SCRIPT_DIR%install_ocs_agent.bat" "%OCS_AGENT_URL%" "%COMPUTERNAME%"
) else (
    echo [AVISO] Script install_ocs_agent.bat nao encontrado na mesma pasta.
)

:: =============================================================================
:: [4] COPIA DO APLICATIVO CADASTRO DE PATRIMONIO
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
:: [5] GRAVACAO DE CONFIGURACOES E CHAVE RUN NO REGISTRO DO WINDOWS
:: =============================================================================
echo [3/4] Gravando configuracoes no Registro do Windows...

:: Salva Endpoint da API Externa para o executavel de cadastro
reg add "HKLM\Software\OCS_Inventario" /v "ApiEndpointUrl" /t REG_SZ /d "%CADASTRO_API_URL%" /f >nul 2>&1

:: Registra para ser executado no logon de qualquer usuario
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Run" /v "CadastroPatrimonio" /t REG_SZ /d "\"%TARGET_APP_DIR%\CadastroPatrimonio.exe\"" /f >nul 2>&1

echo       Registro configurado com sucesso.

:: =============================================================================
:: [6] DISPARAR APLICATIVO NA SESSAO ATUAL
:: =============================================================================
echo [4/4] Inicializando Cadastro de Patrimonio...
if exist "%TARGET_APP_DIR%\CadastroPatrimonio.exe" (
    start "" "%TARGET_APP_DIR%\CadastroPatrimonio.exe"
)

echo.
echo =======================================================================
echo   INSTALACAO WORKGROUP CONCLUIDA COM SUCESSO!
echo =======================================================================
echo.
pause
exit /b 0
