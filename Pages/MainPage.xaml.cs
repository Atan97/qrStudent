using qrStudent.Pages.UpdateStudent;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace qrStudent.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            
        }
        private void UpdateStudentPage_Click(object sender, RoutedEventArgs e)
        {

            MainWindow wnd = (MainWindow)Application.Current.MainWindow;
            wnd.BackToMenu.Visibility = Visibility.Visible;
            this.NavigationService.Navigate(new Uri("Pages/UpdateStudent/UpdateStudentPage.xaml", UriKind.Relative));

           
            
        }

        private void GenerateQrPage_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = (MainWindow)Application.Current.MainWindow;
            wnd.BackToMenu.Visibility = Visibility.Visible;
            this.NavigationService.Navigate(new Uri("Pages/GenerateQr/GenerateQrPage.xaml", UriKind.Relative));
        }
    }
}
