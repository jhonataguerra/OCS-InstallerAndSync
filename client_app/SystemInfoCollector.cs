using System;
using System.Management;

namespace OCSCadastroApp
{
    /// <summary>
    /// Modelo que encapsula os dados coletados do computador.
    /// </summary>
    public class SystemData
    {
        public string Hostname { get; set; }
        public string UsuarioWindows { get; set; }
        public string VersaoWindows { get; set; }
        public string Arquitetura { get; set; }
        public string SerialBios { get; set; }
    }

    /// <summary>
    /// Coletor nativo de informacoes de hardware e sistema operacional compativel com Windows 7 ate 11.
    /// </summary>
    public static class SystemInfoCollector
    {
        public static SystemData Collect()
        {
            var data = new SystemData
            {
                Hostname = Environment.MachineName,
                UsuarioWindows = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName),
                VersaoWindows = GetWindowsVersion(),
                Arquitetura = GetSystemArchitecture(),
                SerialBios = GetBiosSerialNumber()
            };

            return data;
        }

        private static string GetSystemArchitecture()
        {
            try
            {
                // Verifica a variavel de ambiente ARCHITEW6432 (indica se um processo 32-bit roda sob OS 64-bit)
                string archW64 = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
                if (!string.IsNullOrEmpty(archW64) && archW64.IndexOf("64", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "64 bits";
                }

                string archProc = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                if (!string.IsNullOrEmpty(archProc) && (archProc.IndexOf("64", StringComparison.OrdinalIgnoreCase) >= 0 || archProc.IndexOf("AMD64", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return "64 bits";
                }

                if (IntPtr.Size == 8)
                {
                    return "64 bits";
                }
            }
            catch { }

            return "32 bits";
        }

        private static string GetWindowsVersion()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, CSDVersion, Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        if (os["Caption"] != null)
                        {
                            string caption = os["Caption"].ToString().Trim();
                            string sp = os["CSDVersion"] != null ? " " + os["CSDVersion"].ToString().Trim() : "";
                            return caption + sp;
                        }
                    }
                }
            }
            catch
            {
                // Fallback seguro caso WMI nao responda
            }

            return Environment.OSVersion.VersionString;
        }

        private static string GetBiosSerialNumber()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementObject bios in searcher.Get())
                    {
                        if (bios["SerialNumber"] != null)
                        {
                            string serial = bios["SerialNumber"].ToString().Trim();
                            if (!string.IsNullOrEmpty(serial))
                            {
                                return serial;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback seguro
            }

            return "NAO_IDENTIFICADO";
        }
    }
}
