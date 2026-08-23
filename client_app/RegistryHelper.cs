using System;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace OCSCadastroApp
{
    /// <summary>
    /// Gerencia a verificacao e gravacao do status de conclusao e prazos no Registro do Windows e em disco.
    /// </summary>
    public static class RegistryHelper
    {
        private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Verifica se este computador/usuario ja possui o cadastro concluido com sucesso.
        /// </summary>
        public static bool IsCadastroConcluido()
        {
            try
            {
                // 1. Verifica no Registro HKEY_LOCAL_MACHINE
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryValueName);
                        if (val != null && val.ToString() == "1") return true;
                    }
                }

                // 2. Verifica no Registro HKEY_CURRENT_USER
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryValueName);
                        if (val != null && val.ToString() == "1") return true;
                    }
                }

                // 3. Verifica presenca de arquivo flag local em ProgramData ou LocalAppData
                string flagProgramData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"OCS_Inventario\concluido.flag"
                );
                if (File.Exists(flagProgramData)) return true;

                string flagLocalAppData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"OCS_Inventario\concluido.flag"
                );
                if (File.Exists(flagLocalAppData)) return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Obtem ou registra a data da primeira execucao da aplicacao nesta maquina.
        /// </summary>
        public static DateTime GetOrCreateFirstRunDate()
        {
            DateTime now = DateTime.Now;

            try
            {
                // 1. Tenta ler de HKLM
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryFirstRunValueName);
                        DateTime parsed;
                        if (val != null && DateTime.TryParseExact(val.ToString(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                        {
                            return parsed;
                        }
                    }
                }

                // 2. Tenta ler de HKCU
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null)
                    {
                        object val = key.GetValue(AppConfig.RegistryFirstRunValueName);
                        DateTime parsed;
                        if (val != null && DateTime.TryParseExact(val.ToString(), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                        {
                            return parsed;
                        }
                    }
                }

                // 3. Tenta ler de arquivo local
                string fileFirstRun = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"OCS_Inventario\first_run.dat"
                );
                if (File.Exists(fileFirstRun))
                {
                    string content = File.ReadAllText(fileFirstRun).Trim();
                    DateTime parsed;
                    if (DateTime.TryParseExact(content, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    {
                        return parsed;
                    }
                }
            }
            catch { }

            // Se nao encontrou, registra a data atual como primeira execucao
            SaveFirstRunDate(now);
            return now;
        }

        private static void SaveFirstRunDate(DateTime date)
        {
            string dateStr = date.ToString(DateFormat, CultureInfo.InvariantCulture);

            // Grava em HKLM (se possivel)
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null) key.SetValue(AppConfig.RegistryFirstRunValueName, dateStr, RegistryValueKind.String);
                }
            }
            catch { }

            // Grava em HKCU
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AppConfig.RegistrySubKey))
                {
                    if (key != null) key.SetValue(AppConfig.RegistryFirstRunValueName, dateStr, RegistryValueKind.String);
                }
            }
            catch { }

            // Grava em arquivo
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "OCS_Inventario"
                );
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "first_run.dat"), dateStr);
            }
            catch { }
        }

        /// <summary>
        /// Marca o computador/usuario como concluido de forma definitiva apos confirmacao do servidor.
        /// </summary>
        public static void MarcarCadastroConcluido(string patrimonio, string usuario)
        {
            string dataIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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
            catch { }

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
            catch { }

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
