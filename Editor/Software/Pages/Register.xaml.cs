﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
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
using System.Windows.Media.Animation;
using FirstFloor.ModernUI.Windows.Controls;
using System.Timers;
using System.Text.RegularExpressions;

namespace Software.Pages
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        private Timer timerAnim;
        public Register()
        {
            InitializeComponent();

            loadingRegister.IsActive = false;

            btnRegister.Click += btnRegister_Click;
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(userName);
        }

        void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            loadingRegister.IsActive = true;

            timerAnim = new Timer(1000);
            timerAnim.Elapsed += new ElapsedEventHandler(DoRegister);
            timerAnim.Enabled = true;
        }

        void DoRegister(object sender, ElapsedEventArgs e)
        {
            timerAnim.Enabled = false;
            this.Dispatcher.Invoke((Action)(() =>
            {
                // Check if all element are filled in
                for (int i = 0; i < FieldsGrid.Children.Count; i++)
                {
                    UIElement elt = FieldsGrid.Children[i];
                    if (elt is TextBox && ((TextBox)elt).Text.Length == 0 || elt is PasswordBox && ((PasswordBox)elt).Password.Length == 0)
                    {
                        ShowError("Le champ \"" + ((Label)FieldsGrid.Children[i - 1]).Content + "\" doit être complété.");
                        return;
                    }
                }
                if (userName.Text.Length < 4 || userName.Text.Length > 30)
                {
                    ShowError("Le nom d'utilisateur doit contenir de 4 à 30 caractères.");
                    return;
                }
                else if (passwordBox.Password != passwordBox2.Password)
                {
                    ShowError("Les deux mots de passe ne sont pas équivalents.");
                    return;
                }
                else if (emailAddress.Text.Length < 3 || !IsValidEmail(emailAddress.Text))
                {
                    ShowError("L'adresse E-Mail entrée est incorrecte.");
                    return;
                }


            }));

        }

        private void ShowError(string err)
        {
            loadingRegister.IsActive = false;

            DoubleAnimation opacityAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            ErrMsg.BeginAnimation(OpacityProperty, opacityAnim);

            ErrMsg.Text = err;
        }

        public static bool IsValidEmail(string mailAddress)
        {
            Regex mailIDPattern = new Regex(@"[\w-]+@([\w-]+\.)+[\w-]+");
            return !string.IsNullOrEmpty(mailAddress) && mailIDPattern.IsMatch(mailAddress);
        }
    }
}
