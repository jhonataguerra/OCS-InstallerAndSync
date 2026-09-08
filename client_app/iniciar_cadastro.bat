@echo off
rem ============================================================================
rem PROJETO: Inventario e Identificacao de Maquinas OCS
rem LAUNCHER: Iniciar Cadastro de Patrimonio conforme versao do Windows / .NET
rem EXECUCAO: GPO de Logon do Usuario
rem COMPATIBILIDADE: Windows 7 (32/64 bit), Windows 10, Windows 11
rem ============================================================================

setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"

rem 1. Deteccao da versao do Windows via 'ver'
for /f "tokens=2 delims=[]" %%i in ('ver') do (
    for /f "tokens=2,3,4 delims=. " %%a in ("%%i") do (
        set WIN_MAJOR=%%a
        set WIN_MINOR=%%b
        set WIN_BUILD=%%c
    )
)

if not defined WIN_MAJOR (
    for /f "tokens=4-6 delims=. " %%i in ('ver') do (
        set WIN_MAJOR=%%i
        set WIN_MINOR=%%j
        set WIN_BUILD=%%k
    )
)

rem 2. Selecao do executavel correspondente
set "TARGET_EXE="

rem Windows 7 (NT 6.1) -> .NET 3.5
if "!WIN_MAJOR!.!WIN_MINOR!"=="6.1" (
    if exist "%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe"
    )
) else (
    rem Windows 11 (NT 10.0, Build >= 22000) -> .NET 4.8
    if "!WIN_MAJOR!.!WIN_MINOR!"=="10.0" (
        if !WIN_BUILD! geq 22000 (
            if exist "%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe" (
                set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe"
            )
        ) else (
            rem Windows 10 -> .NET 4.6
            if exist "%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe" (
                set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe"
            )
        )
    )
)

rem 3. Estrategia de Fallback (Binario Padrao / Unificado)
if not defined TARGET_EXE (
    if exist "%SCRIPT_DIR%CadastroPatrimonio.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio.exe"
    ) else if exist "%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe"
    ) else if exist "%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe"
    )
)

rem 4. Disparo do executavel em background
if defined TARGET_EXE (
    start "" "!TARGET_EXE!"
    exit /b 0
) else (
    exit /b 1
)
