#Requires -Version 3.0
<#
.SYNOPSIS
    Suite de Testes Automatizados — install_ocs_agent.bat (OCS Installer & Sync)

.DESCRIPTION
    Valida o comportamento do script de instalação do OCS Inventory Agent 2.11
    em cenários simulados: detecção de arquitetura, idempotência, resiliência a
    erros e geração de log.

    Os testes NÃO instalam o agente real e são seguros para qualquer máquina
    de desenvolvimento ou CI. Cada teste cria um ambiente isolado em Temp.

.PARAMETER Verbose
    Exibe mensagens detalhadas de debug durante a execução.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File test_install_agent.ps1
    powershell -ExecutionPolicy Bypass -File test_install_agent.ps1 -Verbose

.NOTES
    Projeto : OCS InstallerAndSync
    Autor   : Engenharia de Sistemas / TI
    Versão  : 1.0.0
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ============================================================================
# CONFIGURAÇÕES
# ============================================================================
$SCRIPT_ROOT    = Split-Path -Parent $PSScriptRoot   # raiz do repositório
$BATCH_SOURCE   = Join-Path $SCRIPT_ROOT 'scripts\install_ocs_agent.bat'
$SYSTEM_LOG     = "$env:SystemRoot\Temp\ocs_agent_install.log"

# Contadores globais
$script:PASS_COUNT = 0
$script:FAIL_COUNT = 0
$script:TEST_RESULTS = [System.Collections.Generic.List[hashtable]]::new()

# ============================================================================
# FRAMEWORK MÍNIMO DE TESTES
# ============================================================================
function Write-TestHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Register-TestResult {
    param([string]$Id, [string]$Description, [bool]$Passed, [string]$Detail = '')

    if ($Passed) {
        Write-Host "  [OK] $Id PASS  $Description" -ForegroundColor Green
        $script:PASS_COUNT++
        $script:TEST_RESULTS.Add(@{ Id=$Id; Status='PASS'; Description=$Description; Detail='' })
    }
    else {
        Write-Host "  [FALHA] $Id FAIL  $Description" -ForegroundColor Red
        if ($Detail) {
            Write-Host "       Detalhe: $Detail" -ForegroundColor Yellow
        }
        $script:FAIL_COUNT++
        $script:TEST_RESULTS.Add(@{ Id=$Id; Status='FAIL'; Description=$Description; Detail=$Detail })
    }
}

# ============================================================================
# FUNÇÕES AUXILIARES
# ============================================================================

<#
.SYNOPSIS
    Cria um ambiente de teste isolado em %TEMP% com os arquivos necessários.
.OUTPUTS
    Caminho do diretório temporário criado.
#>
function New-TestEnvironment {
    param(
        [switch]$WithInstaller32,    # Cria OCS-Agent-2.11-x86.exe (cópia de cmd.exe)
        [switch]$WithInstaller64,    # Cria OCS-Agent-2.11-x64.exe (cópia de cmd.exe)
        [switch]$WithOCSServiceBin,  # Cria ProgramFiles\OCS Inventory Agent\OCSInventory.exe
        [switch]$WithOCSServiceBinX86 # Cria ProgramFiles(x86)\OCS Inventory Agent\OCSInventory.exe
    )

    $testId = [System.Guid]::NewGuid().ToString('N').Substring(0, 8)
    $testDir = Join-Path $env:TEMP "ocs_test_$testId"
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null

    # Copia o batch script para o diretório de teste
    Copy-Item -Path $BATCH_SOURCE -Destination (Join-Path $testDir 'install_ocs_agent.bat') -Force

    # Mock de instalador: usa cmd.exe como stub (executável válido que aceita parâmetros)
    $cmdExe = "$env:SystemRoot\System32\cmd.exe"

    if ($WithInstaller32) {
        Copy-Item -Path $cmdExe -Destination (Join-Path $testDir 'OCS-Agent-2.11-x86.exe') -Force
        Write-Verbose "  [Env] Criado mock installer x86 em $testDir"
    }

    if ($WithInstaller64) {
        Copy-Item -Path $cmdExe -Destination (Join-Path $testDir 'OCS-Agent-2.11-x64.exe') -Force
        Write-Verbose "  [Env] Criado mock installer x64 em $testDir"
    }

    if ($WithOCSServiceBin) {
        $binDir = Join-Path $testDir 'FakePF\OCS Inventory Agent'
        New-Item -ItemType Directory -Path $binDir -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $binDir 'OCSInventory.exe') -Force | Out-Null
        Write-Verbose "  [Env] Criado fake OCSInventory.exe em ProgramFiles"
    }

    if ($WithOCSServiceBinX86) {
        $binDir = Join-Path $testDir 'FakePFx86\OCS Inventory Agent'
        New-Item -ItemType Directory -Path $binDir -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $binDir 'OCSInventory.exe') -Force | Out-Null
        Write-Verbose "  [Env] Criado fake OCSInventory.exe em ProgramFiles(x86)"
    }

    return $testDir
}

<#
.SYNOPSIS
    Executa o batch de instalação em um ambiente simulado.
.OUTPUTS
    Hashtable com ExitCode e NewLogContent (linhas adicionadas ao log durante a execução).
#>
function Invoke-InstallBatch {
    param(
        [string]$TestDir,
        [hashtable]$EnvOverrides = @{}
    )

    # Registra tamanho do log antes para isolar entradas desta execução
    $logSizeBefore = 0
    if (Test-Path $SYSTEM_LOG) {
        $logSizeBefore = (Get-Item $SYSTEM_LOG).Length
    }

    # Salva e aplica sobrescritas de ambiente
    $savedEnv = @{}
    foreach ($key in $EnvOverrides.Keys) {
        $savedEnv[$key] = [System.Environment]::GetEnvironmentVariable($key, 'Process')
        $val = $EnvOverrides[$key]
        if ($null -eq $val) {
            [System.Environment]::SetEnvironmentVariable($key, $null, 'Process')
        } else {
            [System.Environment]::SetEnvironmentVariable($key, $val, 'Process')
        }
    }

    try {
        $batchPath = Join-Path $TestDir 'install_ocs_agent.bat'
        $proc = Start-Process -FilePath 'cmd.exe' `
            -ArgumentList "/c `"$batchPath`"" `
            -WorkingDirectory $TestDir `
            -Wait -PassThru -WindowStyle Hidden

        $exitCode = $proc.ExitCode
    }
    finally {
        # Restaura variáveis de ambiente
        foreach ($key in $savedEnv.Keys) {
            [System.Environment]::SetEnvironmentVariable($key, $savedEnv[$key], 'Process')
        }
    }

    # Lê apenas as linhas novas do log
    $newLogContent = ''
    if (Test-Path $SYSTEM_LOG) {
        $logContent = [System.IO.File]::ReadAllText($SYSTEM_LOG, [System.Text.Encoding]::Default)
        if ($logContent.Length -gt $logSizeBefore) {
            $newLogContent = $logContent.Substring([int]$logSizeBefore)
        }
    }

    return @{ ExitCode = $exitCode; Log = $newLogContent }
}

<#
.SYNOPSIS Remove um diretório de teste ignorando erros.#>
function Remove-TestEnvironment {
    param([string]$Path)
    if ($Path -and (Test-Path $Path)) {
        Remove-Item -Path $Path -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# ============================================================================
# ============================================================================
#  SUITE DE TESTES
# ============================================================================
# ============================================================================
Write-TestHeader 'OCS Installer — Suite de Testes Automatizados v1.0'
Write-Host "  Batch fonte : $BATCH_SOURCE"
Write-Host "  Log sistema : $SYSTEM_LOG"
Write-Host ""

if (-not (Test-Path $BATCH_SOURCE)) {
    Write-Host "[ERRO FATAL] Script de instalação não encontrado: $BATCH_SOURCE" -ForegroundColor Red
    exit 99
}

# ============================================================================
# T-01: Detecção de Arquitetura — Sistema 64-bit (AMD64 nativo)
# ============================================================================
Write-Host "-- T-01: Deteccao de Arquitetura x64 (AMD64 nativo) --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithInstaller64 -WithInstaller32

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'AMD64'
        'PROCESSOR_ARCHITEW6432'  = $null   # Nao deve existir em processo nativo 64-bit
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.Log -match 'x64') -and
              ($result.Log -match 'ARCHITECTURE=AMD64') -and
              ($result.Log -match 'OCS-Agent-2\.11-x64\.exe')
    $detail = if (-not $passed) { "Log nao continha selecao de x64. Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-01' 'Sistema 64-bit (AMD64 nativo) → instalador x64 selecionado' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-02: Detecção de Arquitetura — Sistema 32-bit puro
# ============================================================================
Write-Host "-- T-02: Deteccao de Arquitetura x86 (32-bit nativo) --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithInstaller64 -WithInstaller32

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'x86'
        'PROCESSOR_ARCHITEW6432'  = $null   # Nao definida em SO 32-bit puro
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.Log -match 'x86') -and
              ($result.Log -match 'ARCHITECTURE=x86') -and
              ($result.Log -match 'OCS-Agent-2\.11-x86\.exe')
    $detail = if (-not $passed) { "Log nao continha selecao de x86. Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-02' 'Sistema 32-bit nativo → instalador x86 selecionado' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-03: Detecção de Arquitetura — WOW64 (processo 32-bit em SO 64-bit)
# ============================================================================
Write-Host "-- T-03: Deteccao de Arquitetura x64 via WOW64 (ARCHITEW6432) --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithInstaller64 -WithInstaller32

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'x86'    # Processo 32-bit (WOW64)
        'PROCESSOR_ARCHITEW6432'  = 'AMD64'  # SO real e 64-bit
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.Log -match 'x64') -and
              ($result.Log -match 'WOW64') -and
              ($result.Log -match 'OCS-Agent-2\.11-x64\.exe')
    $detail = if (-not $passed) { "Log nao detectou WOW64. Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-03' 'WOW64 (32-bit em SO 64-bit via ARCHITEW6432) → instalador x64 selecionado' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-04: Idempotência — OCSInventory.exe já existe em ProgramFiles (64-bit path)
# ============================================================================
Write-Host "-- T-04: Idempotencia — binario ja existe em ProgramFiles --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithOCSServiceBin

try {
    $fakePF = Join-Path $testDir 'FakePF'
    $envOvr = @{
        'ProgramFiles'       = $fakePF
        'ProgramFiles(x86)' = "$testDir\FakePFx86_vazio"
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.ExitCode -eq 0) -and
              ($result.Log -match 'ja esta instalado|encontrado em ProgramFiles|Nenhuma acao necessaria')
    $detail = if (-not $passed) { "ExitCode=$($result.ExitCode). Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-04' 'Binario OCSInventory.exe em ProgramFiles → script encerra sem reinstalar (exit 0)' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-05: Idempotência — OCSInventory.exe já existe em ProgramFiles(x86)
# ============================================================================
Write-Host "-- T-05: Idempotencia — binario ja existe em ProgramFiles(x86) --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithOCSServiceBinX86

try {
    $fakePFx86 = Join-Path $testDir 'FakePFx86'
    $envOvr = @{
        'ProgramFiles'       = "$testDir\FakePF_vazio"
        'ProgramFiles(x86)' = $fakePFx86
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.ExitCode -eq 0) -and
              ($result.Log -match 'encontrado em ProgramFiles|Nenhuma acao necessaria')
    $detail = if (-not $passed) { "ExitCode=$($result.ExitCode). Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-05' 'Binario OCSInventory.exe em ProgramFiles(x86) → script encerra sem reinstalar (exit 0)' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-06: Erro — Instalador ausente → exit code 1 + log de erro crítico
# ============================================================================
Write-Host "-- T-06: Resiliencia — instalador ausente --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment   # SEM instaladores

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'AMD64'
        'PROCESSOR_ARCHITEW6432'  = $null
        'ProgramFiles'            = "$testDir\FakePF_vazio"
        'ProgramFiles(x86)'       = "$testDir\FakePFx86_vazio"
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.ExitCode -eq 1) -and ($result.Log -match 'ERRO CRITICO')
    $detail = if (-not $passed) { "ExitCode=$($result.ExitCode). Esperado 1. Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-06' 'Instalador ausente → exit code 1 e ERRO CRITICO no log' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-07: Log — Arquivo de log é criado e contém o hostname da máquina
# ============================================================================
Write-Host "-- T-07: Log — arquivo criado e contem hostname --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment -WithInstaller32 -WithInstaller64

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'AMD64'
        'PROCESSOR_ARCHITEW6432'  = $null
    }

    # Remove log anterior para garantir criacao limpa
    if (Test-Path $SYSTEM_LOG) { Remove-Item $SYSTEM_LOG -Force -ErrorAction SilentlyContinue }

    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $logExists  = Test-Path $SYSTEM_LOG
    $logHasHost = $logExists -and ((Get-Content $SYSTEM_LOG -Raw -ErrorAction SilentlyContinue) -match [regex]::Escape($env:COMPUTERNAME))

    $passed = $logExists -and $logHasHost
    $detail = if (-not $passed) { "logExists=$logExists, logHasHost=$logHasHost" } else { '' }
    Register-TestResult 'T-07' 'Log criado em C:\Windows\Temp e contem COMPUTERNAME' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# T-08: Log — Erros críticos são registrados com prefixo "ERRO CRITICO"
# ============================================================================
Write-Host "-- T-08: Log — erros criticos registrados com prefixo correto --" -ForegroundColor DarkCyan
$testDir = New-TestEnvironment   # SEM instaladores (garante erro)

try {
    $envOvr = @{
        'PROCESSOR_ARCHITECTURE'  = 'x86'
        'PROCESSOR_ARCHITEW6432'  = $null
        'ProgramFiles'            = "$testDir\FakePF_vazio"
        'ProgramFiles(x86)'       = "$testDir\FakePFx86_vazio"
    }
    $result = Invoke-InstallBatch -TestDir $testDir -EnvOverrides $envOvr

    $passed = ($result.Log -match 'ERRO CRITICO') -and ($result.ExitCode -eq 1)
    $detail = if (-not $passed) { "Prefixo ERRO CRITICO ausente. Log: $($result.Log)" } else { '' }
    Register-TestResult 'T-08' 'Falha critica registrada com prefixo ERRO CRITICO no log' $passed $detail
}
finally { Remove-TestEnvironment $testDir }

# ============================================================================
# SUMÁRIO FINAL
# ============================================================================
$total = $script:PASS_COUNT + $script:FAIL_COUNT

Write-Host ""
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "  RESULTADO FINAL" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""

foreach ($r in $script:TEST_RESULTS) {
    $color = if ($r.Status -eq 'PASS') { 'Green' } else { 'Red' }
    $mark  = if ($r.Status -eq 'PASS') { '[OK]  ' } else { '[FAIL]' }
    Write-Host "  $mark $($r.Id)  $($r.Description)" -ForegroundColor $color
}

Write-Host ""

if ($script:FAIL_COUNT -eq 0) {
    Write-Host "  Suite: $($script:PASS_COUNT)/$total testes passaram." -ForegroundColor Green
    Write-Host "  STATUS: TODOS OS TESTES APROVADOS" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "  Suite: $($script:PASS_COUNT)/$total testes passaram. $($script:FAIL_COUNT) falha(s)." -ForegroundColor Red
    Write-Host "  STATUS: FALHAS DETECTADAS — corrija antes de prosseguir." -ForegroundColor Red
    exit 1
}
