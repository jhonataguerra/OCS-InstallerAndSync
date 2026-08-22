-- ============================================================================
-- PROJETO: Inventario e Identificacao de Maquinas OCS
-- ETAPA 2: Schema do Banco de Dados para Coleta e Cadastro de Computadores
-- COMPATIBILIDADE: MySQL 5.5+, MariaDB 10.0+ (InnoDB / UTF-8)
-- ============================================================================

-- Cria a tabela de cadastros caso nao exista
CREATE TABLE IF NOT EXISTS `computadores_cadastro` (
    `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    
    -- Chave principal de identificacao e relacionamento com o OCS Server
    `hostname` VARCHAR(100) NOT NULL,
    
    -- Dados preenchidos interativamente pelo usuario
    `nome_completo` VARCHAR(150) NOT NULL,
    `numero_patrimonio` VARCHAR(50) NOT NULL,
    `setor_local` VARCHAR(100) NOT NULL,
    
    -- Dados coletados automaticamente pelo cliente Windows
    `usuario_windows` VARCHAR(100) DEFAULT NULL,
    `versao_windows` VARCHAR(150) DEFAULT NULL,
    `arquitetura` VARCHAR(20) DEFAULT NULL,     -- '32 bits' ou '64 bits'
    `serial_bios` VARCHAR(100) DEFAULT NULL,
    `ip_origem` VARCHAR(45) DEFAULT NULL,
    
    -- Controle do processo de sincronizacao da Etapa 3
    `sincronizado_ocs` TINYINT(1) NOT NULL DEFAULT 0,
    `data_sincronizacao` DATETIME DEFAULT NULL,
    
    -- Timestamps de auditoria
    `data_cadastro` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `data_atualizacao` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_hostname` (`hostname`),
    KEY `idx_patrimonio` (`numero_patrimonio`),
    KEY `idx_sincronizado` (`sincronizado_ocs`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
