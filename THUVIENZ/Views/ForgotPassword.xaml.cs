using System;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class ForgotPassword : Window
    {
        public ForgotPassword()
        {
            InitializeComponent();
            this.DataContext = new ForgotPasswordViewModel();
        }

        private void BtnBackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            new Login().Show();
            this.Close();
        }
    }
}