using System;
using System.Collections.Generic;
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
    /// Inclui tratamento avancado para seriais genericos de BIOS (ex: "To be filled by O.E.M.").
    /// </summary>
    public static class SystemInfoCollector
    {
        // Lista de valores genericos comuns inseridos por fabricantes de placas-mae
        private static readonly HashSet<string> BlacklistSerials = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "To be filled by O.E.M.",
            "To be filled by O.E.M",
            "To Be Filled By O.E.M.",
            "To Be Filled By O.E.M",
            "Default string",
            "System Serial Number",
            "0123456789",
            "123456789",
            "None",
            "N/A",
            "Null",
            "Empty",
            "Chassis Serial Number",
            "Not Specified"
        };

        public static SystemData Collect()
        {
            var data = new SystemData
            {
                Hostname = Environment.MachineName,
                UsuarioWindows = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName),
                VersaoWindows = GetWindowsVersion(),
                Arquitetura = GetSystemArchitecture(),
                SerialBios = GetValidSerialNumber()
            };

            return data;
        }

        private static string GetSystemArchitecture()
        {
            try
            {
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
            catch { }

            return Environment.OSVersion.VersionString;
        }

        /// <summary>
        /// Obtem o numero de serie da maquina, filtrando valores genericos ("To be filled by O.E.M.")
        /// e buscando alternativas na placa-mae ou UUID do produto.
        /// </summary>
        private static string GetValidSerialNumber()
        {
            // 1. Tenta obter o Serial Number da BIOS (Win32_BIOS)
            string biosSerial = QueryWmiProperty("SELECT SerialNumber FROM Win32_BIOS", "SerialNumber");
            if (IsValidSerial(biosSerial))
            {
                return biosSerial;
            }

            // 2. Se a BIOS for generica, tenta o Serial da Placa-Mae (Win32_BaseBoard)
            string boardSerial = QueryWmiProperty("SELECT SerialNumber FROM Win32_BaseBoard", "SerialNumber");
            if (IsValidSerial(boardSerial))
            {
                return boardSerial;
            }

            // 3. Se ainda for generico, tenta o UUID do Sistema (Win32_ComputerSystemProduct)
            string systemUuid = QueryWmiProperty("SELECT UUID FROM Win32_ComputerSystemProduct", "UUID");
            if (IsValidSerial(systemUuid) && !systemUuid.Equals("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", StringComparison.OrdinalIgnoreCase))
            {
                return "UUID-" + systemUuid;
            }

            // 4. Caso nao haja serial valido nos componentes de hardware
            return "GENERICO_OEM";
        }

        private static bool IsValidSerial(string serial)
        {
            if (string.IsNullOrEmpty(serial)) return false;
            string clean = serial.Trim();
            if (clean.Length < 3) return false;
            if (BlacklistSerials.Contains(clean)) return false;

            return true;
        }

        private static string QueryWmiProperty(string query, string propertyName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj[propertyName] != null)
                        {
                            string val = obj[propertyName].ToString().Trim();
                            if (!string.IsNullOrEmpty(val))
                            {
                                return val;
                            }
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
