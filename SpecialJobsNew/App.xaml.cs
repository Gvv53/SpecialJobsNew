using System.Windows;

namespace SpecialJobs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Выдано не обработанное исключение " +
                (e.Exception.InnerException!= null ? e.Exception.InnerException.Message + e.Exception.InnerException.StackTrace : 
                e.Exception.Message) +
                e.Exception.StackTrace,
                "", MessageBoxButton.OK);
            e.Handled = false;
        }
    }
}
