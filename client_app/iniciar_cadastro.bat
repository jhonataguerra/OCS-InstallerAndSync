@echo off
setlocal enabledelayedexpansion
set "SCRIPT_DIR=%~dp0"

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

set "TARGET_EXE="
if "!WIN_MAJOR!.!WIN_MINOR!"=="6.1" (
    if exist "%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe" set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe"
) else if "!WIN_MAJOR!.!WIN_MINOR!"=="10.0" (
    if !WIN_BUILD! geq 22000 (
        if exist "%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe" set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe"
    ) else (
        if exist "%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe" set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe"
    )
)

rem 3. Estrategia de Fallback Resiliente (Unificado ou Qualquer Binario Presente)
if not defined TARGET_EXE (
    if exist "%SCRIPT_DIR%CadastroPatrimonio.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio.exe"
    ) else if exist "%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win10_net46.exe"
    ) else if exist "%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win11_net48.exe"
    ) else if exist "%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CadastroPatrimonio_Win7_net35.exe"
    )
)
if defined TARGET_EXE (
    start "" "!TARGET_EXE!"
    exit /b 0
)
echo [ERRO] Nenhum executavel CadastroPatrimonio compativel foi encontrado.
exit /b 1
