﻿using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static MainWindow Instance { get; private set; }

        //public static string 

        public static void LoadMainWindow()
        {
            Instance = new MainWindow();
            Instance.InitializeComponent();
            Instance.Icon = GlobalVars.SoftwareIcon;
        }
    }
}
