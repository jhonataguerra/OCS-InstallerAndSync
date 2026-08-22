using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace OCSCadastroApp
{
    public partial class MainForm : Form
    {
        private SystemData _sysData;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Coleta dados automaticamente no carregamento
                _sysData = SystemInfoCollector.Collect();

                lblSysHostname.Text = "Hostname: " + _sysData.Hostname;
                lblSysUsuario.Text = "Usuario Atual: " + _sysData.UsuarioWindows;
                lblSysSerial.Text = "Serial BIOS: " + _sysData.SerialBios;
                lblSysSO.Text = string.Format("SO: {0} ({1})", _sysData.VersaoWindows, _sysData.Arquitetura);

                // Sugere nome baseado no usuario do Windows se aplicavel
                string userNameOnly = Environment.UserName;
                if (!string.IsNullOrEmpty(userNameOnly) && !userNameOnly.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    // Deixa focado no campo nome
                    txtNome.Focus();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Aviso ao obter informacoes do sistema: " + ex.Message;
            }
        }

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string patrimonio = txtPatrimonio.Text.Trim();
            string setor = txtSetor.Text.Trim();

            // 1. Validacao de campos
            if (string.IsNullOrEmpty(nome))
            {
                MessageBox.Show("Por favor, preencha o seu Nome Completo.", "Campo Obrigatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (string.IsNullOrEmpty(patrimonio))
            {
                MessageBox.Show("Por favor, informe o Numero de Patrimonio do equipamento.", "Campo Obrigatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPatrimonio.Focus();
                return;
            }

            if (string.IsNullOrEmpty(setor))
            {
                MessageBox.Show("Por favor, informe o Setor ou Departamento.", "Campo Obrigatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSetor.Focus();
                return;
            }

            // 2. Bloqueia botoes durante o envio
            btnEnviar.Enabled = false;
            btnEnviar.Text = "Enviando...";
            lblStatus.Text = "Conectando ao servidor e registrando dados...";
            lblStatus.ForeColor = Color.DarkBlue;
            Application.DoEvents();

            try
            {
                // Monta JSON de forma nativa e compativel (sem depender de DLLs externas)
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

                // Configura requisicao HTTP
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AppConfig.ApiEndpointUrl);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.ContentLength = postBytes.Length;
                request.Timeout = 12000; // 12 segundos de timeout
                request.ReadWriteTimeout = 12000;

                // Suporte a SSL/TLS seguro em Windows legados
                try
                {
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls; // TLS 1.2 + legados
                }
                catch { }

                if (!string.IsNullOrEmpty(AppConfig.ApiToken))
                {
                    request.Headers.Add("X-API-TOKEN", AppConfig.ApiToken);
                }

                // Envia dados
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(postBytes, 0, postBytes.Length);
                }

                // Le resposta
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

                        MessageBox.Show(
                            "Cadastro do equipamento concluido com sucesso!\n\nOs dados foram vinculados ao inventario.",
                            "Sucesso",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                    else
                    {
                        throw new Exception("Servidor respondeu com codigo HTTP: " + (int)response.StatusCode);
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
                    "Nao foi possivel enviar os dados para o servidor.\nVerifique a conexao de rede e tente novamente." + extraMsg,
                    "Falha de Comunicacao",
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
