<?php
/**
 * Configurações de Conexão com o Banco de Dados MySQL / MariaDB
 * e Parâmetros de Segurança da API.
 */

return [
    'db_host'    => '127.0.0.1',
    'db_port'    => 3306,
    'db_name'    => 'ocsweb',
    'db_user'    => 'ocs',
    'db_pass'    => 'ocs',
    'db_charset' => 'utf8mb4',
    
    // Token de segurança obrigatório para comunicação entre Cliente e API (SEC-02)
    'api_token'  => 'OCS_SEC_TOKEN_8f93e1b742a0489c93df51e7b99c2d15',

    // Caminho do log seguro de erros da API (SEC-03)
    'error_log'  => '/var/log/ocs_cadastro_api_error.log'
];
