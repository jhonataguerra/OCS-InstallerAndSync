# -*- coding: utf-8 -*-
"""
Gerador do Documento Único Unificado de Engenharia e Implantação
Combina README.md, todos os manuais da pasta docs/ e anexa o Roteiro de Testes de Homologação.
Formato: A4, padrão executivo/técnico.
"""
import os
import sys
import pypdf
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, KeepTogether, HRFlowable
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
            self.drawString(40, 810, "SISTEMA INTEGRADO DE INVENTÁRIO OCS — DOCUMENTAÇÃO TÉCNICA E OPERACIONAL UNIFICADA")
            self.setStrokeColor(colors.HexColor("#CBD5E0"))
            self.setLineWidth(0.4)
            self.line(40, 804, 555, 804)

        self.drawRightString(555, 28, f"Página {self._pageNumber} de {page_count}")
        self.drawString(40, 28, "CONFIDENCIAL — ENGENHARIA DE SISTEMAS & OPERAÇÕES DE TI | OCS INVENTORY NG")
        self.setStrokeColor(colors.HexColor("#CBD5E0"))
        self.setLineWidth(0.4)
        self.line(40, 38, 555, 38)
        self.restoreState()

def build_unified_pdf():
    base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    docs_dir = os.path.join(base_dir, "docs")
    os.makedirs(docs_dir, exist_ok=True)

    # 1. Gera ou atualiza os PDFs dependentes se necessário
    test_pdf_script = os.path.join(base_dir, "scripts", "generate_test_roadmap_pdf.py")
    if os.path.exists(test_pdf_script):
        import subprocess
        subprocess.run([sys.executable, test_pdf_script], cwd=base_dir, check=False)

    temp_main_pdf = os.path.join(docs_dir, "temp_manual_completo.pdf")
    final_output_pdf = os.path.join(docs_dir, "documento_unico_manual_completo.pdf")
    test_roadmap_pdf = os.path.join(docs_dir, "roteiro_testes_homologacao.pdf")

    doc = SimpleDocTemplate(
        temp_main_pdf,
        pagesize=A4,
        leftMargin=38,
        rightMargin=38,
        topMargin=46,
        bottomMargin=46
    )

    styles = getSampleStyleSheet()
    C_PRIMARY   = colors.HexColor("#0F172A") # Slate 900
    C_NAVY      = colors.HexColor("#1E3A8A") # Blue 900
    C_BLUE      = colors.HexColor("#2563EB") # Blue 600
    C_DARK_BLUE = colors.HexColor("#1E40AF")
    C_LIGHT     = colors.HexColor("#F8FAFC")
    C_BORDER    = colors.HexColor("#E2E8F0")
    C_CODE_BG   = colors.HexColor("#F1F5F9")
    C_TEXT      = colors.HexColor("#334155")

    sTitle = ParagraphStyle('Title', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=20, leading=24, textColor=C_PRIMARY, spaceAfter=4)
    sSub = ParagraphStyle('Sub', parent=styles['Normal'], fontName='Helvetica', fontSize=10.5, leading=15, textColor=C_NAVY, spaceAfter=8)
    sH1 = ParagraphStyle('H1', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=12.5, leading=16, textColor=C_NAVY, spaceBefore=12, spaceAfter=5, keepWithNext=True)
    sH2 = ParagraphStyle('H2', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=10, leading=13.5, textColor=C_DARK_BLUE, spaceBefore=8, spaceAfter=3, keepWithNext=True)
    sBody = ParagraphStyle('Body', parent=styles['Normal'], fontName='Helvetica', fontSize=8.5, leading=12, textColor=C_TEXT, spaceAfter=4)
    sBullet = ParagraphStyle('Bullet', parent=sBody, leftIndent=12, bulletIndent=2, spaceAfter=2.5)
    sCode = ParagraphStyle('Code', parent=styles['Code'], fontName='Courier', fontSize=7.2, leading=9.5, textColor=colors.HexColor("#0F172A"), backColor=C_CODE_BG, borderPadding=(3,5,3), spaceAfter=5)
    sCell = ParagraphStyle('Cell', parent=styles['Normal'], fontName='Helvetica', fontSize=8, leading=10.5, textColor=C_TEXT)
    sCellH = ParagraphStyle('CellH', parent=styles['Normal'], fontName='Helvetica-Bold', fontSize=8, leading=10.5, textColor=colors.white, alignment=1)
    sCellSmall = ParagraphStyle('CellSmall', parent=sCell, fontSize=7.5, leading=9.5)

    story = []

    # CAPA / CABEÇALHO DO DOCUMENTO ÚNICO
    story.append(Paragraph("MANUAL DE ENGENHARIA E IMPLANTAÇÃO UNIFICADO", sTitle))
    story.append(Paragraph("Sistema Integrado de Inventário e Identificação de Máquinas — OCS Inventory NG<br/>Compatibilidade: Windows 7 (32/64 bits), Windows 10 e Windows 11 | Cenários: Active Directory & Workgroup", sSub))
    story.append(HRFlowable(width="100%", thickness=1.5, color=C_BLUE, spaceBefore=2, spaceAfter=8))

    meta_html = """<b>Status do Documento:</b> Oficial / Homologado &nbsp;|&nbsp; <b>Versão da Suíte:</b> 2.11 (Dual x86/x64)<br/>
    <b>Servidor OCS Homologado:</b> http://192.168.2.48/ocsinventory (ou IP externo configurável via parâmetro/script)<br/>
    <b>API de Ingestão:</b> http://[SERVIDOR]/cadastro_api/cadastrar.php &nbsp;|&nbsp; <b>Token Criptográfico:</b> X-API-TOKEN integrado<br/>
    <b>Conteúdo Integrado:</b> Arquitetura Global, Scripts de Automação, Guia Active Directory (GPO), Guia Workgroup (Fora do Domínio), Backend/Banco de Dados, Executável Cliente, Sincronização Cron e Roteiro de Testes."""
    
    tMeta = Table([[Paragraph(meta_html, sBody)]], colWidths=[519])
    tMeta.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), C_LIGHT),
        ('BOX', (0,0), (-1,-1), 0.8, C_BORDER),
        ('LEFTPADDING', (0,0), (-1,-1), 8),
        ('RIGHTPADDING', (0,0), (-1,-1), 8),
        ('TOPPADDING', (0,0), (-1,-1), 6),
        ('BOTTOMPADDING', (0,0), (-1,-1), 6)
    ]))
    story.append(tMeta)
    story.append(Spacer(1, 6))

    # SEÇÃO 1: VISÃO GERAL E ARQUITETURA
    story.append(Paragraph("1. VISÃO GERAL E FLUXO ARQUITETURAL", sH1))
    story.append(Paragraph("O sistema integra a coleta técnica automatizada de hardware e software ao cadastro patrimonial presencial ou remoto, sincronizando de forma transparente com a base de dados do OCS Inventory NG Server.", sBody))
    
    fluxo_txt = """
[ MÁQUINA CLIENTE (AD / GPO) ]               [ MÁQUINA FORA DO DOMÍNIO (WORKGROUP) ]
       │                                                    │
       ├─► (GPO Startup) install_ocs_agent.bat              ├─► instalar_workgroup.bat (IP externo)
       │      │                                             │   └─► instalar_workgroup_tag_manual.bat (TAG)
       │      └─► Instala OCS Agent (/TAG=%COMPUTERNAME%)   │
       │                                                    ├─► Copia CadastroPatrimonio.exe para ProgramFiles
       ├─► (GPO Logon) CadastroPatrimonio.exe               ├─► Grava Run (HKLM) e ApiEndpointUrl no Registro
       │      │                                             │
       │      ▼ (Coleta WMI + Formulário Responsável/Patrimônio/Setor)
       └──────┴─────────────► POST HTTP/JSON + X-API-TOKEN ─────────────► [ API PHP: cadastrar.php ]
                                                                                   │
                                                                                   ▼ (UPSERT Hostname)
                                                                        [ MariaDB / MySQL: ocsweb ]
                                                                                   │
                                                                                   ▼
    [ OCS Server: Cron a cada 10 min ] ◄── sync_ocs_patrimonio.php ────────────────┘
    Atualiza SOMENTE accountinfo.TAG (ex: VIC-123456 / PACO-123456 / LOCAL-123456)
    Preserva 100% de hardware.NAME, USERID e histórico completo.
    """
    story.append(Paragraph(fluxo_txt.replace("\n", "<br/>").replace(" ", "&nbsp;"), sCode))

    # SEÇÃO 2: ESTRUTURA DO REPOSITÓRIO
    story.append(Paragraph("2. ESTRUTURA DE COMPONENTES DO REPOSITÓRIO", sH1))
    repo_rows = [
        [Paragraph("<b>Diretório / Arquivo</b>", sCellH), Paragraph("<b>Função Técnica / Descrição</b>", sCellH)],
        [Paragraph("<code>scripts/install_ocs_agent.bat</code>", sCell), Paragraph("Instalador autônomo e silencioso do OCS Agent. Detecta x86/x64, valida serviço existente (idempotente) e suporta parâmetros de URL e TAG.", sCellSmall)],
        [Paragraph("<code>scripts/instalar_workgroup.bat</code>", sCell), Paragraph("Instalador mestre para computadores fora do domínio. IP externo configurável via variável; persistência no Registro e inicialização imediata.", sCellSmall)],
        [Paragraph("<code>scripts/instalar_workgroup_tag_manual.bat</code>", sCell), Paragraph("Versão interativa para Workgroup permitindo digitação da TAG personalizada no prompt no momento da instalação.", sCellSmall)],
        [Paragraph("<code>client_app/CadastroPatrimonio.exe</code>", sCell), Paragraph("Binário executável Windows Forms compilado nativamente em .NET 3.5 AnyCPU. Leitura dinâmica de endpoint via Registro.", sCellSmall)],
        [Paragraph("<code>api/cadastrar.php & config.php</code>", sCell), Paragraph("API REST de ingestão dos dados patrimoniais com token de segurança (SEC-02) e sanitização estrita anti-XSS.", sCellSmall)],
        [Paragraph("<code>database/schema.sql</code>", sCell), Paragraph("Definição da tabela <code>computadores_cadastro</code> com chave única primária em <code>hostname</code>.", sCellSmall)],
        [Paragraph("<code>sync/sync_ocs_patrimonio.php</code>", sCell), Paragraph("Script CLI em PHP para execução via Cron. Atualiza <code>accountinfo.TAG</code> aplicando regras de prefixo (PAC/VIC/LOCAL).", sCellSmall)],
        [Paragraph("<code>tests/test_install_agent.ps1</code>", sCell), Paragraph("Suíte de 10 testes automatizados (T-01 a T-10) cobrindo arquitetura, idempotência, resiliência e parâmetros Workgroup.", sCellSmall)],
    ]
    tRepo = Table(repo_rows, colWidths=[175, 344])
    tRepo.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), C_NAVY),
        ('GRID', (0,0), (-1,-1), 0.5, C_BORDER),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [colors.white, C_LIGHT]),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 5),
        ('RIGHTPADDING', (0,0), (-1,-1), 5),
    ]))
    story.append(tRepo)
    story.append(Spacer(1, 6))

    # SEÇÃO 3: DISTRIBUIÇÃO VIA ACTIVE DIRECTORY (GPO)
    story.append(Paragraph("3. DISTRIBUIÇÃO CORPORATIVA VIA ACTIVE DIRECTORY (GPO)", sH1))
    story.append(Paragraph("Para redes corporativas gerenciadas via domínio, a distribuição divide-se em duas GPOs integradas:", sBody))
    story.append(Paragraph("• <b>Etapa 1 (GPO de Computador - Startup):</b> Distribui o <code>install_ocs_agent.bat</code> juntamente com os executáveis <code>OCS-Agent-2.11-x86.exe</code> e <code>OCS-Agent-2.11-x64.exe</code>. Executa antes do logon com permissão SYSTEM, garantindo que o agente esteja ativo e o primeiro inventário enviado.", sBullet))
    story.append(Paragraph("• <b>Etapa 2 (GPO de Usuário - Logon):</b> Publica apenas o binário <code>CadastroPatrimonio.exe</code> no compartilhamento de logon (SYSVOL). O executável abre na sessão do usuário exibindo os dados de hardware coletados via WMI.", sBullet))
    story.append(Paragraph("• <b>Regra dos 7 Dias:</b> Nos primeiros 7 dias, exibe aviso amarelo e contador de 10s para leitura; após 7 dias da primeira execução sem preenchimento, torna-se obrigatório (painel vermelho e bloqueio de 120s).", sBullet))
    story.append(Paragraph("• <b>Idempotência e Encerramento:</b> Assim que transmitido com sucesso (HTTP 200), a flag é gravada no Registro (HKLM/HKCU). Nos próximos logons, o programa encerra em menos de 10 milissegundos.", sBullet))

    # SEÇÃO 4: DISTRIBUIÇÃO FORA DO DOMÍNIO (WORKGROUP)
    story.append(Paragraph("4. DISTRIBUIÇÃO EM MÁQUINAS FORA DO DOMÍNIO (WORKGROUP)", sH1))
    story.append(Paragraph("Para filiais sem DC, computadores em Workgroup, home office ou redes isoladas, utilizam-se os instaladores unificados:", sBody))
    story.append(Paragraph("<b>1) Configuração do IP / Host Externo:</b> No script <code>scripts/instalar_workgroup.bat</code> ou <code>scripts/instalar_workgroup_tag_manual.bat</code>, defina a variável <code>SERVER_HOST=seu.ip.externo</code>.", sBody))
    story.append(Paragraph("<b>2) Execução com Privilégios de Administrador:</b>", sBody))
    story.append(Paragraph("• <b>Instalação Direta:</b> Botão direito em <code>instalar_workgroup.bat</code> > 'Executar como Administrador'. Instala o agente OCS apontando para o IP externo, usa a TAG padrão (%COMPUTERNAME%), copia o <code>CadastroPatrimonio.exe</code> para <code>C:\\Program Files\\InventarioPatrimonio</code>, adiciona na chave Run do Windows e dispara imediatamente o aplicativo.", sBullet))
    story.append(Paragraph("• <b>Instalação com TAG Manual:</b> Botão direito em <code>instalar_workgroup_tag_manual.bat</code> > 'Executar como Administrador'. Solicita a digitação da TAG no console (ex: VIC-123456). O OCS Agent é instalado e já envia o inventário inicial com essa TAG personalizada.", sBullet))
    story.append(Paragraph("<b>3) Leitura Dinâmica de Endpoint:</b> O executável <code>CadastroPatrimonio.exe</code> lê o endereço da API diretamente de <code>HKLM\\Software\\OCS_Inventario\\ApiEndpointUrl</code> gravado pelo script, não necessitando de recompilação ao mudar de servidor.", sBody))

    # SEÇÃO 5: BACKEND, API E BANCO DE DADOS
    story.append(Paragraph("5. SERVIDOR: BANCO DE DADOS E API DE INGESTÃO", sH1))
    story.append(Paragraph("No servidor Linux (onde roda o OCS Server / Apache / MySQL):", sBody))
    story.append(Paragraph("<b>1) Banco de Dados:</b> Importe a estrutura com <code>mysql -u root -p ocsweb &lt; database/schema.sql</code>.", sBody))
    story.append(Paragraph("<b>2) Publicação da API:</b> Copie <code>api/config.php</code> e <code>api/cadastrar.php</code> para <code>/var/www/html/cadastro_api/</code>.", sBody))
    story.append(Paragraph("<b>3) Segurança Criptográfica:</b> A API exige o cabeçalho <code>X-API-TOKEN</code> definido em <code>config.php</code>. Requisições sem o token ou com token inválido recebem HTTP 401 Unauthorized imediato, protegendo contra cadastros forjados.", sBody))

    # SEÇÃO 6: PROCESSO DE SINCRONIZAÇÃO OCS
    story.append(Paragraph("6. SINCRONIZAÇÃO AUTOMÁTICA DE PATRIMÔNIO (CRON)", sH1))
    story.append(Paragraph("O script oficial <code>sync/sync_ocs_patrimonio.php</code> roda no crontab do servidor a cada 10 minutos:", sBody))
    story.append(Paragraph("<code>*/10 * * * * /usr/bin/php /var/www/html/cadastro_api/sync/sync_ocs_patrimonio.php > /dev/null 2>&1</code>", sCode))
    story.append(Paragraph("• <b>Preservação Absoluta:</b> O script realiza UPDATE <b>exclusivamente</b> no campo <code>accountinfo.TAG</code>. O campo <code>hardware.NAME</code> permanece com o hostname original, mantendo integridade histórica e compatibilidade de rede.", sBody))
    story.append(Paragraph("• <b>Regra de Prefixo:</b> Hostnames com <code>PAC*</code> recebem <code>PACO-{patrimonio}</code>; hostnames com <code>PLA*</code>, <code>DES*</code> ou <code>FAZ*</code> recebem <code>VIC-{patrimonio}</code>; outros hostnames recebem <code>LOCAL-{patrimonio}</code>.", sBody))
    story.append(Paragraph("• <b>Tolerância à Ordem:</b> Caso o usuário cadastre o patrimônio antes do inventário do agente chegar ao servidor, o registro aguarda como pendente e é associado automaticamente no próximo ciclo.", sBody))

    # SEÇÃO 7: SANIDADE E TESTES AUTOMATIZADOS (10/10)
    story.append(Paragraph("7. SUÍTE DE TESTES AUTOMATIZADOS E AUDITORIA (10/10)", sH1))
    story.append(Paragraph("Antes do envio para os terminais, o orquestrador <code>scripts/run_tests_and_security.py</code> executa a suíte de testes unitários e de integração em ambiente isolado (sem tocar em produção):", sBody))
    test_rows = [
        [Paragraph("<b>ID</b>", sCellH), Paragraph("<b>Cenário de Teste / Validação</b>", sCellH), Paragraph("<b>Resultado Esperado</b>", sCellH)],
        [Paragraph("T-01", sCell), Paragraph("Detecção de Arquitetura AMD64 nativo", sCellSmall), Paragraph("Instalador x64 selecionado automaticamente", sCellSmall)],
        [Paragraph("T-02", sCell), Paragraph("Detecção de Arquitetura x86 nativo (32-bit)", sCellSmall), Paragraph("Instalador x86 selecionado automaticamente", sCellSmall)],
        [Paragraph("T-03", sCell), Paragraph("Detecção de Arquitetura via WOW64 (ARCHITEW6432)", sCellSmall), Paragraph("Instalador x64 selecionado para processo 32-bit em SO 64", sCellSmall)],
        [Paragraph("T-04", sCell), Paragraph("Idempotência em ProgramFiles", sCellSmall), Paragraph("Binário existente encerra script com exit 0 sem reinstalar", sCellSmall)],
        [Paragraph("T-05", sCell), Paragraph("Idempotência em ProgramFiles(x86)", sCellSmall), Paragraph("Binário existente encerra script com exit 0 sem reinstalar", sCellSmall)],
        [Paragraph("T-06", sCell), Paragraph("Resiliência a Instalador Ausente", sCellSmall), Paragraph("Interrompe com exit 1 e grava ERRO CRITICO no log", sCellSmall)],
        [Paragraph("T-07", sCell), Paragraph("Geração de Log em %TEMP%", sCellSmall), Paragraph("Arquivo ocs_agent_install.log contém COMPUTERNAME", sCellSmall)],
        [Paragraph("T-08", sCell), Paragraph("Prefixo Padronizado de Falhas", sCellSmall), Paragraph("Gravação explícita de prefixo ERRO CRITICO para SIEM", sCellSmall)],
        [Paragraph("T-09", sCell), Paragraph("Parametrização de URL Customizada (Workgroup)", sCellSmall), Paragraph("Argumento %1 repassado ao instalador e logado", sCellSmall)],
        [Paragraph("T-10", sCell), Paragraph("Parametrização de TAG Manual (Workgroup)", sCellSmall), Paragraph("Argumento %2 repassado como /TAG= e logado", sCellSmall)],
    ]
    tTests = Table(test_rows, colWidths=[38, 250, 231])
    tTests.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), C_NAVY),
        ('GRID', (0,0), (-1,-1), 0.5, C_BORDER),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [colors.white, C_LIGHT]),
        ('TOPPADDING', (0,0), (-1,-1), 2.5),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2.5),
        ('LEFTPADDING', (0,0), (-1,-1), 4),
        ('RIGHTPADDING', (0,0), (-1,-1), 4),
    ]))
    story.append(tTests)
    story.append(Spacer(1, 6))

    # SEÇÃO 8: GUIA RÁPIDO DE DIAGNÓSTICO E RESET
    story.append(Paragraph("8. GUIA DE DIAGNÓSTICO RÁPIDO E COMANDOS DE RESET", sH1))
    story.append(Paragraph("<b>Reset completo da aplicação no cliente (para testes de homologação):</b>", sBody))
    story.append(Paragraph("<code>reg delete \"HKCU\\Software\\OCS_Inventario\" /f<br/>reg delete \"HKLM\\Software\\OCS_Inventario\" /f<br/>del /f /q \"%ProgramData%\\OCS_Inventario\\*.*\" 2>nul<br/>del /f /q \"%LocalAppData%\\OCS_Inventario\\*.*\" 2>nul</code>", sCode))
    story.append(Paragraph("<b>Localização de Logs:</b>", sBody))
    story.append(Paragraph("• <b>Instalação Windows:</b> <code>C:\\Windows\\Temp\\ocs_agent_install.log</code>", sBullet))
    story.append(Paragraph("• <b>Agente OCS:</b> <code>C:\\ProgramData\\OCS Inventory NG\\Agent\\OCSInventory.log</code>", sBullet))
    story.append(Paragraph("• <b>Erros da API:</b> <code>/var/log/ocs_cadastro_api_error.log</code> (no servidor)", sBullet))
    story.append(Paragraph("• <b>Sincronização Cron:</b> <code>/var/log/ocs_sync_patrimonio.log</code> (no servidor)", sBullet))
    story.append(Spacer(1, 6))

    story.append(Paragraph("O Roteiro Oficial Guiado de Testes de Homologação encontra-se anexado a seguir nesta mesma documentação impressa.", sBody))

    # Constrói o corpo principal
    doc.build(story, canvasmaker=NumberedCanvas)
    print(f"[OK] Documento principal compilado em: {temp_main_pdf}")

    # Mescla com o PDF do Roteiro de Testes
    writer = pypdf.PdfWriter()

    reader_main = pypdf.PdfReader(temp_main_pdf)
    for page in reader_main.pages:
        writer.add_page(page)

    if os.path.exists(test_roadmap_pdf):
        reader_test = pypdf.PdfReader(test_roadmap_pdf)
        for page in reader_test.pages:
            writer.add_page(page)
        print(f"[OK] Roteiro de testes ({len(reader_test.pages)} paginas) anexado com sucesso.")
    else:
        print("[AVISO] roteiro_testes_homologacao.pdf nao encontrado para anexo.")

    with open(final_output_pdf, "wb") as f_out:
        writer.write(f_out)

    if os.path.exists(temp_main_pdf):
        os.remove(temp_main_pdf)

    print(f"[SUCESSO] Documento Unico consolidado gerado em: {final_output_pdf}")

if __name__ == "__main__":
    build_unified_pdf()
