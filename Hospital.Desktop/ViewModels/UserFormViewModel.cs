using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class UserFormViewModel :BaseViewModel
    {
        private readonly ApiService _apiService;

        public UserFormDTO User { get; set; }
        public ObservableCollection<string> Roles { get; set; } = new() { "Admin", "User", "Manager" };
        public ObservableCollection<dynamic> Employees { get; set; } = new();

        // حالات الشاشة
        public bool IsAddMode { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }

        // خاصية ربط الموظف
        private bool _linkEmployee;
        public bool LinkEmployee
        {
            get => _linkEmployee;
            set
            {
                _linkEmployee = value;
                if (!value) User.EmployeeId = null; // تصغير المعرف إذا ألغي الربط
                OnPropertyChanged(nameof(LinkEmployee));
            }
        }

        public ICommand SaveCommand { get; }

        public UserFormViewModel(UserFormDTO user = null, string mode = "Add")
        {
            _apiService = new ApiService();
            IsAddMode = mode == "Add";
            IsEditMode = mode == "Edit";
            IsViewMode = mode == "View";

            User = user ?? new UserFormDTO();

            // إذا كان المستخدم يمتلك EmployeeId، نفعل خيار الربط تلقائياً
            LinkEmployee = User.EmployeeId.HasValue;

            LoadEmployees();

            SaveCommand = new RelayCommand(async (p) => await SaveUser(), (p) => !IsViewMode);
        }

        private async void LoadEmployees()
        {
            // جلب قائمة الموظفين لملء الكومبوبوكس
            var result = await _apiService.GetAsync<List<dynamic>>("Employees");
            if (result != null)
                foreach (var emp in result) Employees.Add(emp);
        }

        private async Task SaveUser()
        {
            try
            {
                if (IsAddMode)
                    await _apiService.PostAsync<dynamic>("Auth/Register", User);
                else
                    await _apiService.PutAsync<dynamic>("Auth/UpdateUser", User);

                MessageBox.Show("تمت العملية بنجاح");
                // هنا نغلق النافذة (سنحتاج لإضافة آلية غلق)
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}