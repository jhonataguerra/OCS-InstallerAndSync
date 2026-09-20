using System.Drawing;
using System.Windows.Forms;

namespace OCSCadastroApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelHeader;
        private Label lblHeaderBadge;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Panel panelHeaderLine;

        private Panel panelContent;

        private Label lblNome;
        private TextBox txtNome;
        private Label lblPatrimonio;
        private TextBox txtPatrimonio;
        private Label lblSetor;
        private TextBox txtSetor;

        private Panel panelSysInfo;
        private Label lblSysTitle;
        private Label lblTagHostname;
        private Label lblValHostname;
        private Label lblTagUsuario;
        private Label lblValUsuario;
        private Label lblTagSerial;
        private Label lblValSerial;
        private Label lblTagSO;
        private Label lblValSO;

        private Panel panelAviso;
        private Label lblAvisoIcon;
        private Label lblAvisoTitulo;
        private Label lblAvisoDescricao;

        private Button btnEnviar;
        private Button btnFechar;
        private Label lblStatus;
        private Timer timerBloqueio;

        private Panel panelFooter;
        private Panel panelFooterLine;
        private Label lblFooterPrefeitura;
        private Label lblFooterSecretaria;
        private Label lblFooterDiretoria;
        private Label lblFooterVersao;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.panelHeader = new Panel();
            this.lblHeaderBadge = new Label();
            this.lblHeaderTitle = new Label();
            this.lblHeaderSubtitle = new Label();
            this.panelHeaderLine = new Panel();

            this.panelContent = new Panel();

            this.lblNome = new Label();
            this.txtNome = new TextBox();
            this.lblPatrimonio = new Label();
            this.txtPatrimonio = new TextBox();
            this.lblSetor = new Label();
            this.txtSetor = new TextBox();

            this.panelSysInfo = new Panel();
            this.lblSysTitle = new Label();
            this.lblTagHostname = new Label();
            this.lblValHostname = new Label();
            this.lblTagUsuario = new Label();
            this.lblValUsuario = new Label();
            this.lblTagSerial = new Label();
            this.lblValSerial = new Label();
            this.lblTagSO = new Label();
            this.lblValSO = new Label();

            this.panelAviso = new Panel();
            this.lblAvisoIcon = new Label();
            this.lblAvisoTitulo = new Label();
            this.lblAvisoDescricao = new Label();

            this.btnEnviar = new Button();
            this.btnFechar = new Button();
            this.lblStatus = new Label();
            this.timerBloqueio = new Timer(this.components);

            this.panelFooter = new Panel();
            this.panelFooterLine = new Panel();
            this.lblFooterPrefeitura = new Label();
            this.lblFooterSecretaria = new Label();
            this.lblFooterDiretoria = new Label();
            this.lblFooterVersao = new Label();

            this.panelHeader.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.panelSysInfo.SuspendLayout();
            this.panelAviso.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = Color.FromArgb(255, 255, 255);
            this.panelHeader.Controls.Add(this.lblHeaderBadge);
            this.panelHeader.Controls.Add(this.lblHeaderSubtitle);
            this.panelHeader.Controls.Add(this.lblHeaderTitle);
            this.panelHeader.Controls.Add(this.panelHeaderLine);
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Location = new Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new Size(620, 88);
            this.panelHeader.TabIndex = 0;

            // 
            // panelHeaderLine
            // 
            this.panelHeaderLine.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
            this.panelHeaderLine.Dock = DockStyle.Top;
            this.panelHeaderLine.Location = new Point(0, 0);
            this.panelHeaderLine.Name = "panelHeaderLine";
            this.panelHeaderLine.Size = new Size(620, 3);
            this.panelHeaderLine.TabIndex = 3;

            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblHeaderTitle.ForeColor = Color.FromArgb(15, 23, 42); // Slate 900
            this.lblHeaderTitle.Location = new Point(24, 18);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new Size(320, 28);
            this.lblHeaderTitle.Text = "Identificação de Patrimônio";

            // 
            // lblHeaderSubtitle
            // 
            this.lblHeaderSubtitle.AutoSize = true;
            this.lblHeaderSubtitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblHeaderSubtitle.ForeColor = Color.FromArgb(100, 116, 139); // Slate 500
            this.lblHeaderSubtitle.Location = new Point(25, 52);
            this.lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            this.lblHeaderSubtitle.Size = new Size(430, 18);
            this.lblHeaderSubtitle.Text = "Vincule as informações deste computador ao inventário corporativo";

            // 
            // lblHeaderBadge
            // 
            this.lblHeaderBadge.BackColor = Color.FromArgb(239, 246, 255); // Blue 50
            this.lblHeaderBadge.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblHeaderBadge.ForeColor = Color.FromArgb(29, 78, 216); // Blue 700
            this.lblHeaderBadge.Location = new Point(480, 22);
            this.lblHeaderBadge.Name = "lblHeaderBadge";
            this.lblHeaderBadge.Size = new Size(116, 26);
            this.lblHeaderBadge.Text = "OCS INVENTORY";
            this.lblHeaderBadge.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // panelContent
            // 
            this.panelContent.BackColor = Color.FromArgb(248, 250, 252); // Slate 50
            this.panelContent.Controls.Add(this.lblStatus);
            this.panelContent.Controls.Add(this.btnFechar);
            this.panelContent.Controls.Add(this.btnEnviar);
            this.panelContent.Controls.Add(this.panelAviso);
            this.panelContent.Controls.Add(this.panelSysInfo);
            this.panelContent.Controls.Add(this.txtSetor);
            this.panelContent.Controls.Add(this.lblSetor);
            this.panelContent.Controls.Add(this.txtPatrimonio);
            this.panelContent.Controls.Add(this.lblPatrimonio);
            this.panelContent.Controls.Add(this.txtNome);
            this.panelContent.Controls.Add(this.lblNome);
            this.panelContent.Location = new Point(0, 88);
            this.panelContent.Name = "panelContent";
            this.panelContent.Padding = new Padding(28, 16, 28, 16);
            this.panelContent.Size = new Size(620, 480);
            this.panelContent.TabIndex = 1;

            // 
            // lblNome
            // 
            this.lblNome.AutoSize = true;
            this.lblNome.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblNome.ForeColor = Color.FromArgb(51, 65, 85); // Slate 700
            this.lblNome.Location = new Point(28, 14);
            this.lblNome.Name = "lblNome";
            this.lblNome.Size = new Size(160, 15);
            this.lblNome.Text = "NOME DO RESPONSÁVEL";

            // 
            // txtNome
            // 
            this.txtNome.BackColor = Color.White;
            this.txtNome.BorderStyle = BorderStyle.FixedSingle;
            this.txtNome.Font = new Font("Segoe UI", 10.5F);
            this.txtNome.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtNome.Location = new Point(28, 34);
            this.txtNome.MaxLength = 150;
            this.txtNome.Name = "txtNome";
            this.txtNome.Size = new Size(564, 26);
            this.txtNome.TabIndex = 2;
            this.txtNome.KeyPress += new KeyPressEventHandler(this.TxtNome_KeyPress);
            this.txtNome.TextChanged += new System.EventHandler(this.TxtNome_TextChanged);

            // 
            // lblPatrimonio
            // 
            this.lblPatrimonio.AutoSize = true;
            this.lblPatrimonio.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblPatrimonio.ForeColor = Color.FromArgb(51, 65, 85);
            this.lblPatrimonio.Location = new Point(28, 70);
            this.lblPatrimonio.Name = "lblPatrimonio";
            this.lblPatrimonio.Size = new Size(130, 15);
            this.lblPatrimonio.Text = "Nº DE PATRIMÔNIO";

            // 
            // txtPatrimonio
            // 
            this.txtPatrimonio.BackColor = Color.White;
            this.txtPatrimonio.BorderStyle = BorderStyle.FixedSingle;
            this.txtPatrimonio.Font = new Font("Segoe UI", 10.5F);
            this.txtPatrimonio.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtPatrimonio.Location = new Point(28, 90);
            this.txtPatrimonio.MaxLength = 50;
            this.txtPatrimonio.Name = "txtPatrimonio";
            this.txtPatrimonio.Size = new Size(564, 26);
            this.txtPatrimonio.TabIndex = 3;
            this.txtPatrimonio.KeyPress += new KeyPressEventHandler(this.TxtPatrimonio_KeyPress);
            this.txtPatrimonio.TextChanged += new System.EventHandler(this.TxtPatrimonio_TextChanged);

            // 
            // lblSetor
            // 
            this.lblSetor.AutoSize = true;
            this.lblSetor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblSetor.ForeColor = Color.FromArgb(51, 65, 85);
            this.lblSetor.Location = new Point(28, 126);
            this.lblSetor.Name = "lblSetor";
            this.lblSetor.Size = new Size(160, 15);
            this.lblSetor.Text = "SETOR / DEPARTAMENTO";

            // 
            // txtSetor
            // 
            this.txtSetor.BackColor = Color.White;
            this.txtSetor.BorderStyle = BorderStyle.FixedSingle;
            this.txtSetor.Font = new Font("Segoe UI", 10.5F);
            this.txtSetor.ForeColor = Color.FromArgb(15, 23, 42);
            this.txtSetor.Location = new Point(28, 146);
            this.txtSetor.MaxLength = 100;
            this.txtSetor.Name = "txtSetor";
            this.txtSetor.Size = new Size(564, 26);
            this.txtSetor.TabIndex = 4;

            // 
            // panelSysInfo
            // 
            this.panelSysInfo.BackColor = Color.FromArgb(255, 255, 255);
            this.panelSysInfo.BorderStyle = BorderStyle.FixedSingle;
            this.panelSysInfo.Controls.Add(this.lblValSO);
            this.panelSysInfo.Controls.Add(this.lblTagSO);
            this.panelSysInfo.Controls.Add(this.lblValSerial);
            this.panelSysInfo.Controls.Add(this.lblTagSerial);
            this.panelSysInfo.Controls.Add(this.lblValUsuario);
            this.panelSysInfo.Controls.Add(this.lblTagUsuario);
            this.panelSysInfo.Controls.Add(this.lblValHostname);
            this.panelSysInfo.Controls.Add(this.lblTagHostname);
            this.panelSysInfo.Controls.Add(this.lblSysTitle);
            this.panelSysInfo.Location = new Point(28, 186);
            this.panelSysInfo.Name = "panelSysInfo";
            this.panelSysInfo.Size = new Size(564, 108);
            this.panelSysInfo.TabIndex = 5;

            // 
            // lblSysTitle
            // 
            this.lblSysTitle.AutoSize = true;
            this.lblSysTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.lblSysTitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            this.lblSysTitle.Location = new Point(14, 8);
            this.lblSysTitle.Name = "lblSysTitle";
            this.lblSysTitle.Size = new Size(220, 13);
            this.lblSysTitle.Text = "INFORMAÇÕES TÉCNICAS DETECTADAS";

            // 
            // lblTagHostname
            // 
            this.lblTagHostname.AutoSize = true;
            this.lblTagHostname.Font = new Font("Segoe UI", 8F);
            this.lblTagHostname.ForeColor = Color.FromArgb(100, 116, 139);
            this.lblTagHostname.Location = new Point(14, 28);
            this.lblTagHostname.Name = "lblTagHostname";
            this.lblTagHostname.Size = new Size(69, 13);
            this.lblTagHostname.Text = "HOSTNAME:";

            // 
            // lblValHostname
            // 
            this.lblValHostname.AutoSize = true;
            this.lblValHostname.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblValHostname.ForeColor = Color.FromArgb(30, 41, 59);
            this.lblValHostname.Location = new Point(14, 43);
            this.lblValHostname.Name = "lblValHostname";
            this.lblValHostname.Size = new Size(27, 15);
            this.lblValHostname.Text = "---";

            // 
            // lblTagUsuario
            // 
            this.lblTagUsuario.AutoSize = true;
            this.lblTagUsuario.Font = new Font("Segoe UI", 8F);
            this.lblTagUsuario.ForeColor = Color.FromArgb(100, 116, 139);
            this.lblTagUsuario.Location = new Point(290, 28);
            this.lblTagUsuario.Name = "lblTagUsuario";
            this.lblTagUsuario.Size = new Size(95, 13);
            this.lblTagUsuario.Text = "USUÁRIO ATUAL:";

            // 
            // lblValUsuario
            // 
            this.lblValUsuario.AutoSize = true;
            this.lblValUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblValUsuario.ForeColor = Color.FromArgb(30, 41, 59);
            this.lblValUsuario.Location = new Point(290, 43);
            this.lblValUsuario.Name = "lblValUsuario";
            this.lblValUsuario.Size = new Size(27, 15);
            this.lblValUsuario.Text = "---";

            // 
            // lblTagSerial
            // 
            this.lblTagSerial.AutoSize = true;
            this.lblTagSerial.Font = new Font("Segoe UI", 8F);
            this.lblTagSerial.ForeColor = Color.FromArgb(100, 116, 139);
            this.lblTagSerial.Location = new Point(14, 63);
            this.lblTagSerial.Name = "lblTagSerial";
            this.lblTagSerial.Size = new Size(74, 13);
            this.lblTagSerial.Text = "SERIAL BIOS:";

            // 
            // lblValSerial
            // 
            this.lblValSerial.AutoSize = true;
            this.lblValSerial.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblValSerial.ForeColor = Color.FromArgb(30, 41, 59);
            this.lblValSerial.Location = new Point(14, 78);
            this.lblValSerial.Name = "lblValSerial";
            this.lblValSerial.Size = new Size(27, 15);
            this.lblValSerial.Text = "---";

            // 
            // lblTagSO
            // 
            this.lblTagSO.AutoSize = true;
            this.lblTagSO.Font = new Font("Segoe UI", 8F);
            this.lblTagSO.ForeColor = Color.FromArgb(100, 116, 139);
            this.lblTagSO.Location = new Point(290, 63);
            this.lblTagSO.Name = "lblTagSO";
            this.lblTagSO.Size = new Size(137, 13);
            this.lblTagSO.Text = "SISTEMA OPERACIONAL:";

            // 
            // lblValSO
            // 
            this.lblValSO.AutoSize = true;
            this.lblValSO.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblValSO.ForeColor = Color.FromArgb(30, 41, 59);
            this.lblValSO.Location = new Point(290, 78);
            this.lblValSO.Name = "lblValSO";
            this.lblValSO.Size = new Size(27, 15);
            this.lblValSO.Text = "---";

            // 
            // panelAviso
            // 
            this.panelAviso.BackColor = Color.FromArgb(254, 243, 199); // Amber 100
            this.panelAviso.BorderStyle = BorderStyle.FixedSingle;
            this.panelAviso.Controls.Add(this.lblAvisoDescricao);
            this.panelAviso.Controls.Add(this.lblAvisoTitulo);
            this.panelAviso.Controls.Add(this.lblAvisoIcon);
            this.panelAviso.Location = new Point(28, 304);
            this.panelAviso.Name = "panelAviso";
            this.panelAviso.Size = new Size(564, 66);
            this.panelAviso.TabIndex = 6;

            // 
            // lblAvisoIcon
            // 
            this.lblAvisoIcon.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblAvisoIcon.ForeColor = Color.FromArgb(180, 83, 9); // Amber 700
            this.lblAvisoIcon.Location = new Point(10, 15);
            this.lblAvisoIcon.Name = "lblAvisoIcon";
            this.lblAvisoIcon.Size = new Size(32, 32);
            this.lblAvisoIcon.Text = "ℹ";
            this.lblAvisoIcon.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // lblAvisoTitulo
            // 
            this.lblAvisoTitulo.AutoSize = true;
            this.lblAvisoTitulo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.lblAvisoTitulo.ForeColor = Color.FromArgb(120, 53, 15); // Amber 900
            this.lblAvisoTitulo.Location = new Point(46, 11);
            this.lblAvisoTitulo.Name = "lblAvisoTitulo";
            this.lblAvisoTitulo.Size = new Size(300, 15);
            this.lblAvisoTitulo.Text = "Preenchimento do Inventário Corporativo Pendente";

            // 
            // lblAvisoDescricao
            // 
            this.lblAvisoDescricao.Font = new Font("Segoe UI", 8.5F);
            this.lblAvisoDescricao.ForeColor = Color.FromArgb(146, 64, 14); // Amber 800
            this.lblAvisoDescricao.Location = new Point(46, 29);
            this.lblAvisoDescricao.Name = "lblAvisoDescricao";
            this.lblAvisoDescricao.Size = new Size(506, 32);
            this.lblAvisoDescricao.Text = "Restam X dias para se tornar obrigatório. Após gravado com sucesso, esta tela não será mais exibida.";

            // 
            // btnEnviar
            // 
            this.btnEnviar.BackColor = Color.FromArgb(37, 99, 235); // Modern Royal Blue 600
            this.btnEnviar.Cursor = Cursors.Hand;
            this.btnEnviar.FlatAppearance.BorderSize = 0;
            this.btnEnviar.FlatStyle = FlatStyle.Flat;
            this.btnEnviar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnEnviar.ForeColor = Color.White;
            this.btnEnviar.Location = new Point(318, 380);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new Size(274, 42);
            this.btnEnviar.TabIndex = 7;
            this.btnEnviar.Text = "Gravar e Concluir";
            this.btnEnviar.UseVisualStyleBackColor = false;
            this.btnEnviar.Click += new System.EventHandler(this.BtnEnviar_Click);

            // 
            // btnFechar
            // 
            this.btnFechar.BackColor = Color.FromArgb(241, 245, 249); // Slate 100
            this.btnFechar.Cursor = Cursors.Default;
            this.btnFechar.Enabled = false;
            this.btnFechar.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            this.btnFechar.FlatStyle = FlatStyle.Flat;
            this.btnFechar.Font = new Font("Segoe UI", 9.5F);
            this.btnFechar.ForeColor = Color.FromArgb(100, 116, 139);
            this.btnFechar.Location = new Point(28, 380);
            this.btnFechar.Name = "btnFechar";
            this.btnFechar.Size = new Size(274, 42);
            this.btnFechar.TabIndex = 8;
            this.btnFechar.Text = "Fechar (10s)";
            this.btnFechar.UseVisualStyleBackColor = false;
            this.btnFechar.Click += new System.EventHandler(this.BtnFechar_Click);

            // 
            // lblStatus
            // 
            this.lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this.lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            this.lblStatus.Location = new Point(28, 432);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(564, 22);
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // timerBloqueio
            // 
            this.timerBloqueio.Interval = 1000;
            this.timerBloqueio.Tick += new System.EventHandler(this.TimerBloqueio_Tick);

            // 
            // panelFooterLine
            // 
            this.panelFooterLine.BackColor = Color.FromArgb(226, 232, 240); // Slate 200
            this.panelFooterLine.Dock = DockStyle.Top;
            this.panelFooterLine.Location = new Point(0, 0);
            this.panelFooterLine.Name = "panelFooterLine";
            this.panelFooterLine.Size = new Size(620, 1);
            this.panelFooterLine.TabIndex = 0;

            // 
            // lblFooterPrefeitura
            // 
            this.lblFooterPrefeitura.AutoSize = false;
            this.lblFooterPrefeitura.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblFooterPrefeitura.ForeColor = Color.FromArgb(30, 41, 59); // Slate 800
            this.lblFooterPrefeitura.Location = new Point(0, 10);
            this.lblFooterPrefeitura.Name = "lblFooterPrefeitura";
            this.lblFooterPrefeitura.Size = new Size(620, 18);
            this.lblFooterPrefeitura.Text = "Prefeitura Municipal de Santa B\u00e1rbara d\u0027Oeste";
            this.lblFooterPrefeitura.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // lblFooterSecretaria
            // 
            this.lblFooterSecretaria.AutoSize = false;
            this.lblFooterSecretaria.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblFooterSecretaria.ForeColor = Color.FromArgb(100, 116, 139); // Slate 500
            this.lblFooterSecretaria.Location = new Point(0, 29);
            this.lblFooterSecretaria.Name = "lblFooterSecretaria";
            this.lblFooterSecretaria.Size = new Size(620, 16);
            this.lblFooterSecretaria.Text = "Secret\u00e1ria de Administra\u00e7\u00e3o";
            this.lblFooterSecretaria.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // lblFooterDiretoria
            // 
            this.lblFooterDiretoria.AutoSize = false;
            this.lblFooterDiretoria.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblFooterDiretoria.ForeColor = Color.FromArgb(100, 116, 139); // Slate 500
            this.lblFooterDiretoria.Location = new Point(0, 46);
            this.lblFooterDiretoria.Name = "lblFooterDiretoria";
            this.lblFooterDiretoria.Size = new Size(620, 16);
            this.lblFooterDiretoria.Text = "Diretoria de Gest\u00e3o de Tecnologia da Informa\u00e7\u00e3o";
            this.lblFooterDiretoria.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // lblFooterVersao
            // 
            this.lblFooterVersao.AutoSize = false;
            this.lblFooterVersao.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblFooterVersao.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            this.lblFooterVersao.Location = new Point(0, 64);
            this.lblFooterVersao.Name = "lblFooterVersao";
            this.lblFooterVersao.Size = new Size(620, 16);
            this.lblFooterVersao.Text = "v1.0 - 2026";
            this.lblFooterVersao.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // panelFooter
            // 
            this.panelFooter.BackColor = Color.FromArgb(248, 250, 252); // Slate 50
            this.panelFooter.BorderStyle = BorderStyle.None;
            this.panelFooter.Controls.Add(this.lblFooterVersao);
            this.panelFooter.Controls.Add(this.lblFooterDiretoria);
            this.panelFooter.Controls.Add(this.lblFooterSecretaria);
            this.panelFooter.Controls.Add(this.lblFooterPrefeitura);
            this.panelFooter.Controls.Add(this.panelFooterLine);
            this.panelFooter.Dock = DockStyle.Bottom;
            this.panelFooter.Location = new Point(0, 568);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Padding = new Padding(28, 0, 28, 8);
            this.panelFooter.Size = new Size(620, 88);
            this.panelFooter.TabIndex = 2;

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(620, 656);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.panelFooter);
            this.Controls.Add(this.panelHeader);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Inventário Corporativo — OCS Inventory";
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.panelSysInfo.ResumeLayout(false);
            this.panelSysInfo.PerformLayout();
            this.panelAviso.ResumeLayout(false);
            this.panelAviso.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
