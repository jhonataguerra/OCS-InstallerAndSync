@echo off
setlocal enabledelayedexpansion

echo ================================================================
echo [INFO] Iniciando build do CadastroPatrimonio
echo ================================================================

rem 1. Unified Bundling Configuration
echo [INFO] Gerando App.config para Unified Bundling (Compatibilidade Multi-OS)...
(
echo ^<?xml version="1.0" encoding="utf-8" ?^>
echo ^<configuration^>
echo   ^<startup useLegacyV2RuntimeActivationPolicy="true"^>
echo     ^<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/^>
echo     ^<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6"/^>
echo     ^<supportedRuntime version="v4.0"/^>
echo     ^<supportedRuntime version="v2.0.50727"/^>
echo   ^</startup^>
echo ^</configuration^>
) > "%~dp0CadastroPatrimonio.exe.config"

rem 2. Detect OS and build specific
echo [INFO] Detectando sistema operacional para build otimizado nativo...
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

set "CSC_35=%SystemRoot%\Microsoft.NET\Framework\v3.5\csc.exe"
set "CSC_4X=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

set "CSC_EXE="

if "!WIN_MAJOR!.!WIN_MINOR!"=="6.1" (
    echo [INFO] OS Detectado: Windows 7. Target: .NET 3.5
    set "CSC_EXE=!CSC_35!"
) else (
    if "!WIN_MAJOR!.!WIN_MINOR!"=="10.0" (
        if !WIN_BUILD! geq 22000 (
            echo [INFO] OS Detectado: Windows 11. Target: .NET 4.8
        ) else (
            echo [INFO] OS Detectado: Windows 10. Target: .NET 4.6
        )
        set "CSC_EXE=!CSC_4X!"
    ) else (
        echo [INFO] OS Nao mapeado. Target: .NET 4.x
        set "CSC_EXE=!CSC_4X!"
    )
)

if not exist "!CSC_EXE!" (
    echo [AVISO] Compilador nativo nao encontrado para a versao do SO. Buscando alternativas...
    if exist "%CSC_4X%" (
        set "CSC_EXE=%CSC_4X%"
    ) else if exist "%CSC_35%" (
        set "CSC_EXE=%CSC_35%"
    ) else (
        echo [ERRO] Nenhum compilador C# csc.exe localizado.
        exit /b 1
    )
)

set "REFERENCES=/r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Management.dll,System.Core.dll"

echo [INFO] Compilando executavel unificado CadastroPatrimonio.exe...
"!CSC_EXE!" /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest.xml" /out:"%~dp0CadastroPatrimonio.exe" %REFERENCES% "%~dp0AppConfig.cs" "%~dp0RegistryHelper.cs" "%~dp0SystemInfoCollector.cs" "%~dp0MainForm.Designer.cs" "%~dp0MainForm.cs" "%~dp0Program.cs"

if !ERRORLEVEL! neq 0 (
    echo [ERRO] Falha na compilacao principal.
    exit /b 1
)

echo.
echo [INFO] Gerando binarios especificos por versao do Windows / .NET...

rem Windows 7 (32-bit e 64-bit) -> .NET 3.5
if exist "%CSC_35%" (
    echo [INFO] Compilando: CadastroPatrimonio_Win7_net35.exe...
    "%CSC_35%" /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest.xml" /out:"%~dp0CadastroPatrimonio_Win7_net35.exe" %REFERENCES% "%~dp0AppConfig.cs" "%~dp0RegistryHelper.cs" "%~dp0SystemInfoCollector.cs" "%~dp0MainForm.Designer.cs" "%~dp0MainForm.cs" "%~dp0Program.cs"
)

rem Windows 10 e Windows 11 -> .NET 4.6 e 4.8
if exist "%CSC_4X%" (
    echo [INFO] Compilando: CadastroPatrimonio_Win10_net46.exe...
    "%CSC_4X%" /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest.xml" /out:"%~dp0CadastroPatrimonio_Win10_net46.exe" %REFERENCES% "%~dp0AppConfig.cs" "%~dp0RegistryHelper.cs" "%~dp0SystemInfoCollector.cs" "%~dp0MainForm.Designer.cs" "%~dp0MainForm.cs" "%~dp0Program.cs"

    echo [INFO] Compilando: CadastroPatrimonio_Win11_net48.exe...
    "%CSC_4X%" /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest.xml" /out:"%~dp0CadastroPatrimonio_Win11_net48.exe" %REFERENCES% "%~dp0AppConfig.cs" "%~dp0RegistryHelper.cs" "%~dp0SystemInfoCollector.cs" "%~dp0MainForm.Designer.cs" "%~dp0MainForm.cs" "%~dp0Program.cs"
)

echo.
echo ================================================================
echo SUCESSO: CadastroPatrimonio compilado com exito!
echo - CadastroPatrimonio.exe (Unificado + .config)
echo - CadastroPatrimonio_Win7_net35.exe (Win7 32/64 bit)
echo - CadastroPatrimonio_Win10_net46.exe (Win10)
echo - CadastroPatrimonio_Win11_net48.exe (Win11)
echo ================================================================
exit /b 0
