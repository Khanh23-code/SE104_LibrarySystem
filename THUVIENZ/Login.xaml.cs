using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace THUVIENZ
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Chuyển sang trang Profile của Reader khi đăng nhập thành công
            Profile profileWindow = new Profile();
            profileWindow.Show();
            this.Close();
        }
    }
}