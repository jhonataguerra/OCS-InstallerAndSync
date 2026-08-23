<?php
/**
 * ============================================================================
 * PROJETO: Inventario e Identificacao de Maquinas OCS
 * ETAPA 2: Endpoint de Ingestao e Cadastro de Computadores (Hardened)
 * ============================================================================
 */

header('Content-Type: application/json; charset=utf-8');
header('X-Content-Type-Options: nosniff');
header('X-Frame-Options: DENY');

// Permite apenas requisicoes POST
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    http_response_code(405);
    echo json_encode([
        'status' => 'error',
        'message' => 'Metodo nao permitido. Utilize POST.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// Carrega configuracoes
$configFile = __DIR__ . '/config.php';
if (!file_exists($configFile)) {
    http_response_code(500);
    echo json_encode([
        'status' => 'error',
        'message' => 'Configuracao do servidor indisponivel.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$config = require $configFile;

// Limitador de tamanho do payload (Max 10 KB para prevenir DoS / buffer overflow)
$contentLength = (int)($_SERVER['CONTENT_LENGTH'] ?? 0);
if ($contentLength > 10240) {
    http_response_code(413);
    echo json_encode([
        'status' => 'error',
        'message' => 'Payload muito grande.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// Leitura do payload
$rawBody = file_get_contents('php://input');
$inputData = json_decode($rawBody, true);

if (!is_array($inputData) || empty($inputData)) {
    $inputData = $_POST;
}

// [SEC-02] Validacao de Token de Seguranca Obrigatorio
$expectedToken = $config['api_token'] ?? '';
$clientToken = $_SERVER['HTTP_X_API_TOKEN'] ?? ($inputData['api_token'] ?? '');

if (empty($expectedToken) || !hash_equals($expectedToken, (string)$clientToken)) {
    http_response_code(401);
    echo json_encode([
        'status' => 'error',
        'message' => 'Acesso nao autorizado. Token de seguranca invalido ou ausente.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// Funcao auxiliar para sanitizacao rigorosa de strings (Prevenção de XSS e caracteres nulos)
function sanitizeInput($str, $maxLength = 150) {
    if ($str === null) return '';
    $clean = trim((string)$str);
    // Remove caracteres de controle nulos e tags HTML/JavaScript
    $clean = str_replace(chr(0), '', $clean);
    $clean = strip_tags($clean);
    $clean = htmlspecialchars($clean, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
    return mb_substr($clean, 0, $maxLength, 'UTF-8');
}

// [SEC-04] Extracao e higienizacao dos campos
$hostname         = sanitizeInput($inputData['hostname'] ?? '', 100);
$nomeCompleto     = sanitizeInput($inputData['nome_completo'] ?? '', 150);
$numeroPatrimonio = preg_replace('/[^\d]/', '', (string)($inputData['numero_patrimonio'] ?? ''));
$setorLocal       = sanitizeInput($inputData['setor_local'] ?? '', 100);

$usuarioWindows   = sanitizeInput($inputData['usuario_windows'] ?? '', 100);
$versaoWindows    = sanitizeInput($inputData['versao_windows'] ?? '', 150);
$arquitetura      = sanitizeInput($inputData['arquitetura'] ?? '', 20);
$serialBios       = sanitizeInput($inputData['serial_bios'] ?? '', 100);
$ipOrigem         = filter_var($_SERVER['REMOTE_ADDR'] ?? '', FILTER_VALIDATE_IP) ?: '0.0.0.0';

// Validacao dos campos minimos
if (empty($hostname) || empty($nomeCompleto) || empty($numeroPatrimonio) || empty($setorLocal)) {
    http_response_code(400);
    echo json_encode([
        'status' => 'error',
        'message' => 'Campos obrigatorios invalidos ou ausentes (hostname, nome, patrimonio numerico, setor).'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

// Conexao com MySQL / MariaDB via PDO
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

    // Query de Insercao / Atualizacao parametrizada contra SQL Injection
    $sql = "INSERT INTO `computadores_cadastro` 
            (`hostname`, `nome_completo`, `numero_patrimonio`, `setor_local`, `usuario_windows`, `versao_windows`, `arquitetura`, `serial_bios`, `ip_origem`, `sincronizado_ocs`)
            VALUES 
            (:hostname, :nome_completo, :numero_patrimonio, :setor_local, :usuario_windows, :versao_windows, :arquitetura, :serial_bios, :ip_origem, 0)
            ON DUPLICATE KEY UPDATE
                `nome_completo`     = VALUES(`nome_completo`),
                `numero_patrimonio` = VALUES(`numero_patrimonio`),
                `setor_local`       = VALUES(`setor_local`),
                `usuario_windows`   = VALUES(`usuario_windows`),
                `versao_windows`    = VALUES(`versao_windows`),
                `arquitetura`       = VALUES(`arquitetura`),
                `serial_bios`       = VALUES(`serial_bios`),
                `ip_origem`         = VALUES(`ip_origem`),
                `sincronizado_ocs`  = 0,
                `data_atualizacao`  = CURRENT_TIMESTAMP";

    $stmt = $pdo->prepare($sql);
    $stmt->execute([
        ':hostname'         => $hostname,
        ':nome_completo'     => $nomeCompleto,
        ':numero_patrimonio' => $numeroPatrimonio,
        ':setor_local'       => $setorLocal,
        ':usuario_windows'   => $usuarioWindows,
        ':versao_windows'    => $versaoWindows,
        ':arquitetura'       => $arquitetura,
        ':serial_bios'       => $serialBios,
        ':ip_origem'         => $ipOrigem
    ]);

    // Resposta de sucesso (HTTP 200 OK)
    http_response_code(200);
    echo json_encode([
        'status' => 'success',
        'message' => 'Cadastro gravado com sucesso.',
        'data' => [
            'hostname' => $hostname,
            'numero_patrimonio' => $numeroPatrimonio
        ]
    ], JSON_UNESCAPED_UNICODE);

} catch (PDOException $e) {
    // [SEC-03] Oculta mensagens brutas do banco de dados e grava em log de auditoria seguro
    $errorMsg = sprintf("[%s] [DB_ERROR] IP: %s | Hostname: %s | Msg: %s\n", date('Y-m-d H:i:s'), $ipOrigem, $hostname, $e->getMessage());
    $logPath = $config['error_log'] ?? '/var/log/ocs_cadastro_api_error.log';
    @file_put_contents($logPath, $errorMsg, FILE_APPEND);

    http_response_code(500);
    echo json_encode([
        'status' => 'error',
        'message' => 'Erro interno ao processar cadastro no servidor.'
    ], JSON_UNESCAPED_UNICODE);
}
