using System;
using System.Threading;
using System.Windows.Forms;

namespace OCSCadastroApp
{
    static class Program
    {
        private static Mutex _appMutex;

        [STAThread]
        static void Main(string[] args)
        {
            // 1. Garante que apenas uma instancia do executavel rode ao mesmo tempo
            bool isNewInstance;
            _appMutex = new Mutex(true, "Global\\OCSCadastroPatrimonio_SingleInstanceMutex", out isNewInstance);
            if (!isNewInstance)
            {
                // Ja existe uma instancia em execucao
                return;
            }

            // 2. REGRA PRINCIPAL: Verifica se o computador/usuario ja possui cadastro concluido com sucesso
            // Se ja foi concluido, encerra imediatamente sem exibir o formulario
            if (RegistryHelper.IsCadastroConcluido())
            {
                return;
            }

            // 3. Inicializacao do ambiente grafico Windows Forms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                if (_appMutex != null)
                {
                    _appMutex.ReleaseMutex();
                    _appMutex.Close();
                }
            }
        }
    }
}
