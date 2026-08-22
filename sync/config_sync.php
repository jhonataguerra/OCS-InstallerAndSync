<?php
/**
 * ============================================================================
 * PROJETO: Inventario e Identificacao de Maquinas OCS
 * ETAPA 3: Configuracoes da Sincronizacao OCS <-> Cadastro de Patrimonio
 * ============================================================================
 */

return [
    // Conexao com a base de dados do OCS Inventory (geralmente ocsweb)
    'db_host'    => '127.0.0.1',
    'db_port'    => 3306,
    'db_name'    => 'ocsweb',
    'db_user'    => 'ocs',
    'db_pass'    => 'ocs',
    'db_charset' => 'utf8mb4',

    // Nome da tabela de cadastros criada na Etapa 2
    'cadastro_table' => 'computadores_cadastro',

    // Nome da tabela principal de inventario do OCS
    'ocs_hardware_table' => 'hardware',
    'ocs_accountinfo_table' => 'accountinfo',

    // Formato do novo nome: {HOSTNAME}-{PATRIMONIO}
    // Exemplo: 'PC-FINANCEIRO-01-12345'
    'name_separator' => '-',

    // Se true, atualiza tambem o campo USERID no OCS com o nome do usuario cadastrado
    'update_userid' => true,

    // Se true, forca a atualizacao mesmo para registros previamente marcados
    'force_recheck_all' => false,

    // Caminho do arquivo de log da sincronizacao (deixe vazio para apenas saida padrao stdout)
    'log_file' => '/var/log/ocs_sync_patrimonio.log'
];
