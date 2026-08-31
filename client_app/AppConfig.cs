using System;

namespace OCSCadastroApp
{
    /// <summary>
    /// Configuracoes centrais da aplicacao cliente de cadastro.
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// URL do endpoint da API de cadastro no servidor OCS.
        /// </summary>
        public static string ApiEndpointUrl = "http://192.168.2.48/cadastro_api/cadastrar.php";

        /// <summary>
        /// Token de seguranca obrigatorio (SEC-02) correspondente ao api/config.php.
        /// </summary>
        public static string ApiToken = "OCS_SEC_TOKEN_8f93e1b742a0489c93df51e7b99c2d15";

        /// <summary>
        /// Chave de Registro utilizada para controlar a execucao e prazos.
        /// </summary>
        public const string RegistrySubKey = @"Software\OCS_Inventario";
        public const string RegistryValueName = "CadastroConcluido";
        public const string RegistryDateValueName = "DataCadastro";
        public const string RegistryFirstRunValueName = "PrimeiraExecucao";

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
