// Zen Media Saver - Reklamsız Medya ve Müzik İndirici
// Geliştirici: Serkan ŞENTÜRK
// Telif Hakkı © 2026 Serkan ŞENTÜRK. Tüm Hakları Saklıdır.

using System;
using System.Threading;
using System.Windows;

namespace ZenMediaSaver
{
    public class App : Application
    {
        private static Mutex _mutex;

        [STAThread]
        public static void Main()
        {
            bool createdNew;
            _mutex = new Mutex(true, "ZenMediaSaver_SingleInstance_SerkanSenturk_2026", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "Zen Media Saver zaten açık!\nLütfen görev çubuğundan pencereye geçiş yapın.\n\nGeliştirici: Serkan ŞENTÜRK",
                    "Zen Media Saver",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            App app = new App();
            app.Run(new MainWindow());
        }
    }
}
