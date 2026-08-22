@echo off
set "CSC_EXE="

if exist "%SystemRoot%\Microsoft.NET\Framework\v3.5\csc.exe" (
    set "CSC_EXE=%SystemRoot%\Microsoft.NET\Framework\v3.5\csc.exe"
    goto DO_BUILD
)

if exist "%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC_EXE=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
    goto DO_BUILD
)

echo [ERRO] Compilador C# csc.exe nao foi localizado no sistema.
exit /b 1

:DO_BUILD
echo [INFO] Utilizando compilador: %CSC_EXE%
echo Compilando CadastroPatrimonio.exe...

"%CSC_EXE%" /target:winexe /platform:anycpu /optimize+ /win32manifest:"%~dp0app.manifest" /out:"%~dp0CadastroPatrimonio.exe" /r:System.dll,System.Windows.Forms.dll,System.Drawing.dll,System.Management.dll "%~dp0AppConfig.cs" "%~dp0RegistryHelper.cs" "%~dp0SystemInfoCollector.cs" "%~dp0MainForm.Designer.cs" "%~dp0MainForm.cs" "%~dp0Program.cs"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ================================================================
    echo SUCESSO: CadastroPatrimonio.exe compilado com exito!
    echo ================================================================
    exit /b 0
) else (
    echo.
    echo [ERRO] Falha durante a compilacao do executavel.
    exit /b 1
)
