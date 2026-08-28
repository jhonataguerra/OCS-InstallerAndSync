#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
============================================================================
PROJETO : Inventario e Identificacao de Maquinas OCS
SCRIPT  : Orquestrador de Testes + Gate de Validacao de Seguranca
============================================================================

Fluxo de execucao:
  1. Executa a suite de testes automatizados (tests/run_tests.bat)
  2. Avalia o resultado:
     - TODOS os testes passam → gera o Relatorio de Seguranca em PDF
     - QUALQUER falha        → exibe sumario de falhas e DIFERE a seguranca

Uso:
  python scripts/run_tests_and_security.py
  python scripts/run_tests_and_security.py --force-security  (ignora falhas, apenas dev)
"""

import subprocess
import sys
import os
import argparse
from datetime import datetime

# ============================================================================
# CONFIGURAÇÕES
# ============================================================================
REPO_ROOT        = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TEST_RUNNER      = os.path.join(REPO_ROOT, 'tests', 'run_tests.bat')
SECURITY_SCRIPT  = os.path.join(REPO_ROOT, 'scripts', 'generate_security_report_pdf.py')
SEPARATOR        = '=' * 68


def banner(text: str, char: str = '=') -> None:
    print(f'\n{char * 68}')
    print(f'  {text}')
    print(f'{char * 68}')


def timestamp() -> str:
    return datetime.now().strftime('%Y-%m-%d %H:%M:%S')


# ============================================================================
# ETAPA 1 — EXECUÇÃO DOS TESTES
# ============================================================================
def run_test_suite() -> tuple[int, str, str]:
    """
    Executa run_tests.bat e retorna (exit_code, stdout, stderr).
    """
    banner('ETAPA 1 / 2 — Suite de Testes Automatizados')
    print(f'  Iniciado em  : {timestamp()}')
    print(f'  Runner       : {TEST_RUNNER}')
    print()

    if not os.path.isfile(TEST_RUNNER):
        print(f'[ERRO FATAL] Runner de testes nao encontrado: {TEST_RUNNER}')
        sys.exit(2)

    result = subprocess.run(
        ['cmd.exe', '/c', TEST_RUNNER],
        cwd=REPO_ROOT,
        capture_output=False,   # Exibe output em tempo real
        text=True,
        encoding='utf-8',
        errors='replace'
    )

    return result.returncode


# ============================================================================
# ETAPA 2 — GATE DE SEGURANÇA
# ============================================================================
def run_security_gate(tests_exit_code: int, force: bool) -> None:
    """
    Se todos os testes passam (exit_code == 0), gera o relatorio de seguranca.
    Caso contrario, difere a validacao com mensagem clara.
    """
    banner('ETAPA 2 / 2 — Gate de Validacao de Seguranca')

    if tests_exit_code == 0 or force:
        if force and tests_exit_code != 0:
            print('  [AVISO] --force-security ativo: ignorando falhas de teste.')
            print('          NAO use em producao ou em PRs!\n')

        print('  [OK] Todos os testes passaram.')
        print('  [->] Iniciando geracao do Relatorio de Seguranca em PDF...\n')

        if not os.path.isfile(SECURITY_SCRIPT):
            print(f'  [ERRO] Script de seguranca nao encontrado: {SECURITY_SCRIPT}')
            sys.exit(3)

        sec_result = subprocess.run(
            [sys.executable, SECURITY_SCRIPT],
            cwd=REPO_ROOT,
            text=True,
            encoding='utf-8',
            errors='replace'
        )

        print()
        if sec_result.returncode == 0:
            banner('RESULTADO FINAL — SUCESSO COMPLETO', char='-')
            print('  [OK] Suite de testes  : APROVADA')
            print('  [OK] Relatorio PDF    : GERADO')
            print(f'  Concluido em          : {timestamp()}')
            print()
            sys.exit(0)
        else:
            banner('RESULTADO FINAL — FALHA NA GERACAO DO PDF', char='-')
            print('  [OK]    Suite de testes  : APROVADA')
            print('  [FALHA] Relatorio PDF    : ERRO NA GERACAO')
            print('          Verifique se a dependencia "reportlab" esta instalada:')
            print('          pip install reportlab')
            print()
            sys.exit(4)

    else:
        # ----------------------------------------------------------------
        # Testes falharam — DIFERE a validacao de seguranca
        # ----------------------------------------------------------------
        banner('RESULTADO FINAL — VALIDACAO DE SEGURANCA DIFERIDA', char='!')
        print()
        print('  [!] GATE DE SEGURANCA NAO EXECUTADO')
        print()
        print('  Motivo   : A suite de testes retornou falhas.')
        print('  Acao     : Corrija os testes listados acima e execute novamente.')
        print()
        print('  Proximos passos:')
        print('    1. Analise as falhas reportadas acima.')
        print('    2. Corrija o(s) problema(s) em install_ocs_agent.bat ou no ambiente.')
        print('    3. Execute novamente:')
        print('       python scripts/run_tests_and_security.py')
        print()
        print('  Referencia rapida para debug:')
        print(f'    Log de instalacao : %SystemRoot%\\Temp\\ocs_agent_install.log')
        print(f'    Suite de testes   : tests\\test_install_agent.ps1')
        print(f'    Runner de testes  : tests\\run_tests.bat')
        print()
        print(f'  Diferido em : {timestamp()}')
        print()
        sys.exit(1)


# ============================================================================
# PONTO DE ENTRADA
# ============================================================================
def main() -> None:
    parser = argparse.ArgumentParser(
        description='Orquestrador de Testes + Gate de Seguranca — OCS InstallerAndSync'
    )
    parser.add_argument(
        '--force-security',
        action='store_true',
        help='[DEV ONLY] Gera o relatorio mesmo com testes falhando. NAO usar em producao.'
    )
    args = parser.parse_args()

    banner('OCS InstallerAndSync — Orquestrador de Qualidade e Seguranca')
    print(f'  Repositorio : {REPO_ROOT}')
    print(f'  Iniciado em : {timestamp()}')

    # Etapa 1: Testes
    exit_code = run_test_suite()

    # Etapa 2: Gate de Segurança
    run_security_gate(exit_code, force=args.force_security)


if __name__ == '__main__':
    main()
