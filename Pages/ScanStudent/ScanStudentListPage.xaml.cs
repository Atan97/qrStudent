using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace qrStudent.Pages.ScanStudent
{
    /// <summary>
    /// Interaction logic for ScanStudentListPage.xaml
    /// </summary>
    public partial class ScanStudentListPage : Page
    {
        public ScanStudentListPage(ScanStudentModel e)
        {
            InitializeComponent();
            studentList(e);

        }
       
        private void studentList(ScanStudentModel data)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {


                var sql = "SELECT Id, Nama,Tingkatan,Kelas FROM SenaraiPelajar where Tingkatan=@tingkatan and Kelas=@kelas COLLATE NOCASE";
                StudentListGrid.ItemsSource = conn.Query<ScanStudentModel>(sql, new { tingkatan = data.Tingkatan, kelas = data.Kelas }).ToList();
                StudentListGrid.CanUserAddRows = false;
            }
               
        }

        private void BackToSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Pages/ScanStudent/ScanStudentSelect.xaml", UriKind.Relative));
        }
    }

    
}
