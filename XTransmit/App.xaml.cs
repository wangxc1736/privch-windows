﻿using MaterialDesignThemes.Wpf;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using XTransmit.Control;
using XTransmit.Model;
using XTransmit.Utility;

namespace XTransmit
{
    /**
     * TODO - English, Chinese language
     * TODO - Check memory leak, stream close, object dispose.
     * TODO - Add support for Remote Http Proxy, Trojan ...
     * TODO - Auto search and add servers
     * TODO - Auto detect and remove invalid servers
     * TODO - Use Task instead BackgroundWorker
     * 
     * NOTE
     * System.Text.Json don't support System.Runtime.Serialization [DataContract], [DataMember]
     * Compared to DataGrid, ListView comes with "*" column width, double click, row sort and application command problems
     * EventHandler name accept "_"
     */
    public partial class App : Application
    {
        public static string Name { get; private set; }

        public static string DirectoryApplication { get; private set; }
        public static string DirectoryPrivoxy { get; private set; }
        public static string DirectoryShadowsocks { get; private set; }
        public static string DirectoryV2Ray { get; private set; }

        public static string FileApplication { get; private set; }
        public static string FilePreferenceXml { get; private set; }
        public static string FileConfigXml { get; private set; }
        public static string FileIPAddressXml { get; private set; }
        public static string FileUserAgentXml { get; private set; }

        public static string FileShadowsocksXml { get; private set; }
        public static string FileV2RayXml { get; private set; }


        public static void CloseMainWindow()
        {
            SettingManager.Appearance.IsWindowHomeVisible = Current.MainWindow.IsVisible;
            Current.MainWindow.Hide();
            Current.MainWindow.Close();
        }

        public static void ShowMainWindow()
        {
            if (Current.MainWindow.IsVisible)
            {
                if (Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    Current.MainWindow.WindowState = WindowState.Normal;
                }

                Current.MainWindow.Activate();
            }
            else
            {
                Current.MainWindow.Show();
            }
        }

        /** Application ===============================================================================
         */
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string dirData = "data";
            string dirBin = "binary";

            // to avoid loading WindowHome on startup fails
            StartupUri = new System.Uri("View/WindowShutdown.xaml", System.UriKind.Relative);

            // single instance
            if (SystemUtil.IsProcessExist("XTransmit"))
            {
                Shutdown();
                return;
            }

            // init directory
            Name = (string)Current.FindResource("app_name");

            //FileApplication = System.Reflection.Assembly.GetEntryAssembly().Location;
            FileApplication = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryApplication = Path.GetDirectoryName(FileApplication);

            try
            {
                Directory.CreateDirectory($@"{DirectoryApplication}\{dirBin}");
                Directory.CreateDirectory($@"{DirectoryApplication}\{dirData}");
            }
            catch
            {
                string message = (string)FindResource("app_init_fail");
                new View.DialogPrompt(Name, message).ShowDialog();

                Shutdown();
                return;
            }

            DirectoryPrivoxy = $@"{DirectoryApplication}\{dirBin}\privoxy";
            DirectoryShadowsocks = $@"{DirectoryApplication}\{dirBin}\shadowsocks";
            DirectoryV2Ray = $@"{DirectoryApplication}\{dirBin}\v2ray";

            FilePreferenceXml = $@"{DirectoryApplication}\{dirData}\Preference.xml";
            FileConfigXml = $@"{DirectoryApplication}\{dirData}\Config.xml";
            FileIPAddressXml = $@"{DirectoryApplication}\{dirData}\IPAddress.xml"; //china ip optimized
            FileUserAgentXml = $@"{DirectoryApplication}\{dirData}\UserAgent.xml";

            FileShadowsocksXml = $@"{DirectoryApplication}\{dirData}\ServerShadowsocks.xml";
            FileV2RayXml = $@"{DirectoryApplication}\{dirData}\ServerV2Ray.xml";

            // initialize binaries
            ProcPrivoxy.KillRunning();
            ProcSS.KillRunning();
            ProcV2Ray.KillRunning();
            if (!ProcPrivoxy.Prepare() || !ProcSS.Prepare() || !ProcV2Ray.Prepare())
            {
                string message = (string)FindResource("app_init_fail");
                new View.DialogPrompt(Name, message).ShowDialog();

                Shutdown();
                return;
            }

            // load data
            ServerManager.Load(FileShadowsocksXml, FileV2RayXml);
            SettingManager.LoadFileOrDefault(FileConfigXml, FilePreferenceXml);

            // initialize transmit
            if (!TransmitCtrl.StartServer())
            {
                string message = (string)FindResource("app_service_fail");
                new View.DialogPrompt(Name, message).ShowDialog();

                Shutdown();
                return;
            }

            TransmitCtrl.EnableTransmit(SettingManager.Configuration.IsTransmitEnabled);

            // initialize interface and theme
            InterfaceCtrl.ModifyTheme(theme => theme.SetBaseTheme(
                SettingManager.Appearance.IsDarkTheme ? Theme.Dark : Theme.Light));

            // done
            StartupUri = new System.Uri("View/WindowHome.xaml", System.UriKind.Relative);
            Exit += Application_Exit;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // save settings and fix autorun status. reduce startup time
            SettingManager.WriteFile(FileConfigXml, FilePreferenceXml);
            if (SettingManager.Configuration.IsAutorun)
            {
                SystemUtil.CheckOrCreateUserStartupShortcut();
            }
            else
            {
                SystemUtil.DeleteUserStartupShortcuts();
            }

            // shutdown service
            TransmitCtrl.StopServer();
            TransmitCtrl.EnableTransmit(false);

            InterfaceCtrl.Dispose();
            Model.IPAddress.IPInformation.Dispose();

            // not important
            ProcPrivoxy.KillRunning(); 
            ProcSS.KillRunning();
            ProcV2Ray.KillRunning();
        }

        // Something wrong happen, Unexpercted, Abnormally. Not set yet
        private void Application_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Start another process to send feedback for user 
            Shutdown();
        }
    }
}
