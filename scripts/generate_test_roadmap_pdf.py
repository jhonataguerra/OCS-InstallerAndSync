#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Roteiro de Testes de Homologacao - OCS Inventory (Etapas 1-3)
Gera PDF em docs/roteiro_testes_homologacao.pdf
Execucao normal (sem GPO), matriz Win7 32/64, Win10, Win11
"""
import os
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, HRFlowable, KeepTogether, PageBreak
)
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfgen import canvas

class NumberedCanvas(canvas.Canvas):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._saved_page_states = []
    def showPage(self):
        self._saved_page_states.append(dict(self.__dict__))
        self._startPage()
    def save(self):
        num_pages = len(self._saved_page_states)
        for state in self._saved_page_states:
            self.__dict__.update(state)
            self.draw_page_decorations(num_pages)
            super().showPage()
        super().save()
    def draw_page_decorations(self, page_count):
        self.saveState()
        self.setFont("Helvetica", 7.5)
        self.setFillColor(colors.HexColor("#718096"))
        if self._pageNumber > 1:
            self.drawString(40, 810, "OCS INVENTORY — ROTEIRO DE TESTES DE HOMOLOGACAO (EXECUCAO NORMAL, SEM GPO)")
            self.setStrokeColor(colors.HexColor("#CBD5E0"))
            self.setLineWidth(0.4)
            self.line(40, 804, 555, 804)
        self.drawRightString(555, 30, f"Pagina {self._pageNumber} de {page_count}")
        self.drawString(40, 30, "CONFIDENCIAL — TI & HOMOLOGACAO | OCS Server 192.168.2.48")
        self.setStrokeColor(colors.HexColor("#CBD5E0"))
        self.line(40, 42, 555, 42)
        self.restoreState()

def build_pdf(filename="docs/roteiro_testes_homologacao.pdf"):
    base = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out = os.path.join(base, filename)
    os.makedirs(os.path.dirname(out), exist_ok=True)

    doc = SimpleDocTemplate(out, pagesize=A4, leftMargin=38, rightMargin=38, topMargin=45, bottomMargin=50)
    styles = getSampleStyleSheet()
    C_NAVY = colors.HexColor("#1A365D")
    C_BLUE = colors.HexColor("#2B6CB0")
    C_LIGHT = colors.HexColor("#F7FAFC")
    C_BORDER = colors.HexColor("#E2E8F0")
    C_GREEN = colors.HexColor("#276749")
    C_AMBER = colors.HexColor("#975A16")
    C_TEXT = colors.HexColor("#2D3748")

    sTitle = ParagraphStyle('Title', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=18, leading=22, textColor=C_NAVY, spaceAfter=2)
    sSub = ParagraphStyle('Sub', parent=styles['Normal'], fontName='Helvetica', fontSize=10, leading=14, textColor=colors.HexColor("#4A5568"), spaceAfter=10)
    sH1 = ParagraphStyle('H1', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=12.5, leading=16, textColor=C_NAVY, spaceBefore=14, spaceAfter=5, keepWithNext=True)
    sH2 = ParagraphStyle('H2', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=10.5, leading=14, textColor=C_BLUE, spaceBefore=10, spaceAfter=4, keepWithNext=True)
    sBody = ParagraphStyle('Body', parent=styles['Normal'], fontName='Helvetica', fontSize=9, leading=13, textColor=C_TEXT, spaceAfter=5)
    sBullet = ParagraphStyle('Bullet', parent=sBody, leftIndent=14, bulletIndent=0, spaceAfter=3)
    sCode = ParagraphStyle('Code', parent=styles['Code'], fontName='Courier', fontSize=7.8, leading=10.5, textColor=colors.HexColor("#1A202C"), backColor=colors.HexColor("#EDF2F7"), borderPadding=(4,5,4), spaceAfter=6)
    sCell = ParagraphStyle('Cell', parent=styles['Normal'], fontName='Helvetica', fontSize=8, leading=10.5, textColor=C_TEXT)
    sCellH = ParagraphStyle('CellH', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=8, leading=10.5, textColor=colors.white, alignment=1)
    sCellSmall = ParagraphStyle('CellSmall', parent=sCell, fontSize=7.5, leading=9.5)
    sBadge = ParagraphStyle('Badge', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=7, leading=8, alignment=1, textColor=colors.white)

    story = []

    # COVER
    story.append(Paragraph("ROTEIRO DE TESTES DE HOMOLOGACAO", sTitle))
    story.append(Paragraph("Sistema Integrado de Inventario OCS — Etapas 1, 2 e 3<br/>Execucao Normal em Terminal (Sem GPO) — Matriz Win7 32 / Win7 64 / Win10 / Win11", sSub))
    story.append(HRFlowable(width="100%", thickness=1.4, color=C_BLUE, spaceBefore=2, spaceAfter=10))

    # Info box
    info_html = """<b>Servidor de Homologacao:</b> OCS Server ja configurado em <b>http://192.168.2.48/ocsinventory</b> &nbsp;|&nbsp; <b>API:</b> <font face="Courier">http://192.168.2.48/cadastro_api/cadastrar.php</font><br/>
    <b>Banco:</b> ocsweb (MySQL/MariaDB) &nbsp;|&nbsp; <b>Token:</b> <font face="Courier">OCS_SEC_TOKEN_8f93e1b742a0489c93df51e7b99c2d15</font> (<font face="Courier">api/config.php</font> = <font face="Courier">client_app/AppConfig.cs</font>)<br/>
    <b>Modo de teste:</b> <b>Nao sera testado via GPO.</b> Todas as etapas sao por execucao manual com duplo-clique / CMD como Administrador, simulando o comportamento da GPO de forma controlada."""
    tInfo = Table([[Paragraph(info_html, sBody)]], colWidths=[519])
    tInfo.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,-1),C_LIGHT),('BOX',(0,0),(-1,-1),0.7,C_BORDER),('LEFTPADDING',(0,0),(-1,-1),8),('RIGHTPADDING',(0,0),(-1,-1),8),('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7)]))
    story.append(tInfo)
    story.append(Spacer(1,8))

    # Sumario
    story.append(Paragraph("Como usar este roteiro", sH2))
    story.append(Paragraph("Execute na ordem 0 → 1 → 2 → 3. Repita os blocos marcados com <b>◉ Repetir por SO</b> em cada um dos 4 terminais da matriz. Marque <b>OK / FALHA</b> na Checklist Final (pag. final). Logs e evidencias devem ser coletados para aceite.", sBody))
    story.append(Paragraph("Requisitos minimos dos terminais: VM ou fisico com rede para 192.168.2.48, usuario Administrador local, .NET 3.5 ativo (Win7 ja possui; Win10/11: Ativar Recursos do Windows → .NET Framework 3.5), sem OCS Agent previamente instalado.", sBody))

    # ETAPA 0
    story.append(Paragraph("ETAPA 0 — Sanidade Automatizada (na estacao de dev)", sH1))
    story.append(Paragraph("Valida o instalador <font face=\"Courier\">scripts/install_ocs_agent.bat</font> sem tocar no servidor. Deve passar 8/8 antes de ir aos terminais.", sBody))
    story.append(Paragraph("<font face=\"Courier\">cd D:\\DEV\\GitHub\\OCS1<br/>tests\\run_tests.bat<br/>:: ou: python scripts\\run_tests_and_security.py</font>", sCode))
    story.append(Paragraph("Esperado: <b>T-01..T-03</b> deteccao x64/x86/WOW64, <b>T-04/T-05</b> idempotencia, <b>T-06</b> erro critico se instalador ausente, <b>T-07/T-08</b> log com COMPUTERNAME. Exit 0.", sBody))

    # MATRIZ
    story.append(Paragraph("Matriz de Terminais ◉ Repetir por SO", sH1))
    headers = [Paragraph("<b>Terminal</b>", sCellH), Paragraph("<b>SO</b>", sCellH), Paragraph("<b>Arch</b>", sCellH), Paragraph("<b>Obs. Execucao Normal</b>", sCellH), Paragraph("<b>Checkpoint</b>", sCellH)]
    rows = [
        [Paragraph("T-W7-32", sCell), Paragraph("Windows 7 SP1 32-bit", sCell), Paragraph("x86", sCell), Paragraph("Rodar <font face=\"Courier\">.bat</font> e <font face=\"Courier\">.exe</font> como Administrador; .NET 3.5 nativo", sCellSmall), Paragraph("Log x86, Serial BIOS", sCellSmall)],
        [Paragraph("T-W7-64", sCell), Paragraph("Windows 7 SP1 64-bit", sCell), Paragraph("x64", sCell), Paragraph("Mesmo que acima; valida WOW64 e ProgramFiles(x86)", sCellSmall), Paragraph("Log x64", sCellSmall)],
        [Paragraph("T-W10", sCell), Paragraph("Windows 10 22H2 64-bit", sCell), Paragraph("x64", sCell), Paragraph("Ativar .NET 3.5 se necessario; UAC asInvoker", sCellSmall), Paragraph("Form + API OK", sCellSmall)],
        [Paragraph("T-W11", sCell), Paragraph("Windows 11 64-bit", sCell), Paragraph("x64", sCell), Paragraph("Idem W10; valida manifest Win11", sCellSmall), Paragraph("TAG VIC/PACO/LOCAL", sCellSmall)],
    ]
    tMat = Table([headers]+rows, colWidths=[62,128,48,158,123])
    tMat.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),C_NAVY),('VALIGN',(0,0),(-1,-1),'MIDDLE'),('GRID',(0,0),(-1,-1),0.5,C_BORDER),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white, C_LIGHT]),('TOPPADDING',(0,0),(-1,-1),4),('BOTTOMPADDING',(0,0),(-1,-1),4),('LEFTPADDING',(0,0),(-1,-1),5),('RIGHTPADDING',(0,0),(-1,-1),5)]))
    story.append(tMat)
    story.append(Spacer(1,4))
    story.append(Paragraph("Dica: use hostnames com prefixos distintos para validar a regra da TAG: <b>PAC-</b> (ex: PAC-W7-32), <b>PLA/DES/FAZ</b> (ex: PLA-W10-01) e <b>outros</b> (ex: W11-TESTE). Assim testa PACO / VIC / LOCAL em uma rodada.", sBody))

    # ETAPA 1
    story.append(Paragraph("ETAPA 1 — Instalacao do OCS Agent (Execucao Normal, sem GPO)", sH1))
    story.append(Paragraph("<b>◉ Repetir por SO</b> &nbsp;|&nbsp; Tempo: ~3 min por terminal", sBody))
    story.append(Paragraph("1) Preparar pacote", sH2))
    story.append(Paragraph("Gere (ou copie do servidor) os dois exes do Packager com <font face=\"Courier\">utils/Parametros Packager.txt</font> apontando para <font face=\"Courier\">http://192.168.2.48/ocsinventory</font> e <font face=\"Courier\">/TAG=%COMPUTERNAME%</font>: <font face=\"Courier\">OCS-Agent-2.11-x86.exe</font> e <font face=\"Courier\">OCS-Agent-2.11-x64.exe</font>. Deixe os tres arquivos na mesma pasta no terminal: <font face=\"Courier\">install_ocs_agent.bat</font> + 2 exes.", sBody))
    story.append(Paragraph("2) Executar (como Administrador)", sH2))
    story.append(Paragraph("<font face=\"Courier\">:: CMD como Administrador na pasta do teste<br/>install_ocs_agent.bat<br/>:: ou duplo-clique > Executar como administrador</font>", sCode))
    story.append(Paragraph("3) Validar no terminal", sH2))
    story.append(Paragraph("<font face=\"Courier\">sc query \"OCS Inventory Service\"  &nbsp; :: STATE RUNNING<br/>dir \"%ProgramFiles%\\OCS Inventory Agent\\OCSInventory.exe\"<br/>dir \"%ProgramFiles(x86)%\\OCS Inventory Agent\\OCSInventory.exe\"<br/>type C:\\Windows\\Temp\\ocs_agent_install.log</font>", sCode))
    checks1 = [
        "Log contem <b>COMPUTERNAME</b> e <b>Arquitetura detectada: x64/x86</b> + instalador correto selecionado",
        "Servico <b>OCS Inventory Service</b> existe; <b>exit 0</b>",
        "Execute novamente o .bat → log <b>ja esta instalado / Nenhuma acao necessaria</b> (idempotencia)",
        "Forcar teste de erro: renomeie os 2 exes e rode → <b>exit 1</b> + <b>ERRO CRITICO</b> no log (depois restaure)",
    ]
    for c in checks1:
        story.append(Paragraph(f"• {c}", sBullet))
    story.append(Paragraph("4) Validar no OCS Server (2 a 5 min apos execução)", sH2))
    story.append(Paragraph("<font face=\"Courier\">mysql -u ocs -p ocsweb -e \"SELECT ID,NAME,TAG,USERID FROM hardware WHERE NAME='SEU_HOSTNAME' LIMIT 1\"<br/>mysql -u ocs -p ocsweb -e \"SELECT HARDWARE_ID,TAG FROM accountinfo WHERE HARDWARE_ID=(SELECT ID FROM hardware WHERE NAME='SEU_HOSTNAME')\"</font>", sCode))
    story.append(Paragraph("Esperado: <b>hardware.NAME = hostname</b>, <b>hardware.TAG = hostname</b> e <b>accountinfo.TAG = hostname</b> (antes do sync). Histórico de hardware/software preservado.", sBody))

    # ETAPA 2
    story.append(Paragraph("ETAPA 2 — Backend (API + Banco) e Aplicativo de Cadastro", sH1))
    story.append(Paragraph("ETAPA 2A — Banco e API (uma vez no servidor, antes dos terminais)", sH2))
    story.append(Paragraph("<font face=\"Courier\">mysql -u root -p ocsweb &lt; database/schema.sql<br/>mkdir -p /var/www/html/cadastro_api && cp api/config.php api/cadastrar.php /var/www/html/cadastro_api/<br/>chown -R www-data:www-data /var/www/html/cadastro_api<br/>nano /var/www/html/cadastro_api/config.php  # ajuste db_pass + api_token</font>", sCode))
    story.append(Paragraph("<font face=\"Courier\">curl -X POST http://192.168.2.48/cadastro_api/cadastrar.php \\<br/>&nbsp;&nbsp;-H \"X-API-TOKEN: OCS_SEC_TOKEN_8f93e1b742a0489c93df51e7b99c2d15\" -H \"Content-Type: application/json\" \\<br/>&nbsp;&nbsp;-d '{\"hostname\":\"VM-TESTE-01\",\"nome_completo\":\"Joao Teste\",\"numero_patrimonio\":\"99999\",\"setor_local\":\"TI\",\"usuario_windows\":\"DOM\\\\joao\",\"versao_windows\":\"Windows 10 Pro\",\"arquitetura\":\"64 bits\",\"serial_bios\":\"SN12345678\"}'</font>", sCode))
    story.append(Paragraph("Esperado: <b>HTTP 200</b> <font face=\"Courier\">{\"status\":\"success\"}</font>. Sem token → <b>401</b>; payload >10KB → <b>413</b>. Confira <font face=\"Courier\">/var/log/ocs_cadastro_api_error.log</font> (sem leak) e <font face=\"Courier\">SELECT * FROM computadores_cadastro WHERE hostname='VM-TESTE-01'</font> com <b>ON DUPLICATE KEY UPDATE</b> sem duplicar.", sBody))

    story.append(Paragraph("ETAPA 2B — Aplicativo CadastroPatrimonio.exe ◉ Repetir por SO", sH2))
    story.append(Paragraph("Pre-requisito: <font face=\"Courier\">client_app/AppConfig.cs</font> ja com <font face=\"Courier\">http://192.168.2.48/cadastro_api/cadastrar.php</font>. Recompile: <font face=\"Courier\">cd client_app && build.bat</font> (usa <font face=\"Courier\">csc.exe</font> 3.5). Copie só o <font face=\"Courier\">CadastroPatrimonio.exe</font> para cada terminal.", sBody))
    story.append(Paragraph("<b>Caso A — Primeira execução (prazo vigente, &lt;7 dias)</b>", sBody))
    for b in ["Duplo-clique no EXE (usuario comum, sem admin).", "Janela exibe: <b>painel amarelo</b>, botao <b>Fechar (10s)</b> desabilitado, campos tecnicos (Hostname, Usuario DOM\\user, Serial BIOS, SO+Arq).", "Tente fechar antes de 10s → mensagem de bloqueio; apos 10s botao vira <b>Fechar Temporariamente</b>.", "Teste filtros: Nome bloqueia numeros, Patrimonio só digitos (cole \"abc123\" → vira \"123\").", "Preencha Nome (≥3 letras), Patrimonio (só numeros), Setor → <b>Gravar e Concluir</b> → <b>Cadastro concluido</b>.", "Valide no servidor: <font face=\"Courier\">SELECT hostname,numero_patrimonio,ip_origem FROM computadores_cadastro WHERE hostname='SEU_HOSTNAME'</font> e no terminal: <font face=\"Courier\">reg query HKCU\\Software\\OCS_Inventario</font> e <font face=\"Courier\">dir %ProgramData%\\OCS_Inventario\\concluido.flag</font> (só existe apos HTTP 200).", "Reabra o EXE → deve <b>encerrar em &lt;10ms sem janela</b> (check HKLM/HKCU/flag em <font face=\"Courier\">RegistryHelper.cs</font>)."]:
        story.append(Paragraph(f"• {b}", sBullet))
    story.append(Paragraph("<b>Caso B — Prazo expirado (>7 dias)</b>", sBody))
    story.append(Paragraph("Para testar sem esperar 7 dias, ajuste a data no registro para -8 dias:", sBullet))
    story.append(Paragraph("<font face=\"Courier\">reg add HKCU\\Software\\OCS_Inventario /v PrimeiraExecucao /t REG_SZ /d \"2026-08-10 08:00:00\" /f</font>", sCode))
    story.append(Paragraph("Reabra o EXE → <b>painel vermelho</b>, mensagem OBRIGATORIO, bloqueio <b>120s (2 min)</b>. Após 120s libera fechar temporário. Ao gravar, flag definitiva como no Caso A.", sBody))
    story.append(Paragraph("<b>Caso C — Falha de rede</b>", sBody))
    for b in ["Desligue Apache (<font face=\"Courier\">systemctl stop apache2</font>) e tente gravar → <b>Falha de Comunicacao</b>, <b>nao</b> grava flag.", "Religue e grave novamente → sucesso. Valida <b>nao considerar concluido antes do banco</b>."]:
        story.append(Paragraph(f"• {b}", sBullet))
    story.append(Paragraph("<b>Reset para re-testar o mesmo terminal</b>", sBody))
    story.append(Paragraph("<font face=\"Courier\">reg delete \"HKCU\\Software\\OCS_Inventario\" /f<br/>reg delete \"HKLM\\Software\\OCS_Inventario\" /f<br/>del /q \"%ProgramData%\\OCS_Inventario\\*.*\"<br/>del /q \"%LocalAppData%\\OCS_Inventario\\*.*\"</font>", sCode))

    # ETAPA 3
    story.append(Paragraph("ETAPA 3 — Sincronizacao accountinfo.TAG (Oficial: .php)", sH1))
    story.append(Paragraph("Oficial: <font face=\"Courier\">sync/sync_ocs_patrimonio.php</font> (unico UPDATE em <font face=\"Courier\">accountinfo.TAG</font>). O <font face=\"Courier\">.py</font> é apenas emergencial e deve ser ignorado. Logica validada: <b>PAC → PACO-{patrimonio}, PLA/DES/FAZ → VIC-{patrimonio}, outros → LOCAL-{patrimonio}</b> (ex: <b>VIC-123456</b>). Nunca altera <font face=\"Courier\">hardware.NAME/USERID/TAG</font>.", sBody))
    story.append(Paragraph("1) Configurar", sH2))
    story.append(Paragraph("<font face=\"Courier\">cat sync/config_sync.php  # db_host 127.0.0.1 se no mesmo servidor<br/>cp sync/config_sync.php sync/sync_ocs_patrimonio.php /var/www/html/cadastro_api/sync/<br/>chown -R www-data:www-data /var/www/html/cadastro_api/sync</font>", sCode))
    story.append(Paragraph("2) Dry-run (simulacao)", sH2))
    story.append(Paragraph("<font face=\"Courier\">php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php --dry-run --verbose<br/>cat /var/log/ocs_sync_patrimonio.log</font>", sCode))
    story.append(Paragraph("Esperado: lista <b>[PENDENTE]</b> (sem OCS ainda), <b>[ATUALIZANDO] accountinfo.TAG 'HOSTNAME' → 'VIC-99999'</b> simulado, <b>0 gravacoes</b>; <font face=\"Courier\">SELECT sincronizado_ocs FROM computadores_cadastro</font> permanece 0.", sBody))
    story.append(Paragraph("3) Execucao real", sH2))
    story.append(Paragraph("<font face=\"Courier\">php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php --verbose<br/>tail -n 50 /var/log/ocs_sync_patrimonio.log<br/>mysql -u ocs -p ocsweb -e \"SELECT h.NAME, a.TAG, c.hostname, c.numero_patrimonio, c.sincronizado_ocs FROM hardware h JOIN accountinfo a ON h.ID=a.HARDWARE_ID JOIN computadores_cadastro c ON c.hostname=h.NAME WHERE c.hostname LIKE '%TESTE%'\"</font>", sCode))
    for b in ["<b>TAC correto:</b> <font face=\"Courier\">a.TAG = VIC-99999 / PACO-99999 / LOCAL-99999</font> conforme prefixo (valide os 3 casos com hostnames PAC-, PLA-, OUTRO).", "<b>h.NAME inalterado</b> = hostname original.", "<b>c.sincronizado_ocs = 1</b> e <b>data_sincronizacao</b> preenchida.", "Re-execute → <b>[JA SINCRONIZADO]</b> sem novo UPDATE.", "Se faltar <font face=\"Courier\">accountinfo</font> para o HARDWARE_ID → <b>[ERRO] sem registro accountinfo</b> e contador <b>Sem registro accountinfo</b> incrementa (nao quebra o lote)."]:
        story.append(Paragraph(f"• {b}", sBullet))
    story.append(Paragraph("4) Caso pendente (cadastro antes do inventario)", sH2))
    story.append(Paragraph("Cadastre um hostname inexistente no OCS, rode o sync → <b>[PENDENTE]</b>. Envie inventario do terminal depois, rode sync novamente → atualiza automaticamente. Valida tolerancia a ordem de chegada.", sBody))
    story.append(Paragraph("5) Cron (apos homologar)", sH2))
    story.append(Paragraph("<font face=\"Courier\">crontab -e<br/>*/10 * * * * /usr/bin/php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php > /dev/null 2>&1</font>", sCode))

    # CHECKLIST FINAL
    story.append(Paragraph("Checklist Final — Marque OK / FALHA por terminal", sH1))
    chk_headers = [Paragraph("<b>Item</b>", sCellH), Paragraph("<b>W7 32</b>", sCellH), Paragraph("<b>W7 64</b>", sCellH), Paragraph("<b>W10</b>", sCellH), Paragraph("<b>W11</b>", sCellH)]
    chk_rows = [
        ["E1: .bat manual OK + log arquitetura", "", "", "", ""],
        ["E1: Servico OCS + idempotencia (2ª exec)", "", "", "", ""],
        ["E1: OCS Server hardware.TAG = hostname", "", "", "", ""],
        ["E2A: curl 200 + 401 sem token", "—", "—", "—", "—"],
        ["E2B: Form abre (10s amarelo)", "", "", "", ""],
        ["E2B: Filtro Nome/Patrimonio + Serial BIOS", "", "", "", ""],
        ["E2B: Gravar → flag HKCU/HKLM/%ProgramData%", "", "", "", ""],
        ["E2B: Reabrir → encerra sem janela", "", "", "", ""],
        ["E2B: Prazo 120s vermelho (simulado)", "", "", "", ""],
        ["E2B: Falha rede nao grava flag", "", "", "", ""],
        ["E3: dry-run sem gravar", "—", "—", "—", "—"],
        ["E3: TAG PACO- / VIC- / LOCAL- correta", "", "", "", ""],
        ["E3: hardware.NAME preservado", "", "", "", ""],
        ["E3: Pendente → sincroniza apos inventario", "", "", "", ""],
    ]
    data = [chk_headers]
    for r in chk_rows:
        data.append([Paragraph(r[0], sCellSmall)] + [Paragraph("☐ OK  ☐ FALHA", sCellSmall) for _ in range(4)])
    # adjust first data row for E2A etc with —
    tChk = Table(data, colWidths=[198,80,80,80,81])
    tChk.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),C_NAVY),('VALIGN',(0,0),(-1,-1),'MIDDLE'),('GRID',(0,0),(-1,-1),0.5,C_BORDER),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white, colors.HexColor("#F8FAFC")]),('TOPPADDING',(0,0),(-1,-1),3),('BOTTOMPADDING',(0,0),(-1,-1),3),('LEFTPADDING',(0,0),(-1,-1),4),('RIGHTPADDING',(0,0),(-1,-1),4)]))
    story.append(tChk)
    story.append(Spacer(1,6))
    story.append(Paragraph("Evidencias obrigatorias para aceite: <b>(1)</b> print do log <font face=\"Courier\">ocs_agent_install.log</font> de cada SO, <b>(2)</b> print do form amarelo/vermelho, <b>(3)</b> <font face=\"Courier\">SELECT</font> do <font face=\"Courier\">accountinfo.TAG</font> com as 3 variacoes PACO/VIC/LOCAL, <b>(4)</b> <font face=\"Courier\">/var/log/ocs_sync_patrimonio.log</font> com RESUMO, <b>(5)</b> teste 401 sem token.", sBody))

    story.append(Paragraph("Logs e diagnóstico rápido", sH2))
    story.append(Paragraph("<font face=\"Courier\">Terminal: C:\\Windows\\Temp\\ocs_agent_install.log | C:\\ProgramData\\OCS Inventory NG\\Agent\\OCSInventory.log<br/>Servidor: /var/log/ocs_cadastro_api_error.log | /var/log/ocs_sync_patrimonio.log<br/>DB: SELECT * FROM computadores_cadastro ORDER BY data_atualizacao DESC LIMIT 20</font>", sCode))
    story.append(Paragraph("Critério de aceite: <b>4/4 terminais</b> com Etapa 1 (servico + idempotencia), Etapa 2 (flag só apos 200 + filtro + Serial BIOS sem \"To be filled by O.E.M.\"), Etapa 3 (TAG prefixada correta em <b>accountinfo</b> e <b>hardware.NAME</b> intacto).", sBody))

    doc.build(story, canvasmaker=NumberedCanvas)
    print(f"[OK] PDF gerado em: {out}")

if __name__ == "__main__":
    build_pdf()
