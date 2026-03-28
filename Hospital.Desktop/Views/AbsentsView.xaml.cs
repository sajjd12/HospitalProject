using Hospital.Core.DTOs;
using Hospital.Desktop.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hospital.Desktop.Views
{
    public partial class AbsentsView : UserControl
    {
        public AbsentsView()
        {
            InitializeComponent();
        }

        // هذه هي الدالة المفقودة التي تسبب الخطأ
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row?.Item is AbsentFullDto absent)
            {
                // استدعاء الـ ViewModel لتنفيذ أمر عرض التفاصيل
                if (this.DataContext is AbsentsViewModel vm)
                {
                    vm.ViewDetailsCommand.Execute(absent);
                }
            }
        }
    }
}