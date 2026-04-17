using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Reflection;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.ViewModels;
using System;

namespace THUVIENZ.Views
{
    public partial class Login : Window
    {
        private readonly AuthService _authService;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const uint EM_SETSEL = 0x00B1;

        public Login()
        {
            InitializeComponent();
            _authService = new AuthService();
            this.DataContext = new LoginViewModel();
        }

        private void LnkRegister_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.Show();
            this.Close();
        }

        private void LnkForgot_Click(object sender, RoutedEventArgs e)
        {
            ForgotPassword forgotWindow = new ForgotPassword();
            forgotWindow.Show();
            this.Close();
        }
    }
}