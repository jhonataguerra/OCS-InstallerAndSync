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
        /// Altere para o IP ou FQDN do seu servidor OCS.
        /// </summary>
        public static string ApiEndpointUrl = "http://192.168.15.20/cadastro_api/cadastrar.php";

        /// <summary>
        /// Token opcional configurado em api/config.php (deixe vazio se nao utilizar).
        /// </summary>
        public static string ApiToken = "";

        /// <summary>
        /// Chave de Registro utilizada para controlar a execucao unica por maquina/usuario.
        /// </summary>
        public const string RegistrySubKey = @"Software\OCS_Inventario";
        public const string RegistryValueName = "CadastroConcluido";
        public const string RegistryDateValueName = "DataCadastro";
    }
}
