using System.Windows;

namespace MyToolBar.Plugin.TabletUtils.Sidebar;

/// <summary>
/// 日期选择对话框
/// </summary>
public partial class DatePickerDialog : Window
{
    public DateTime SelectedDate { get; private set; }

    public DatePickerDialog(DateTime initialDate)
    {
        InitializeComponent();
        DateCal.SelectedDate = initialDate;
        SelectedDate = initialDate;
        DateCal.SelectedDatesChanged += (s, e) =>
        {
            if (DateCal.SelectedDate.HasValue)
                SelectedDate = DateCal.SelectedDate.Value;
        };
    }

    private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
