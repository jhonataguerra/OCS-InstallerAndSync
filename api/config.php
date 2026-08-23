<?php
/**
 * Configurações de Conexão com o Banco de Dados MySQL / MariaDB
 * Pode utilizar o mesmo banco do OCS (ex: ocsweb) ou uma base dedicada.
 */

return [
    'db_host' => '192.168.15.20',
    'db_port' => 3306,
    'db_name' => 'ocsweb',
    'db_user' => 'ocs',
    'db_pass' => '123456',
    'db_charset' => 'utf8mb4',
    
    // Token de segurança opcional (se preenchido, a aplicação EXE enviará no cabeçalho ou payload)
    'api_token' => ''
];
