using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Register : Window
    {
        private readonly AuthService _authService;

        public Register()
        {
            InitializeComponent();
            _authService = new AuthService();
            this.DataContext = new RegisterViewModel();
        }

        private void GenderRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RegisterViewModel vm && sender is RadioButton rb)
            {
                vm.Gender = rb.Content?.ToString() ?? "Khác";
            }
        }

        private void BtnBackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            new Login().Show();
            this.Close();
        }

    }
}