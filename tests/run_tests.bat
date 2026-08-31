@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem SCRIPT : Wrapper para execucao da Suite de Testes Automatizados
rem USO    : Execute este arquivo no Prompt de Comando ou PowerShell
rem ============================================================================

setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "TEST_SCRIPT=%SCRIPT_DIR%test_install_agent.ps1"

echo.
echo ================================================================
echo  OCS InstallerAndSync - Suite de Testes Automatizados
echo ================================================================
echo.

rem ------------------------------------------------------------
rem Verifica disponibilidade do PowerShell
rem ------------------------------------------------------------
where powershell.exe >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo [ERRO] PowerShell nao encontrado. Instale o Windows Management
    echo        Framework 3.0 ou superior para executar os testes.
    exit /b 99
)

rem Verifica versao minima do PowerShell (>= 3.0 requerido)
for /f "usebackq tokens=*" %%V in (
    `powershell -NoProfile -Command "$PSVersionTable.PSVersion.Major" 2^>nul`
) do set "PS_MAJOR=%%V"

if not defined PS_MAJOR (
    echo [ERRO] Nao foi possivel determinar a versao do PowerShell.
    exit /b 99
)

if !PS_MAJOR! lss 3 (
    echo [ERRO] PowerShell %PS_MAJOR%.x detectado. Versao minima requerida: 3.0
    exit /b 99
)

echo [INFO] PowerShell %PS_MAJOR%.x detectado. OK.
echo [INFO] Executando: %TEST_SCRIPT%
echo.

rem ------------------------------------------------------------
rem Executa a suite de testes
rem ------------------------------------------------------------
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%TEST_SCRIPT%"
set "TEST_EXIT=%ERRORLEVEL%"

echo.
echo ================================================================
if %TEST_EXIT% equ 0 (
    echo  RESULTADO: APROVADO - Todos os testes passaram.
) else (
    if %TEST_EXIT% equ 99 (
        echo  RESULTADO: ERRO FATAL - Suite nao pode ser executada.
    ) else (
        echo  RESULTADO: REPROVADO - %TEST_EXIT% falha detectada.
        echo  Corrija as falhas antes de gerar o relatorio de seguranca.
    )
)
echo ================================================================
echo.

endlocal
exit /b %TEST_EXIT%
