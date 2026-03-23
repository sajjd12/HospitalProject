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
using System.Windows.Shapes;

namespace Hospital.Desktop.Views
{
    /// <summary>
    /// Interaction logic for UserFormView.xaml
    /// </summary>
    // داخل UserFormView.xaml.cs
    public partial class UserFormView : Window
    {
        public UserFormView()
        {
            InitializeComponent();
        }

        // يمكنك استدعاء هذه الدالة من الـ ViewModel عبر Interface أو تبسيطها هنا مؤقتاً
        public void CloseWindow()
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
