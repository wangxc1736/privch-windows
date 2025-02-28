﻿using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using Privch.View;
using Privch.ViewModel;
using Privch.ViewModel.Element;

namespace Privch.Control
{
    internal static class InterfaceCtrl
    {
        public static View.Forms.TrayNotify NotifyIcon;

        public static void Initialize()
        {
            NotifyIcon = new View.Forms.TrayNotify();
            ModifyTheme(theme => theme.SetBaseTheme(
                Model.SettingManager.Appearance.IsDarkTheme ? Theme.Dark : Theme.Light));
        }

        public static void Dispose()
        {
            NotifyIcon.Dispose();
        }

        public static void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }

        public static void ShowSetting()
        {
            DialogSetting dialogSetting = Application.Current.Windows.OfType<DialogSetting>().FirstOrDefault();
            if (dialogSetting == null)
            {
                dialogSetting = new DialogSetting();
                dialogSetting.ShowDialog();
            }
            else
            {
                dialogSetting.Activate();
            }
        }

        public static void ShowHomeNotify(string message)
        {
            if (Application.Current.MainWindow is WindowHome windowHome)
            {
                windowHome.SendSnakebarMessage(message);
            }
        }

        public static void UpdateTransmitLock()
        {
            // WindowHome is null on shutdown. NotifyIcon updates status at menu popup
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.UpdateTransmitLock();
            }

            NotifyIcon.UpdateTransmitLock();
        }

        public static void UpdateHomeTransmitStatue()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.UpdateTransmitStatus();
            }
        }

        public static void AddHomeTask(TaskView task)
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddTask(task);
            }
        }

        public static void RemoveHomeTask(TaskView task)
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.RemoveTask(task);
            }
        }

        public static void AddServerByScanQRCode()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddServerByScanQRCode();
            }
        }

        public static void AddServerFromClipboard()
        {
            if (Application.Current.MainWindow is WindowHome windowHome
                && windowHome.DataContext is HomeVModel homeViewModel)
            {
                homeViewModel.AddServerFromClipboard();
            }
        }
    }
}
