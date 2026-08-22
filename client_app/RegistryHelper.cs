using System;
using System.IO;
using Microsoft.Win32;

namespace OCSCadastroApp
{
    /// <summary>
    /// Gerencia a verificacao e gravacao do status de conclusao no Registro do Windows e em disco.
    /// Suporta execucao como Administrador (HKLM) ou Usuario Padrao (HKCU).
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Verifica se este computador/usuario ja possui o cadastro concluido com sucesso.
        /// </summary>
        public static bool IsCadastroConcluido()
        {
            try
            {
                // 1. Verifica no Registro HKEY_LOCAL_MACHINE (nivel de maquina)
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryValueName);
                        if (val != null && val.ToString() == "1")
                        {
                            return true;
                        }
                    }
                }

                // 2. Verifica no Registro HKEY_CURRENT_USER (nivel de usuario)
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryValueName);
                        if (val != null && val.ToString() == "1")
                        {
                            return true;
                        }
                    }
                }

                // 3. Verifica presenca de arquivo flag local em ProgramData ou LocalAppData
                string flagProgramData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"OCS_Inventario\concluido.flag"
                );
                if (File.Exists(flagProgramData))
                {
                    return true;
                }

                string flagLocalAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"OCS_Inventario\concluido.flag"
                );
                if (File.Exists(flagLocalAppData))
                {
                    return true;
                }
            }
            catch
            {
                // Em caso de falha de leitura, prossegue permitindo a verificacao no formulario
            }

            return false;
        }

        /// <summary>
        /// Marca o computador/usuario como concluido de forma definitiva apos confirmacao do servidor.
        /// </summary>
        public static void MarcarCadastroConcluido(string patrimonio, string usuario)
        {
            string dataIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 1. Tenta gravar em HKLM (se tiver permissao de administrador/SYSTEM)
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        key.SetValue(AppConfig.RegistryValueName, 1, RegistryValueKind.DWord);
                        key.SetValue(AppConfig.RegistryDateValueName, dataIso, RegistryValueKind.String);
                        key.SetValue("Patrimonio", patrimonio ?? "", RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Ignora falha de permissao em HKLM para usuarios sem elevacao
            }

            // 2. Grava em HKCU (garantido para o usuario logado)
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        key.SetValue(AppConfig.RegistryValueName, 1, RegistryValueKind.DWord);
                        key.SetValue(AppConfig.RegistryDateValueName, dataIso, RegistryValueKind.String);
                        key.SetValue("Patrimonio", patrimonio ?? "", RegistryValueKind.String);
                    }
                }
            }
            catch
            {
                // Fallback adicional caso Registro esteja bloqueado por GPO restritiva
            }

            // 3. Grava arquivo flag em disco (fallback de persistencia)
            try
            {
                string dirProgramData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "OCS_Inventario"
                );
                if (!Directory.Exists(dirProgramData)) Directory.CreateDirectory(dirProgramData);
                File.WriteAllText(Path.Combine(dirProgramData, "concluido.flag"), dataIso + " - " + patrimonio);
            }
            catch
            {
                try
                {
                    string dirLocal = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "OCS_Inventario"
                    );
                    if (!Directory.Exists(dirLocal)) Directory.CreateDirectory(dirLocal);
                    File.WriteAllText(Path.Combine(dirLocal, "concluido.flag"), dataIso + " - " + patrimonio);
                }
                catch { }
            }
        }
    }
}
