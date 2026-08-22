#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
============================================================================
PROJETO: Inventario e Identificacao de Maquinas OCS
ETAPA 3: Script de Sincronizacao em Python (Hostname -> Hostname-Patrimonio)
============================================================================

Uso:
    python3 sync_ocs_patrimonio.py
    python3 sync_ocs_patrimonio.py --dry-run
    python3 sync_ocs_patrimonio.py --force
"""

import sys
import os
import argparse
from datetime import datetime

# Configuracoes de conexao
DB_CONFIG = {
    'host': '127.0.0.1',
    'port': 3306,
    'user': 'ocs',
    'password': 'ocs',
    'database': 'ocsweb',
    'charset': 'utf8mb4'
}

CADASTRO_TABLE = 'computadores_cadastro'
HARDWARE_TABLE = 'hardware'
ACCOUNTINFO_TABLE = 'accountinfo'
LOG_FILE = '/var/log/ocs_sync_patrimonio.log'

def log(msg):
    timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
    line = f"[{timestamp}] {msg}"
    print(line)
    if LOG_FILE:
        try:
            with open(LOG_FILE, 'a', encoding='utf-8') as f:
                f.write(line + '\n')
        except Exception:
            pass

def main():
    parser = argparse.ArgumentParser(description="Sincronizacao OCS Server com Cadastro de Patrimonio")
    parser.add_argument('--dry-run', action='store_true', help="Simula as operacoes sem gravar no banco")
    parser.add_argument('--force', action='store_true', help="Processa todos os registros mesmo ja marcados")
    args = parser.parse_args()

    # Importacao condicional do conector MySQL
    try:
        import pymysql as mysql_driver
    except ImportError:
        try:
            import mysql.connector as mysql_driver
        except ImportError:
            log("[ERRO] Instale um driver MySQL para Python: 'pip install pymysql' ou 'apt install python3-pymysql'")
            sys.exit(1)

    log("==================================================================")
    log("INICIANDO SINCRONIZACAO OCS (PYTHON)")
    if args.dry_run:
        log("[MODO SIMULACAO ATIVADO - Nenhuma gravacao sera efetuada]")

    try:
        conn = mysql_driver.connect(**DB_CONFIG)
        cursor = conn.cursor(dictionary=True) if hasattr(mysql_driver, 'connector') else conn.cursor(mysql_driver.cursors.DictCursor)

        sql_cadastros = f"SELECT id, hostname, numero_patrimonio, nome_completo, usuario_windows, sincronizado_ocs FROM {CADASTRO_TABLE}"
        if not args.force:
            sql_cadastros += " WHERE sincronizado_ocs = 0"

        cursor.execute(sql_cadastros)
        cadastros = cursor.fetchall()

        total = len(cadastros)
        log(f"Registros pendentes de processamento: {total}")

        if total == 0:
            log("Nenhum registro a sincronizar. Finalizando.")
            log("==================================================================")
            conn.close()
            return

        atualizados = 0
        ja_conformes = 0
        pendentes_ocs = 0

        for item in cadastros:
            id_cad = item['id']
            hostname = item['hostname'].strip()
            patrimonio = item['numero_patrimonio'].strip()
            usuario = (item['usuario_windows'] or item['nome_completo']).strip()
            novo_nome = f"{hostname}-{patrimonio}"

            # Busca no OCS
            sql_busca = f"""
                SELECT h.ID, h.NAME, h.TAG, a.TAG AS ACCOUNTINFO_TAG
                FROM {HARDWARE_TABLE} h
                LEFT JOIN {ACCOUNTINFO_TABLE} a ON h.ID = a.HARDWARE_ID
                WHERE UPPER(TRIM(h.NAME)) = UPPER(%s)
                   OR UPPER(TRIM(h.NAME)) = UPPER(%s)
                   OR UPPER(TRIM(h.TAG))  = UPPER(%s)
                   OR UPPER(TRIM(a.TAG))  = UPPER(%s)
                LIMIT 1
            """
            cursor.execute(sql_busca, (hostname, novo_nome, hostname, hostname))
            ocs_row = cursor.fetchone()

            if not ocs_row:
                log(f"[PENDENTE] Hostname '{hostname}' (Patrimonio: {patrimonio}) nao encontrado no OCS ainda.")
                pendentes_ocs += 1
                continue

            ocs_id = ocs_row['ID']
            ocs_nome = ocs_row['NAME']

            if ocs_nome.upper() == novo_nome.upper():
                if not args.dry_run:
                    cursor.execute(f"UPDATE {CADASTRO_TABLE} SET sincronizado_ocs = 1, data_sincronizacao = NOW() WHERE id = %s", (id_cad,))
                    conn.commit()
                ja_conformes += 1
                continue

            log(f"[ATUALIZANDO] OCS ID #{ocs_id}: '{ocs_nome}' -> '{novo_nome}'")

            if not args.dry_run:
                sql_update_hw = f"UPDATE {HARDWARE_TABLE} SET `NAME` = %s, `USERID` = %s WHERE `ID` = %s"
                cursor.execute(sql_update_hw, (novo_nome, usuario, ocs_id))

                sql_update_cad = f"UPDATE {CADASTRO_TABLE} SET sincronizado_ocs = 1, data_sincronizacao = NOW() WHERE id = %s"
                cursor.execute(sql_update_cad, (id_cad,))
                conn.commit()

            atualizados += 1

        log("------------------------------------------------------------------")
        log(f"RESUMO: Atualizados: {atualizados} | Ja Conformes: {ja_conformes} | Pendentes OCS: {pendentes_ocs}")
        log("Sincronizacao concluida.")
        log("==================================================================")
        conn.close()

    except Exception as e:
        log(f"[ERRO CRITICO] Falha na sincronizacao: {e}")
        sys.exit(1)

if __name__ == '__main__':
    main()
