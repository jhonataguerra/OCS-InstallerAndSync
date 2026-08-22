using System.Drawing;
using System.Windows.Forms;

namespace OCSCadastroApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;

        private GroupBox grpUsuario;
        private Label lblNome;
        private TextBox txtNome;
        private Label lblPatrimonio;
        private TextBox txtPatrimonio;
        private Label lblSetor;
        private TextBox txtSetor;

        private GroupBox grpSistema;
        private Label lblSysHostname;
        private Label lblSysUsuario;
        private Label lblSysSerial;
        private Label lblSysSO;

        private Button btnEnviar;
        private Label lblStatus;

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
            this.panelHeader = new Panel();
            this.lblHeaderTitle = new Label();
            this.lblHeaderSubtitle = new Label();

            this.grpUsuario = new GroupBox();
            this.lblNome = new Label();
            this.txtNome = new TextBox();
            this.lblPatrimonio = new Label();
            this.txtPatrimonio = new TextBox();
            this.lblSetor = new Label();
            this.txtSetor = new TextBox();

            this.grpSistema = new GroupBox();
            this.lblSysHostname = new Label();
            this.lblSysUsuario = new Label();
            this.lblSysSerial = new Label();
            this.lblSysSO = new Label();

            this.btnEnviar = new Button();
            this.lblStatus = new Label();

            this.panelHeader.SuspendLayout();
            this.grpUsuario.SuspendLayout();
            this.grpSistema.SuspendLayout();
            this.SuspendLayout();

            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = Color.FromArgb(24, 90, 157);
            this.panelHeader.Controls.Add(this.lblHeaderSubtitle);
            this.panelHeader.Controls.Add(this.lblHeaderTitle);
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Location = new Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new Size(520, 75);
            this.panelHeader.TabIndex = 0;

            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblHeaderTitle.ForeColor = Color.White;
            this.lblHeaderTitle.Location = new Point(20, 14);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new Size(345, 25);
            this.lblHeaderTitle.Text = "Cadastro de Patrimônio e Equipamento";

            // 
            // lblHeaderSubtitle
            // 
            this.lblHeaderSubtitle.AutoSize = true;
            this.lblHeaderSubtitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblHeaderSubtitle.ForeColor = Color.FromArgb(215, 235, 255);
            this.lblHeaderSubtitle.Location = new Point(22, 43);
            this.lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            this.lblHeaderSubtitle.Size = new Size(450, 15);
            this.lblHeaderSubtitle.Text = "Preencha os dados abaixo para vincular seu equipamento ao inventario corporativo.";

            // 
            // grpUsuario
            // 
            this.grpUsuario.Controls.Add(this.txtSetor);
            this.grpUsuario.Controls.Add(this.lblSetor);
            this.grpUsuario.Controls.Add(this.txtPatrimonio);
            this.grpUsuario.Controls.Add(this.lblPatrimonio);
            this.grpUsuario.Controls.Add(this.txtNome);
            this.grpUsuario.Controls.Add(this.lblNome);
            this.grpUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.grpUsuario.Location = new Point(20, 88);
            this.grpUsuario.Name = "grpUsuario";
            this.grpUsuario.Size = new Size(480, 195);
            this.grpUsuario.TabIndex = 1;
            this.grpUsuario.TabStop = false;
            this.grpUsuario.Text = " Dados do Responsavel e Localizacao ";

            // 
            // lblNome
            // 
            this.lblNome.AutoSize = true;
            this.lblNome.Font = new Font("Segoe UI", 9F);
            this.lblNome.Location = new Point(16, 28);
            this.lblNome.Name = "lblNome";
            this.lblNome.Size = new Size(137, 15);
            this.lblNome.Text = "Nome Completo do Usuario:";

            // 
            // txtNome
            // 
            this.txtNome.Font = new Font("Segoe UI", 9.5F);
            this.txtNome.Location = new Point(19, 48);
            this.txtNome.MaxLength = 150;
            this.txtNome.Name = "txtNome";
            this.txtNome.Size = new Size(442, 24);
            this.txtNome.TabIndex = 2;

            // 
            // lblPatrimonio
            // 
            this.lblPatrimonio.AutoSize = true;
            this.lblPatrimonio.Font = new Font("Segoe UI", 9F);
            this.lblPatrimonio.Location = new Point(16, 80);
            this.lblPatrimonio.Name = "lblPatrimonio";
            this.lblPatrimonio.Size = new Size(168, 15);
            this.lblPatrimonio.Text = "Numero da Etiqueta de Patrimonio:";

            // 
            // txtPatrimonio
            // 
            this.txtPatrimonio.Font = new Font("Segoe UI", 9.5F);
            this.txtPatrimonio.Location = new Point(19, 100);
            this.txtPatrimonio.MaxLength = 50;
            this.txtPatrimonio.Name = "txtPatrimonio";
            this.txtPatrimonio.Size = new Size(442, 24);
            this.txtPatrimonio.TabIndex = 3;

            // 
            // lblSetor
            // 
            this.lblSetor.AutoSize = true;
            this.lblSetor.Font = new Font("Segoe UI", 9F);
            this.lblSetor.Location = new Point(16, 132);
            this.lblSetor.Name = "lblSetor";
            this.lblSetor.Size = new Size(125, 15);
            this.lblSetor.Text = "Setor / Departamento:";

            // 
            // txtSetor
            // 
            this.txtSetor.Font = new Font("Segoe UI", 9.5F);
            this.txtSetor.Location = new Point(19, 152);
            this.txtSetor.MaxLength = 100;
            this.txtSetor.Name = "txtSetor";
            this.txtSetor.Size = new Size(442, 24);
            this.txtSetor.TabIndex = 4;

            // 
            // grpSistema
            // 
            this.grpSistema.Controls.Add(this.lblSysSO);
            this.grpSistema.Controls.Add(this.lblSysSerial);
            this.grpSistema.Controls.Add(this.lblSysUsuario);
            this.grpSistema.Controls.Add(this.lblSysHostname);
            this.grpSistema.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            this.grpSistema.ForeColor = Color.FromArgb(70, 70, 70);
            this.grpSistema.Location = new Point(20, 290);
            this.grpSistema.Name = "grpSistema";
            this.grpSistema.Size = new Size(480, 95);
            this.grpSistema.TabIndex = 5;
            this.grpSistema.TabStop = false;
            this.grpSistema.Text = " Informacoes Tecnicas Detectadas ";

            // 
            // lblSysHostname
            // 
            this.lblSysHostname.AutoSize = true;
            this.lblSysHostname.Font = new Font("Segoe UI", 8.25F);
            this.lblSysHostname.Location = new Point(16, 22);
            this.lblSysHostname.Name = "lblSysHostname";
            this.lblSysHostname.Size = new Size(62, 13);
            this.lblSysHostname.Text = "Hostname: ";

            // 
            // lblSysUsuario
            // 
            this.lblSysUsuario.AutoSize = true;
            this.lblSysUsuario.Font = new Font("Segoe UI", 8.25F);
            this.lblSysUsuario.Location = new Point(240, 22);
            this.lblSysUsuario.Name = "lblSysUsuario";
            this.lblSysUsuario.Size = new Size(76, 13);
            this.lblSysUsuario.Text = "Usuario Atual: ";

            // 
            // lblSysSerial
            // 
            this.lblSysSerial.AutoSize = true;
            this.lblSysSerial.Font = new Font("Segoe UI", 8.25F);
            this.lblSysSerial.Location = new Point(16, 44);
            this.lblSysSerial.Name = "lblSysSerial";
            this.lblSysSerial.Size = new Size(68, 13);
            this.lblSysSerial.Text = "Serial BIOS: ";

            // 
            // lblSysSO
            // 
            this.lblSysSO.AutoSize = true;
            this.lblSysSO.Font = new Font("Segoe UI", 8.25F);
            this.lblSysSO.Location = new Point(16, 66);
            this.lblSysSO.Name = "lblSysSO";
            this.lblSysSO.Size = new Size(99, 13);
            this.lblSysSO.Text = "Sistema Operacional: ";

            // 
            // btnEnviar
            // 
            this.btnEnviar.BackColor = Color.FromArgb(24, 134, 75);
            this.btnEnviar.Cursor = Cursors.Hand;
            this.btnEnviar.FlatStyle = FlatStyle.Flat;
            this.btnEnviar.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            this.btnEnviar.ForeColor = Color.White;
            this.btnEnviar.Location = new Point(160, 396);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new Size(200, 38);
            this.btnEnviar.TabIndex = 6;
            this.btnEnviar.Text = "Gravar e Concluir";
            this.btnEnviar.UseVisualStyleBackColor = false;
            this.btnEnviar.Click += new System.EventHandler(this.BtnEnviar_Click);

            // 
            // lblStatus
            // 
            this.lblStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            this.lblStatus.ForeColor = Color.FromArgb(90, 90, 90);
            this.lblStatus.Location = new Point(20, 440);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(480, 20);
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(246, 248, 250);
            this.ClientSize = new Size(520, 470);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.grpSistema);
            this.Controls.Add(this.grpUsuario);
            this.Controls.Add(this.panelHeader);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Inventario de Equipamento - OCS";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.grpUsuario.ResumeLayout(false);
            this.grpUsuario.PerformLayout();
            this.grpSistema.ResumeLayout(false);
            this.grpSistema.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
