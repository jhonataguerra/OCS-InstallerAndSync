using System;

namespace OCSCadastroApp
{
    /// <summary>
    /// Configuracoes centrais da aplicacao cliente de cadastro.
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// Chave de Registro utilizada para controlar a execucao e prazos.
        /// </summary>
        public const string RegistrySubKey = @"Software\OCS_Inventario";
        public const string RegistryValueName = "CadastroConcluido";
        public const string RegistryDateValueName = "DataCadastro";
        public const string RegistryFirstRunValueName = "PrimeiraExecucao";

        private static string _apiEndpointUrl = "http://192.168.2.48/cadastro_api/cadastrar.php";
        private static string _apiToken = "OCS_SEC_TOKEN_8f93e1b742a0489c93df51e7b99c2d15";
        private static bool _loadedFromRegistry = false;

        private static void EnsureLoaded()
        {
            if (_loadedFromRegistry) return;
            _loadedFromRegistry = true;
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RegistrySubKey))
                {
                    if (key != null)
                    {
                        var ep = key.GetValue("ApiEndpointUrl") as string;
                        if (!string.IsNullOrEmpty(ep)) _apiEndpointUrl = ep;

                        var tk = key.GetValue("ApiToken") as string;
                        if (!string.IsNullOrEmpty(tk)) _apiToken = tk;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// URL do endpoint da API de cadastro no servidor OCS.
        /// </summary>
        public static string ApiEndpointUrl
        {
            get { EnsureLoaded(); return _apiEndpointUrl; }
            set { _apiEndpointUrl = value; }
        }

        /// <summary>
        /// Token de seguranca obrigatorio (SEC-02) correspondente ao api/config.php.
        /// </summary>
        public static string ApiToken
        {
            get { EnsureLoaded(); return _apiToken; }
            set { _apiToken = value; }
        }

        /// <summary>
        /// Prazo em dias para o preenchimento se tornar obrigatorio (cronometro de 2 min).
        /// </summary>
        public const int DiasPrazoObrigatorio = 7;

        /// <summary>
        /// Tempo em segundos de bloqueio antes do vencimento (10 segundos).
        /// </summary>
        public const int SegundosBloqueioNormal = 10;

        /// <summary>
        /// Tempo em segundos de bloqueio apos o vencimento dos 7 dias (2 minutos = 120 segundos).
        /// </summary>
        public const int SegundosBloqueioObrigatorio = 120;
    }
}
