<?php
/**
 * ============================================================================
 * PROJETO: Inventario e Identificacao de Maquinas OCS
 * ETAPA 3: Script de Sincronizacao do Patrimonio no OCS Server
 * ============================================================================
 *
 * Modo de uso (CLI):
 *   php sync_ocs_patrimonio.php
 *   php sync_ocs_patrimonio.php --dry-run
 *   php sync_ocs_patrimonio.php --force
 *   php sync_ocs_patrimonio.php --verbose
 *
 * FUNCIONAMENTO:
 *   1. Le os cadastros pendentes da tabela computadores_cadastro.
 *   2. Localiza a maquina no OCS atraves de hardware.NAME = hostname.
 *   3. Obtem o HARDWARE_ID correspondente.
 *   4. Monta a TAG dinamicamente conforme o prefixo do hostname:
 *        PAC -> PACO-{PATRIMONIO}
 *        PLA -> VIC-{PATRIMONIO}
 *        DES -> VIC-{PATRIMONIO}
 *        FAZ -> VIC-{PATRIMONIO}
 *        Outros -> LOCAL-{PATRIMONIO}
 *   5. Atualiza SOMENTE accountinfo.TAG.
 *   6. Nao altera hardware.NAME.
 *   7. Nao altera hardware.USERID.
 *   8. Nao altera hardware.TAG.
 * ============================================================================
 */

// ============================================================================
// [1] SOMENTE CLI
// ============================================================================

if (php_sapi_name() !== 'cli') {
    die("Este script deve ser executado exclusivamente via CLI.\n");
}


// ============================================================================
// [2] CARREGAMENTO DA CONFIGURACAO
// ============================================================================

$configFile = __DIR__ . '/config_sync.php';

if (!file_exists($configFile)) {
    die("[ERRO] Arquivo de configuracao '$configFile' nao encontrado.\n");
}

$config = require $configFile;


// ============================================================================
// [3] ARGUMENTOS DE LINHA DE COMANDO
// ============================================================================

$options = getopt('', ['dry-run', 'force', 'verbose']);

$isDryRun = isset($options['dry-run']);

$isForce = isset($options['force'])
    || ($config['force_recheck_all'] ?? false);

$isVerbose = isset($options['verbose']);


// ============================================================================
// [4] FUNCAO DE LOG
// ============================================================================

function logMessage($msg, $logFile = null)
{
    $timestamp = date('Y-m-d H:i:s');

    $formatted = "[$timestamp] $msg\n";

    echo $formatted;

    if (!empty($logFile)) {
        @file_put_contents(
            $logFile,
            $formatted,
            FILE_APPEND
        );
    }
}


// ============================================================================
// [5] CONFIGURACAO DO LOG
// ============================================================================

$logFile = $config['log_file'] ?? null;


// ============================================================================
// [6] INICIO
// ============================================================================

logMessage(
    "==================================================================",
    $logFile
);

logMessage(
    "INICIANDO PROCESSO DE SINCRONIZACAO OCS <-> PATRIMONIO",
    $logFile
);

if ($isDryRun) {
    logMessage(
        "[MODO SIMULACAO / DRY-RUN ATIVADO - Nenhuma alteracao sera gravada]",
        $logFile
    );
}


// ============================================================================
// [7] CONEXAO COM O BANCO
// ============================================================================

try {

    $dsn = sprintf(
        'mysql:host=%s;port=%d;dbname=%s;charset=%s',
        $config['db_host'],
        $config['db_port'],
        $config['db_name'],
        $config['db_charset'] ?? 'utf8mb4'
    );

    $pdo = new PDO(
        $dsn,
        $config['db_user'],
        $config['db_pass'],
        [
            PDO::ATTR_ERRMODE            => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES   => false
        ]
    );

    logMessage(
        "Conexao com o banco '{$config['db_name']}' estabelecida.",
        $logFile
    );


// ============================================================================
// [8] BUSCA DOS CADASTROS
// ============================================================================

    $sqlCadastros = "
        SELECT
            id,
            hostname,
            numero_patrimonio,
            nome_completo,
            setor_local,
            usuario_windows,
            sincronizado_ocs
        FROM `{$config['cadastro_table']}`
    ";

    if (!$isForce) {
        $sqlCadastros .= "
            WHERE sincronizado_ocs = 0
        ";
    }

    $stmtCadastros = $pdo->query($sqlCadastros);

    $cadastros = $stmtCadastros->fetchAll();

    $totalEncontrados = count($cadastros);

    logMessage(
        "Registros de cadastro para processar: $totalEncontrados",
        $logFile
    );

    if ($totalEncontrados === 0) {

        logMessage(
            "Nenhum registro pendente de sincronizacao. Finalizando.",
            $logFile
        );

        logMessage(
            "==================================================================",
            $logFile
        );

        exit(0);
    }


// ============================================================================
// [9] CONTADORES
// ============================================================================

    $contAtualizados    = 0;
    $contJaAtualizados  = 0;
    $contNaoLocalizados = 0;
    $contSemAccountInfo = 0;
    $contErros          = 0;


// ============================================================================
// [10] BUSCA DA MAQUINA NO OCS
//
// A localizacao ocorre exclusivamente pelo hardware.NAME.
// Nao sao consultados hardware.TAG ou accountinfo.TAG para localizar a
// maquina.
// ============================================================================

    $sqlBuscaOCS = "
        SELECT
            h.ID,
            h.NAME
        FROM `{$config['ocs_hardware_table']}` h
        WHERE UPPER(TRIM(h.NAME)) = UPPER(TRIM(:hostname))
        LIMIT 1
    ";

    $stmtBuscaOCS = $pdo->prepare($sqlBuscaOCS);


// ============================================================================
// [11] VERIFICACAO DO ACCOUNTINFO
// ============================================================================

    $sqlBuscaAccountInfo = "
        SELECT
            HARDWARE_ID,
            TAG
        FROM `{$config['ocs_accountinfo_table']}`
        WHERE HARDWARE_ID = :hardware_id
        LIMIT 1
    ";

    $stmtBuscaAccountInfo = $pdo->prepare(
        $sqlBuscaAccountInfo
    );


// ============================================================================
// [12] ATUALIZACAO DO TAG
//
// ESTE E O UNICO UPDATE REALIZADO NO OCS.
//
// Nao alteramos:
//   hardware.NAME
//   hardware.USERID
//   hardware.TAG
// ============================================================================

    $sqlUpdateAccountInfo = "
        UPDATE `{$config['ocs_accountinfo_table']}`
        SET `TAG` = :tag
        WHERE `HARDWARE_ID` = :hardware_id
    ";

    $stmtUpdateAccountInfo = $pdo->prepare(
        $sqlUpdateAccountInfo
    );


// ============================================================================
// [13] MARCACAO DO CADASTRO COMO SINCRONIZADO
// ============================================================================

    $sqlUpdateCadastro = "
        UPDATE `{$config['cadastro_table']}`
        SET
            `sincronizado_ocs` = 1,
            `data_sincronizacao` = NOW()
        WHERE `id` = :id
    ";

    $stmtUpdateCadastro = $pdo->prepare(
        $sqlUpdateCadastro
    );


// ============================================================================
// [14] PROCESSAMENTO DOS CADASTROS
// ============================================================================

    foreach ($cadastros as $item) {

        $idCadastro = $item['id'];

        $hostnameOrig = trim(
            $item['hostname']
        );

        $patrimonio = trim(
            $item['numero_patrimonio']
        );


        // --------------------------------------------------------------------
        // Validacao basica
        // --------------------------------------------------------------------

        if ($hostnameOrig === '') {

            logMessage(
                "[ERRO] Cadastro #$idCadastro possui hostname vazio.",
                $logFile
            );

            $contErros++;

            continue;
        }

        if ($patrimonio === '') {

            logMessage(
                "[ERRO] Cadastro #$idCadastro / Hostname '$hostnameOrig' possui patrimonio vazio.",
                $logFile
            );

            $contErros++;

            continue;
        }


        // --------------------------------------------------------------------
        // Determina a TAG dinamicamente a partir dos 3 primeiros caracteres
        // do hostname.
        //
        // PAC -> PACO-{PATRIMONIO}
        // PLA -> VIC-{PATRIMONIO}
        // DES -> VIC-{PATRIMONIO}
        // FAZ -> VIC-{PATRIMONIO}
        // Outros -> LOCAL-{PATRIMONIO}
        // --------------------------------------------------------------------

        $prefixo = strtoupper(
            substr($hostnameOrig, 0, 3)
        );

        switch ($prefixo) {

            case 'PAC':
                $tag = 'PACO-' . $patrimonio;
                break;

            case 'PLA':
            case 'DES':
            case 'FAZ':
                $tag = 'VIC-' . $patrimonio;
                break;

            default:
                $tag = 'LOCAL-' . $patrimonio;
                break;
        }


        // --------------------------------------------------------------------
        // Localiza a maquina no OCS pelo hostname
        // --------------------------------------------------------------------

        $stmtBuscaOCS->execute([
            ':hostname' => $hostnameOrig
        ]);

        $ocsMachine = $stmtBuscaOCS->fetch();


        // --------------------------------------------------------------------
        // Maquina ainda nao encontrada
        // --------------------------------------------------------------------

        if (!$ocsMachine) {

            logMessage(
                "[PENDENTE] Hostname '$hostnameOrig' (TAG calculada: '$tag') ainda nao foi localizado no OCS.",
                $logFile
            );

            $contNaoLocalizados++;

            continue;
        }


        // --------------------------------------------------------------------
        // HARDWARE_ID encontrado
        // --------------------------------------------------------------------

        $ocsId = $ocsMachine['ID'];

        $ocsNomeAtual = $ocsMachine['NAME'];


        if ($isVerbose) {

            logMessage(
                "[LOCALIZADO] Hostname '$hostnameOrig' -> OCS HARDWARE_ID #$ocsId.",
                $logFile
            );

            logMessage(
                "[TAG] Prefixo '$prefixo' -> TAG calculada '$tag'.",
                $logFile
            );
        }


        // --------------------------------------------------------------------
        // Verifica se existe accountinfo para este HARDWARE_ID
        // --------------------------------------------------------------------

        $stmtBuscaAccountInfo->execute([
            ':hardware_id' => $ocsId
        ]);

        $accountInfo = $stmtBuscaAccountInfo->fetch();


        if (!$accountInfo) {

            logMessage(
                "[ERRO] OCS HARDWARE_ID #$ocsId ('$ocsNomeAtual') nao possui registro em accountinfo. TAG '$tag' nao sera atualizado.",
                $logFile
            );

            $contSemAccountInfo++;

            continue;
        }


        // --------------------------------------------------------------------
        // TAG atual
        // --------------------------------------------------------------------

        $tagAtual = trim(
            (string)($accountInfo['TAG'] ?? '')
        );


        // --------------------------------------------------------------------
        // Verifica se o TAG ja esta correto
        // --------------------------------------------------------------------

        if ($tagAtual === $tag) {

            if ($isVerbose) {

                logMessage(
                    "[JA SINCRONIZADO] OCS ID #$ocsId / Hostname '$ocsNomeAtual' ja possui TAG '$tag'.",
                    $logFile
                );
            }

            if (!$isDryRun) {

                $stmtUpdateCadastro->execute([
                    ':id' => $idCadastro
                ]);
            }

            $contJaAtualizados++;

            continue;
        }


        // --------------------------------------------------------------------
        // Atualizacao do accountinfo.TAG
        // --------------------------------------------------------------------

        logMessage(
            "[ATUALIZANDO] OCS ID #$ocsId / Hostname '$ocsNomeAtual': accountinfo.TAG '$tagAtual' -> '$tag'.",
            $logFile
        );


        if (!$isDryRun) {

            $stmtUpdateAccountInfo->execute([
                ':tag'         => $tag,
                ':hardware_id' => $ocsId
            ]);

            $stmtUpdateCadastro->execute([
                ':id' => $idCadastro
            ]);
        }


        $contAtualizados++;
    }


// ============================================================================
// [15] RESUMO
// ============================================================================

    logMessage(
        "------------------------------------------------------------------",
        $logFile
    );

    logMessage(
        "RESUMO DA SINCRONIZACAO:",
        $logFile
    );

    logMessage(
        " - TAGs atualizados em accountinfo: $contAtualizados",
        $logFile
    );

    logMessage(
        " - TAGs ja conformes:               $contJaAtualizados",
        $logFile
    );

    logMessage(
        " - Maquinas nao localizadas:        $contNaoLocalizados",
        $logFile
    );

    logMessage(
        " - Sem registro accountinfo:        $contSemAccountInfo",
        $logFile
    );

    logMessage(
        " - Erros de validacao:              $contErros",
        $logFile
    );

    logMessage(
        "Nenhuma alteracao foi realizada em hardware.NAME, hardware.USERID ou hardware.TAG.",
        $logFile
    );

    logMessage(
        "Sincronizacao concluida.",
        $logFile
    );

    logMessage(
        "==================================================================",
        $logFile
    );


// ============================================================================
// [16] TRATAMENTO DE ERROS
// ============================================================================

} catch (Exception $e) {

    logMessage(
        "[ERRO CRITICO] Falha na execucao da sincronizacao: " .
        $e->getMessage(),
        $logFile
    );

    exit(1);
}

?>
