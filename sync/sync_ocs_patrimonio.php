<?php
/**
 * ============================================================================
 * PROJETO: Inventario e Identificacao de Maquinas OCS
 * ETAPA 3: Script de Sincronizacao de Nomes (Hostname-Patrimonio) no OCS Server
 * ============================================================================
 *
 * Modo de uso (CLI):
 *   php sync_ocs_patrimonio.php
 *   php sync_ocs_patrimonio.php --dry-run
 *   php sync_ocs_patrimonio.php --force
 */

// Garante execucao apenas via linha de comando ou script agendado
if (php_sapi_name() !== 'cli') {
    die("Este script deve ser executado exclusivamente via CLI.\n");
}

$configFile = __DIR__ . '/config_sync.php';
if (!file_exists($configFile)) {
    die("[ERRO] Arquivo de configuracao '$configFile' nao encontrado.\n");
}

$config = require $configFile;

// Leitura de argumentos de linha de comando
$options = getopt('', ['dry-run', 'force', 'verbose']);
$isDryRun = isset($options['dry-run']);
$isForce  = isset($options['force']) || ($config['force_recheck_all'] ?? false);
$isVerbose = isset($options['verbose']);

function logMessage($msg, $logFile = null) {
    $timestamp = date('Y-m-d H:i:s');
    $formatted = "[$timestamp] $msg\n";
    echo $formatted;
    if (!empty($logFile)) {
        @file_put_contents($logFile, $formatted, FILE_APPEND);
    }
}

$logFile = $config['log_file'] ?? null;

logMessage("==================================================================", $logFile);
logMessage("INICIANDO PROCESSO DE SINCRONIZACAO OCS <-> PATRIMONIO", $logFile);
if ($isDryRun) {
    logMessage("[MODO SIMULACAO / DRY-RUN ATIVADO - Nenhuma alteracao sera gravada]", $logFile);
}

try {
    $dsn = sprintf(
        'mysql:host=%s;port=%d;dbname=%s;charset=%s',
        $config['db_host'],
        $config['db_port'],
        $config['db_name'],
        $config['db_charset'] ?? 'utf8mb4'
    );

    $pdo = new PDO($dsn, $config['db_user'], $config['db_pass'], [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        PDO::ATTR_EMULATE_PREPARES => false,
    ]);

    // 1. Busca cadastros pendentes de sincronizacao (ou todos se force estiver ativo)
    $sqlCadastros = "SELECT id, hostname, numero_patrimonio, nome_completo, setor_local, usuario_windows, sincronizado_ocs 
                     FROM `{$config['cadastro_table']}`";
    
    if (!$isForce) {
        $sqlCadastros .= " WHERE sincronizado_ocs = 0";
    }

    $stmtCadastros = $pdo->query($sqlCadastros);
    $cadastros = $stmtCadastros->fetchAll();

    $totalEncontrados = count($cadastros);
    logMessage("Registros de cadastro para processar: $totalEncontrados", $logFile);

    if ($totalEncontrados === 0) {
        logMessage("Nenhum registro pendente de sincronizacao. Finalizando.", $logFile);
        logMessage("==================================================================", $logFile);
        exit(0);
    }

    $contAtualizados = 0;
    $contJaAtualizados = 0;
    $contNaoLocalizados = 0;

    // Prepared statements para busca no OCS
    // Busca por:
    // a) Nome exato original (ex: 'PC-FINANCEIRO-01')
    // b) Nome ja formatado (ex: 'PC-FINANCEIRO-01-12345')
    // c) Tag no accountinfo ou hardware (onde a Etapa 1 colocou o Hostname original)
    $sqlBuscaOCS = "SELECT h.ID, h.NAME, h.USERID, h.TAG, a.TAG AS ACCOUNTINFO_TAG
                    FROM `{$config['ocs_hardware_table']}` h
                    LEFT JOIN `{$config['ocs_accountinfo_table']}` a ON h.ID = a.HARDWARE_ID
                    WHERE UPPER(TRIM(h.NAME)) = UPPER(TRIM(:host1))
                       OR UPPER(TRIM(h.NAME)) = UPPER(TRIM(:novo_nome))
                       OR UPPER(TRIM(h.TAG))  = UPPER(TRIM(:host2))
                       OR UPPER(TRIM(a.TAG))  = UPPER(TRIM(:host3))
                    LIMIT 1";

    $stmtBuscaOCS = $pdo->prepare($sqlBuscaOCS);

    // Prepared statements para atualizacao no OCS
    $sqlUpdateHardware = "UPDATE `{$config['ocs_hardware_table']}`
                          SET `NAME` = :novo_nome" . 
                          ($config['update_userid'] ? ", `USERID` = :usuario" : "") . 
                          " WHERE `ID` = :id";
    $stmtUpdateHardware = $pdo->prepare($sqlUpdateHardware);

    // Prepared statement para marcar cadastro como sincronizado
    $sqlUpdateCadastro = "UPDATE `{$config['cadastro_table']}`
                          SET `sincronizado_ocs` = 1, `data_sincronizacao` = NOW()
                          WHERE `id` = :id";
    $stmtUpdateCadastro = $pdo->prepare($sqlUpdateCadastro);

    foreach ($cadastros as $item) {
        $idCadastro   = $item['id'];
        $hostnameOrig = trim($item['hostname']);
        $patrimonio   = trim($item['numero_patrimonio']);
        $usuario      = trim($item['usuario_windows'] ?? $item['nome_completo']);

        $separador    = $config['name_separator'] ?? '-';
        $novoNome     = sprintf('%s%s%s', $hostnameOrig, $separador, $patrimonio);

        // Executa busca no OCS
        $stmtBuscaOCS->execute([
            ':host1'     => $hostnameOrig,
            ':novo_nome' => $novoNome,
            ':host2'     => $hostnameOrig,
            ':host3'     => $hostnameOrig
        ]);

        $ocsMachine = $stmtBuscaOCS->fetch();

        if (!$ocsMachine) {
            // Maquina ainda nao enviou inventario para o OCS Server
            logMessage("[PENDENTE] Hostname '$hostnameOrig' (Patrimonio: $patrimonio) ainda nao enviou inventario para o OCS. Sera processado no proximo ciclo.", $logFile);
            $contNaoLocalizados++;
            continue;
        }

        $ocsId       = $ocsMachine['ID'];
        $ocsNomeAtual = $ocsMachine['NAME'];

        // Verifica se ja esta com o nome correto
        if (strcasecmp($ocsNomeAtual, $novoNome) === 0) {
            if ($isVerbose) {
                logMessage("[JA SINCRONIZADO] OCS ID #$ocsId ja esta identificado como '$ocsNomeAtual'.", $logFile);
            }
            if (!$isDryRun) {
                $stmtUpdateCadastro->execute([':id' => $idCadastro]);
            }
            $contJaAtualizados++;
            continue;
        }

        // Realiza atualizacao do nome no OCS Server
        logMessage("[ATUALIZANDO] OCS ID #$ocsId: '$ocsNomeAtual' -> '$novoNome' (Patrimonio: $patrimonio)", $logFile);

        if (!$isDryRun) {
            $params = [
                ':novo_nome' => $novoNome,
                ':id'        => $ocsId
            ];
            if ($config['update_userid']) {
                $params[':usuario'] = $usuario;
            }

            $stmtUpdateHardware->execute($params);
            $stmtUpdateCadastro->execute([':id' => $idCadastro]);
        }

        $contAtualizados++;
    }

    logMessage("------------------------------------------------------------------", $logFile);
    logMessage("RESUMO DA SINCRONIZACAO:", $logFile);
    logMessage(" - Registros atualizados no OCS: $contAtualizados", $logFile);
    logMessage(" - Registros ja conformes:       $contJaAtualizados", $logFile);
    logMessage(" - Aguardando envio do OCS Agent:$contNaoLocalizados", $logFile);
    logMessage("Sincronizacao concluida com sucesso.", $logFile);
    logMessage("==================================================================", $logFile);

} catch (Exception $e) {
    logMessage("[ERRO CRITICO] Falha na execucao da sincronizacao: " . $e->getMessage(), $logFile);
    exit(1);
}
