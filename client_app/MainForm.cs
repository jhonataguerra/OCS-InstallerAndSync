using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OCSCadastroApp
{
    public partial class MainForm : Form
    {
        private SystemData _sysData;
        private int _segundosRestantes;
        private bool _podeFechar = false;
        private bool _isObrigatorio = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Coleta dados de hardware e sistema
                _sysData = SystemInfoCollector.Collect();

                lblSysHostname.Text = "Hostname: " + _sysData.Hostname;
                lblSysUsuario.Text = "Usuario Atual: " + _sysData.UsuarioWindows;
                lblSysSerial.Text = "Serial BIOS: " + _sysData.SerialBios;
                lblSysSO.Text = string.Format("SO: {0} ({1})", _sysData.VersaoWindows, _sysData.Arquitetura);

                // 2. Calculo do prazo de 7 dias e configuracao do cronometro
                DateTime dataPrimeiraExecucao = RegistryHelper.GetOrCreateFirstRunDate();
                TimeSpan decorrido = DateTime.Now - dataPrimeiraExecucao;
                int diasRestantes = AppConfig.DiasPrazoObrigatorio - (int)decorrido.TotalDays;

                if (diasRestantes > 0)
                {
                    // Periodo de tolerancia (primeiros 7 dias)
                    _isObrigatorio = false;
                    _segundosRestantes = AppConfig.SegundosBloqueioNormal; // 10 segundos

                    panelAviso.BackColor = Color.FromArgb(254, 243, 199);
                    panelAviso.BorderStyle = BorderStyle.FixedSingle;
                    lblAvisoIcon.Text = "[!]";
                    lblAvisoIcon.ForeColor = Color.FromArgb(180, 83, 9);
                    lblAvisoPrazo.ForeColor = Color.FromArgb(120, 53, 15);
                    lblAvisoPrazo.Text = string.Format(
                        "Preenchimento necessário ({0} dia(s) restantes para se tornar obrigatório).\nApós preenchido e enviado, essa janela não será mais exibida.",
                        diasRestantes
                    );
                }
                else
                {
                    // Prazo expirado: preenchimento se torna obrigatorio (bloqueio de 2 minutos)
                    _isObrigatorio = true;
                    _segundosRestantes = AppConfig.SegundosBloqueioObrigatorio; // 120 segundos (2 min)

                    panelAviso.BackColor = Color.FromArgb(254, 226, 226);
                    panelAviso.BorderStyle = BorderStyle.FixedSingle;
                    lblAvisoIcon.Text = "[X]";
                    lblAvisoIcon.ForeColor = Color.FromArgb(185, 28, 28);
                    lblAvisoPrazo.ForeColor = Color.FromArgb(153, 27, 27);
                    lblAvisoPrazo.Font = new Font(lblAvisoPrazo.Font, FontStyle.Bold);
                    lblAvisoPrazo.Text = "ATENÇÃO: Prazo expirado! O preenchimento agora é OBRIGATÓRIO.\nApós preenchido e enviado, essa janela não será mais exibida.";
                }

                // Inicia o cronometro de contagem regressiva para permitir fechar
                btnFechar.Text = string.Format("Fechar ({0}s)", _segundosRestantes);
                btnFechar.Enabled = false;
                timerBloqueio.Start();

                txtNome.Focus();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Aviso ao iniciar: " + ex.Message;
            }
        }

        private void TimerBloqueio_Tick(object sender, EventArgs e)
        {
            _segundosRestantes--;

            if (_segundosRestantes > 0)
            {
                if (_segundosRestantes >= 60)
                {
                    int min = _segundosRestantes / 60;
                    int sec = _segundosRestantes % 60;
                    btnFechar.Text = string.Format("Fechar ({0}m {1:D2}s)", min, sec);
                }
                else
                {
                    btnFechar.Text = string.Format("Fechar ({0}s)", _segundosRestantes);
                }
            }
            else
            {
                // Tempo esgotado: habilita o botao de fechar temporariamente
                timerBloqueio.Stop();
                _podeFechar = true;
                btnFechar.Text = "Fechar Temporariamente";
                btnFechar.Enabled = true;
                btnFechar.Cursor = Cursors.Hand;
                btnFechar.ForeColor = Color.FromArgb(50, 50, 50);
            }
        }

        private void BtnFechar_Click(object sender, EventArgs e)
        {
            if (_podeFechar)
            {
                this.Close();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Se o usuario tentar fechar pelo 'X' antes do cronometro zerar
            if (!_podeFechar && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                string tempoTexto = _segundosRestantes >= 60 
                    ? string.Format("{0} minuto(s) e {1} segundo(s)", _segundosRestantes / 60, _segundosRestantes % 60)
                    : string.Format("{0} segundo(s)", _segundosRestantes);

                string msg = _isObrigatorio
                    ? string.Format("O preenchimento e obrigatorio.\nAguarde mais {0} para poder fechar temporariamente.", tempoTexto)
                    : string.Format("Aguarde a leitura do aviso ({0}) para poder fechar.", tempoTexto);

                MessageBox.Show(msg, "Aviso de Inventario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #region Tratamento e Validacao de Campos

        // Nome do Responsavel: Aceita apenas letras, espacos e caracteres de nome
        private void TxtNome_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (char.IsLetter(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == '-' || e.KeyChar == '\'')
            {
                return;
            }

            e.Handled = true;
        }

        private void TxtNome_TextChanged(object sender, EventArgs e)
        {
            string original = txtNome.Text;
            string limpo = Regex.Replace(original, @"[^a-zA-ZÀ-ÿ\s\-\']", "");
            if (original != limpo)
            {
                int cursor = txtNome.SelectionStart;
                txtNome.Text = limpo;
                txtNome.SelectionStart = Math.Min(cursor, limpo.Length);
            }
        }

        // Numero de Patrimonio: Aceita estritamente numeros (digitos 0-9)
        private void TxtPatrimonio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            e.Handled = true;
        }

        private void TxtPatrimonio_TextChanged(object sender, EventArgs e)
        {
            string original = txtPatrimonio.Text;
            string limpo = Regex.Replace(original, @"[^\d]", "");
            if (original != limpo)
            {
                int cursor = txtPatrimonio.SelectionStart;
                txtPatrimonio.Text = limpo;
                txtPatrimonio.SelectionStart = Math.Min(cursor, limpo.Length);
            }
        }

        #endregion

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string patrimonio = txtPatrimonio.Text.Trim();
            string setor = txtSetor.Text.Trim();

            // 1. Validacoes dos campos obrigatorios
            if (string.IsNullOrEmpty(nome) || nome.Length < 3)
            {
                MessageBox.Show("Por favor, preencha o Nome do Responsável (somente letras, mínimo 3 caracteres).", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (string.IsNullOrEmpty(patrimonio))
            {
                MessageBox.Show("Por favor, informe o Número de Patrimônio (somente números).", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPatrimonio.Focus();
                return;
            }

            if (string.IsNullOrEmpty(setor))
            {
                MessageBox.Show("Por favor, informe o Setor ou Departamento.", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSetor.Focus();
                return;
            }

            // 2. Bloqueia botoes durante o envio
            btnEnviar.Enabled = false;
            btnEnviar.Text = "Gravando...";
            lblStatus.Text = "Conectando ao servidor e registrando dados...";
            lblStatus.ForeColor = Color.DarkBlue;
            Application.DoEvents();

            try
            {
                string jsonPayload = string.Format(
                    "{{\"hostname\":\"{0}\",\"nome_completo\":\"{1}\",\"numero_patrimonio\":\"{2}\",\"setor_local\":\"{3}\",\"usuario_windows\":\"{4}\",\"versao_windows\":\"{5}\",\"arquitetura\":\"{6}\",\"serial_bios\":\"{7}\"}}",
                    EscapeJson(_sysData.Hostname),
                    EscapeJson(nome),
                    EscapeJson(patrimonio),
                    EscapeJson(setor),
                    EscapeJson(_sysData.UsuarioWindows),
                    EscapeJson(_sysData.VersaoWindows),
                    EscapeJson(_sysData.Arquitetura),
                    EscapeJson(_sysData.SerialBios)
                );

                byte[] postBytes = Encoding.UTF8.GetBytes(jsonPayload);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AppConfig.ApiEndpointUrl);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = postBytes.Length;
                request.Timeout = 12000;
                request.ReadWriteTimeout = 12000;

                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                }
                catch { }

                if (!string.IsNullOrEmpty(AppConfig.ApiToken))
                {
                    request.Headers.Add("X-API-TOKEN", AppConfig.ApiToken);
                }

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(postBytes, 0, postBytes.Length);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        {
                            string responseBody = reader.ReadToEnd();
                        }

                        // 3. Sucesso confirmado pelo servidor: Grava flag definitiva local
                        RegistryHelper.MarcarCadastroConcluido(patrimonio, _sysData.UsuarioWindows);

                        _podeFechar = true;
                        timerBloqueio.Stop();

                        MessageBox.Show(
                            "Cadastro do equipamento concluído com sucesso!\n\nOs dados foram vinculados ao inventário corporativo.",
                            "Cadastro Concluído",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                    else
                    {
                        throw new Exception("Servidor respondeu com código HTTP: " + (int)response.StatusCode);
                    }
                }
            }
            catch (WebException webEx)
            {
                string extraMsg = "";
                if (webEx.Response != null)
                {
                    try
                    {
                        using (StreamReader r = new StreamReader(webEx.Response.GetResponseStream()))
                        {
                            extraMsg = "\nResposta do servidor: " + r.ReadToEnd();
                        }
                    }
                    catch { }
                }

                MessageBox.Show(
                    "Não foi possível enviar os dados para o servidor.\nVerifique a conexão de rede e tente novamente." + extraMsg,
                    "Falha de Comunicação",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocorreu um erro ao processar o cadastro:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnEnviar.Enabled = true;
                btnEnviar.Text = "Gravar e Concluir";
                lblStatus.Text = "";
            }
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t");
        }
    }
}
