using Dapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using qrStudent.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        static string kodKelas = "";
        static string NamaKelas = "";
        static string Tingkatan = "";
        public ScanStudentListPage(ScanStudentModel e)
        {
            InitializeComponent();
            studentList(e);
            kodKelas = e.kodKelas;
            NamaKelas = e.Kelas;
            Tingkatan = e.kodKelas.Split('_')[1];
            kelas.Content = kodKelas;



        }

        private void studentList(ScanStudentModel data)
        {
            StudentListGrid.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "SELECT a.Id, a.Nama,b."+data.kodKelas+ " Siap FROM SenaraiPelajar a LEFT JOIN PelajarToKandungan b on a.Id=b.IdPelajar where a.Tingkatan=@tingkatan and a.Kelas=@kelas COLLATE NOCASE";
                List<DisplayStudentModel> studList = new();
                var dat = conn.Query<ScanStudentModel>(sql, new { tingkatan = data.kodKelas.Split('_')[1], kelas = data.Kelas }).ToList();
                for (int i = 0; i < dat.Count; i++)
                {
                    ScanStudentModel? row = dat[i];
                    DisplayStudentModel stud = new DisplayStudentModel { Nama=row.Nama,No=i+1,Siap=row.Siap,Id=row.Id};
                    studList.Add(stud);
                    //StudentListGrid.Items.Add(stud);
                }
               StudentListGrid.ItemsSource= studList;
               
                StudentListGrid.CanUserAddRows = false;
                StudentListGrid.AutoGenerateColumns = false;
               // StudentListGrid.IsReadOnly = true;
            }

        }
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)e.OriginalSource;
            // DataGridRow dataGridRow = VisualTreeHelpers.FindAncestor<DataGridRow>(checkBox);
            var data = (DisplayStudentModel)checkBox.DataContext;
            var checkedData = checkBox.IsChecked??false;

            //if (checkBox.IsChecked && String.IsNullOrEmpty(produit.Id.ToString()))
            //{
            //    // Show message box here...
            //}
            int stat = 0;
            if (checkedData)
            {
                stat = 1;
            }
            PelajarToKandunganData( stat, data.Id);
            e.Handled = true;
        }
        private void BackToSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Pages/ScanStudent/ScanStudentSelect.xaml", UriKind.Relative));
        }
        private void PelajarToKandunganData(int stat,int IdPelajar)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "update PelajarToKandungan set "+ kodKelas + " = @stat where IdPelajar=@IdPelajar";
                List<DisplayStudentModel> studList = new();
                conn.Execute(sql, new { stat = stat, IdPelajar = IdPelajar });
                
            }
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var alldat = (List<DisplayStudentModel>)StudentListGrid.ItemsSource!;
                StudentListGrid.ItemsSource = null;
                foreach (var data in alldat)
                {
                    if ((data.Nama +","+ Tingkatan + "," + NamaKelas).ToLower() ==  scanText.Text.ToLower())
                    {
                        if (data.Siap==false)
                        {
                            PelajarToKandunganData(1, data.Id);
                            data.Siap = true;
                        }
                       
                       
                    }
                }
                StudentListGrid.ItemsSource = alldat;
                scanText.Text = "";
            }
        }

    }


}
