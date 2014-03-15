using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Software
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Set the current user interface culture to the specific culture
            System.Threading.Thread.CurrentThread.CurrentUICulture =
                        new System.Globalization.CultureInfo("en-US");
        }
    }
}
