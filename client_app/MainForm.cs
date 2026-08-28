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
                // 1. Coleta automática das informações de hardware e sistema
                _sysData = SystemInfoCollector.Collect();

                lblValHostname.Text = _sysData.Hostname;
                lblValUsuario.Text = _sysData.UsuarioWindows;
                lblValSerial.Text = _sysData.SerialBios;
                lblValSO.Text = string.Format("{0} ({1})", _sysData.VersaoWindows, _sysData.Arquitetura);

                // 2. Cálculo do prazo de 7 dias e parametrização do cronômetro
                DateTime dataPrimeiraExecucao = RegistryHelper.GetOrCreateFirstRunDate();
                TimeSpan decorrido = DateTime.Now - dataPrimeiraExecucao;
                int diasRestantes = AppConfig.DiasPrazoObrigatorio - (int)decorrido.TotalDays;

                if (diasRestantes > 0)
                {
                    // Período de tolerância (primeiros 7 dias)
                    _isObrigatorio = false;
                    _segundosRestantes = AppConfig.SegundosBloqueioNormal; // 10 segundos

                    panelAviso.BackColor = Color.FromArgb(254, 243, 199); // Amber 100
                    lblAvisoIcon.Text = "ℹ";
                    lblAvisoIcon.ForeColor = Color.FromArgb(180, 83, 9); // Amber 700
                    lblAvisoTitulo.ForeColor = Color.FromArgb(120, 53, 15); // Amber 900
                    lblAvisoTitulo.Text = string.Format("Preenchimento Pendente ({0} dia(s) restantes para se tornar obrigatório)", diasRestantes);
                    lblAvisoDescricao.ForeColor = Color.FromArgb(146, 64, 14); // Amber 800
                    lblAvisoDescricao.Text = "Por favor, identifique seu equipamento. Após enviado com sucesso, esta tela não será mais exibida.";
                }
                else
                {
                    // Prazo expirado: preenchimento torna-se obrigatório (bloqueio de 2 minutos)
                    _isObrigatorio = true;
                    _segundosRestantes = AppConfig.SegundosBloqueioObrigatorio; // 120 segundos

                    panelAviso.BackColor = Color.FromArgb(254, 226, 226); // Rose 100
                    lblAvisoIcon.Text = "⚠";
                    lblAvisoIcon.ForeColor = Color.FromArgb(185, 28, 28); // Rose 700
                    lblAvisoTitulo.ForeColor = Color.FromArgb(153, 27, 27); // Rose 900
                    lblAvisoTitulo.Text = "Atenção: O prazo de tolerância expirou e o cadastro tornou-se OBRIGATÓRIO";
                    lblAvisoDescricao.ForeColor = Color.FromArgb(153, 27, 27); // Rose 900
                    lblAvisoDescricao.Text = "Identifique seu equipamento agora. Após preenchido e gravado, esta janela não será mais exibida.";
                }

                // Inicializa o botão de fechar com a contagem regressiva
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
                // Tempo encerrado: habilita fechamento temporário
                timerBloqueio.Stop();
                _podeFechar = true;
                btnFechar.Text = "Fechar Temporariamente";
                btnFechar.Enabled = true;
                btnFechar.Cursor = Cursors.Hand;
                btnFechar.BackColor = Color.FromArgb(226, 232, 240);
                btnFechar.ForeColor = Color.FromArgb(30, 41, 59);
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
            if (!_podeFechar && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                string tempoTexto = _segundosRestantes >= 60 
                    ? string.Format("{0} minuto(s) e {1} segundo(s)", _segundosRestantes / 60, _segundosRestantes % 60)
                    : string.Format("{0} segundo(s)", _segundosRestantes);

                string msg = _isObrigatorio
                    ? string.Format("O preenchimento deste inventário é obrigatório.\nAguarde mais {0} para poder fechar temporariamente.", tempoTexto)
                    : string.Format("Por favor, aguarde a leitura das instruções ({0}) para fechar.", tempoTexto);

                MessageBox.Show(msg, "Inventário Corporativo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #region Validação e Filtragem de Campos

        // Nome do Responsável: Aceita exclusivamente letras, acentuação em PT-BR, espaços, hífens e apóstrofos
        private void TxtNome_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (char.IsLetter(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == '-' || e.KeyChar == '\'')
            {
                return;
            }

            e.Handled = true; // Bloqueia números e símbolos
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

        // Nº de Patrimônio: Aceita estritamente dígitos numéricos (0 a 9)
        private void TxtPatrimonio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            e.Handled = true; // Bloqueia letras e caracteres especiais
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

            // 1. Validação de preenchimento mínimo
            if (string.IsNullOrEmpty(nome) || nome.Length < 3)
            {
                MessageBox.Show("Por favor, informe o Nome do Responsável (mínimo 3 caracteres).", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (string.IsNullOrEmpty(patrimonio))
            {
                MessageBox.Show("Por favor, informe o Nº de Patrimônio do equipamento.", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPatrimonio.Focus();
                return;
            }

            if (string.IsNullOrEmpty(setor))
            {
                MessageBox.Show("Por favor, informe o Setor ou Local de trabalho.", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSetor.Focus();
                return;
            }

            // 2. Bloqueia botões durante a transmissão
            btnEnviar.Enabled = false;
            btnEnviar.Text = "Gravando...";
            lblStatus.Text = "Transmitindo informações ao servidor de inventário...";
            lblStatus.ForeColor = Color.FromArgb(30, 64, 175);
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

                // Não utilizar o proxy configurado no Windows.
                // A API está hospedada na rede interna.
                request.Proxy = null;

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

                        // 3. Sucesso confirmado: Grava flag definitiva local e encerra
                        RegistryHelper.MarcarCadastroConcluido(patrimonio, _sysData.UsuarioWindows);

                        _podeFechar = true;
                        timerBloqueio.Stop();

                        MessageBox.Show(
                            "Cadastro de patrimônio concluído com sucesso!\n\nAs informações foram vinculadas ao inventário corporativo.",
                            "Identificação Concluída",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                    else
                    {
                        throw new Exception("O servidor respondeu com código HTTP: " + (int)response.StatusCode);
                    }
                }
            }
            catch (WebException webEx)
            {
                string mensagem = webEx.Message;

                if (webEx.Response != null)
                {
                    try
                    {
                        using (HttpWebResponse errorResponse =
                            (HttpWebResponse)webEx.Response)
                        {
                            mensagem += Environment.NewLine +
                                        Environment.NewLine +
                                        "Código HTTP: " +
                                        (int)errorResponse.StatusCode +
                                        " - " +
                                        errorResponse.StatusDescription;
                        }
                    }
                    catch
                    {
                    }
                }

                MessageBox.Show(
                    "Não foi possível conectar ao servidor de inventário." +
                    Environment.NewLine +
                    Environment.NewLine +
                    mensagem,
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
