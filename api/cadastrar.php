<?php
/**
 * ============================================================================
 * PROJETO: Inventario e Identificacao de Maquinas OCS
 * ETAPA 2: Endpoint de Ingestao e Cadastro de Computadores
 * ============================================================================
 */

header('Content-Type: application/json; charset=utf-8');

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
        'message' => 'Arquivo de configuracao do banco nao encontrado.'
    ], JSON_UNESCAPED_UNICODE);
    exit;
}

$config = require $configFile;

// Leitura do payload (suporta JSON raw no body ou FormData tradicional)
$rawBody = file_get_contents('php://input');
$inputData = json_decode($rawBody, true);

if (!is_array($inputData) || empty($inputData)) {
    $inputData = $_POST;
}

// Validacao de Token de Seguranca (caso configurado)
if (!empty($config['api_token'])) {
    $clientToken = isset($_SERVER['HTTP_X_API_TOKEN']) ? $_SERVER['HTTP_X_API_TOKEN'] : ($inputData['api_token'] ?? '');
    if ($clientToken !== $config['api_token']) {
        http_response_code(401);
        echo json_encode([
            'status' => 'error',
            'message' => 'Token de autenticacao invalido.'
        ], JSON_UNESCAPED_UNICODE);
        exit;
    }
}

// Extracao e higienizacao dos campos obrigatorios
$hostname         = trim($inputData['hostname'] ?? '');
$nomeCompleto     = trim($inputData['nome_completo'] ?? '');
$numeroPatrimonio = trim($inputData['numero_patrimonio'] ?? '');
$setorLocal       = trim($inputData['setor_local'] ?? '');

// Extracao dos campos opcionais / coletados automaticamente
$usuarioWindows   = trim($inputData['usuario_windows'] ?? '');
$versaoWindows    = trim($inputData['versao_windows'] ?? '');
$arquitetura      = trim($inputData['arquitetura'] ?? '');
$serialBios       = trim($inputData['serial_bios'] ?? '');
$ipOrigem         = $_SERVER['REMOTE_ADDR'] ?? '';

// Validacao dos campos minimos
if (empty($hostname) || empty($nomeCompleto) || empty($numeroPatrimonio) || empty($setorLocal)) {
    http_response_code(400);
    echo json_encode([
        'status' => 'error',
        'message' => 'Campos obrigatorios ausentes: hostname, nome_completo, numero_patrimonio, setor_local.'
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

    // Query de Insercao com atualizacao em caso de duplicidade de Hostname (Idempotencia)
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
        'message' => 'Cadastro gravado com sucesso no banco de dados.',
        'data' => [
            'hostname' => $hostname,
            'numero_patrimonio' => $numeroPatrimonio
        ]
    ], JSON_UNESCAPED_UNICODE);

} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode([
        'status' => 'error',
        'message' => 'Erro ao conectar ou gravar no banco de dados: ' . $e->getMessage()
    ], JSON_UNESCAPED_UNICODE);
}
