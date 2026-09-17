@echo off
setlocal enabledelayedexpansion
set "SERVER_HOST=200.x.x.x"
set "OCS_AGENT_URL=http://%SERVER_HOST%/ocsinventory"
set "CADASTRO_API_URL=http://%SERVER_HOST%/cadastro_api/cadastrar.php"
set "SCRIPT_DIR=%~dp0"
set "TARGET_APP_DIR=%ProgramFiles%\InventarioPatrimonio"
openfiles >nul 2>&1
if %ERRORLEVEL% neq 0 (echo [ERRO] Execute como Administrador.& pause& exit /b 1)
set "CUSTOM_TAG="
set /p "CUSTOM_TAG=Digite a TAG (ENTER para %COMPUTERNAME%): "
if "%CUSTOM_TAG%"=="" set "CUSTOM_TAG=%COMPUTERNAME%"
call "%SCRIPT_DIR%install_ocs_agent.bat" "%OCS_AGENT_URL%" "%CUSTOM_TAG%"
if not exist "%TARGET_APP_DIR%" mkdir "%TARGET_APP_DIR%" >nul 2>&1
set "CADASTRO_SRC=%SCRIPT_DIR%CadastroPatrimonio.exe"
if not exist "%CADASTRO_SRC%" set "CADASTRO_SRC=%SCRIPT_DIR%..\client_app\CadastroPatrimonio.exe"
if exist "%CADASTRO_SRC%" copy /y "%CADASTRO_SRC%" "%TARGET_APP_DIR%\CadastroPatrimonio.exe" >nul
reg add "HKLM\Software\OCS_Inventario" /v "ApiEndpointUrl" /t REG_SZ /d "%CADASTRO_API_URL%" /f >nul 2>&1
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Run" /v "CadastroPatrimonio" /t REG_SZ /d "\"%TARGET_APP_DIR%\CadastroPatrimonio.exe\"" /f >nul 2>&1
if exist "%TARGET_APP_DIR%\CadastroPatrimonio.exe" start "" "%TARGET_APP_DIR%\CadastroPatrimonio.exe"
exit /b 0
