using ClosedXML.Excel;
using Dapper;
using qrStudent.Models;
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
    /// Interaction logic for ScanStudentTempListPage.xaml
    /// </summary>
    public partial class ScanStudentTempListPage : Page
    {
        static string NamaKelas = "";
        static string Tingkatan = "";
        static string Subjek = "";
        static string Tajuk = "";
        public ScanStudentTempListPage(StudentModelTemp temp)
        {
            InitializeComponent();
            NamaKelas = temp.Kelas;
            Tingkatan=temp.Tingkatan;
            Subjek=temp.Subjek == "Sila Pilih" ? "" : temp.Subjek; ;
            Tajuk = temp.Tajuk;
            studentList(temp);
            createNamaSubjekFull();
        }
        private void studentList(StudentModelTemp data)
        {
            StudentListGrid.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "SELECT a.Id, a.Nama FROM SenaraiPelajar a where a.Tingkatan=@tingkatan and a.Kelas=@kelas COLLATE NOCASE";
                List<DisplayStudentModel> studList = new();
                var dat = conn.Query<ScanStudentModel>(sql, new { tingkatan = data.Tingkatan, kelas = data.Kelas }).ToList();
                for (int i = 0; i < dat.Count; i++)
                {
                    ScanStudentModel? row = dat[i];
                    DisplayStudentModel stud = new DisplayStudentModel { Nama = row.Nama, No = i + 1, Siap = false, Id = row.Id };
                    studList.Add(stud);
                    //StudentListGrid.Items.Add(stud);
                }
                StudentListGrid.ItemsSource = studList;

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
            var checkedData = checkBox.IsChecked ?? false;

            //if (checkBox.IsChecked && String.IsNullOrEmpty(produit.Id.ToString()))
            //{
            //    // Show message box here...
            //}
            //int stat = 0;
            //if (checkedData)
            //{
            //    stat = 1;
            //}
            //PelajarToKandunganData(stat, data.Id);
            e.Handled = true;
        }
        private void BackToSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Pages/ScanStudent/ScanStudentSelect.xaml", UriKind.Relative));
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var alldat = (List<DisplayStudentModel>)StudentListGrid.ItemsSource!;
                StudentListGrid.ItemsSource = null;
                foreach (var data in alldat)
                {
                    if ((data.Nama + "," + Tingkatan + "," + NamaKelas).ToLower() == scanText.Text.ToLower())
                    {
                        if (data.Siap == false)
                        {
                           // PelajarToKandunganData(1, data.Id);
                            data.Siap = true;
                        }


                    }
                }
                StudentListGrid.ItemsSource = alldat;
                scanText.Text = "";
            }
        }
        private void createNamaSubjekFull()
        {
            var Su = "";
            if (Subjek.Trim()!="")
            {
                Su = $"Subjek: {Subjek}{Environment.NewLine}";
            }

            tajukDat.Content = $"Tingkatan {Tingkatan} Kelas {NamaKelas}{Environment.NewLine}{Su}Tajuk: {Tajuk}";
        }

        private void DownloadExcel_Click(object sender, RoutedEventArgs e)
        {


            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {
                
               
                using var wbook = new XLWorkbook();

                var sheet = wbook.Worksheets.Add("Rekod");
                sheet.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
               // sheet.Cell(1, 1).Value = "REKOD SEMENTARA TINGKATAN " + Tingkatan;

                sheet.Range(sheet.Cell(1, 1), sheet.Cell(1, 3)).Merge();
                sheet.Cell(3, 1).Value = "NO";
                sheet.Cell(3, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                sheet.Range(sheet.Cell(3, 1), sheet.Cell(7, 1)).Merge();
                sheet.Cell(3, 2).Value = "TINGKATAN:";
                sheet.Cell(3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(4, 2).Value = "KELAS:";
                sheet.Cell(4, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(5, 2).Value = "SUBJEK:";
                sheet.Cell(5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(6, 2).Value = "TAJUK:";
                sheet.Cell(6, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(7, 2).Value = "TARIKH:";
                sheet.Cell(7, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(8, 2).Value = "NAMA";
                sheet.Cell(8, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(3, 3).Value = Tingkatan;
                sheet.Cell(3, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                sheet.Cell(4, 3).Value = NamaKelas;
                sheet.Cell(4, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                sheet.Cell(5, 3).Value = Subjek;
                sheet.Cell(5, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                sheet.Cell(6, 3).Value = Tajuk;
                sheet.Cell(6, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);


                sheet.Cell(7, 3).Value = DateTime.Now.ToString("dd/MM/yyyy");
                sheet.Cell(7, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);


                var datC = (List<DisplayStudentModel>)StudentListGrid.ItemsSource;
                for (int i = 0; i < datC.Count; i++)
                {

                    sheet.Cell(i + 9, 1).Value = i + 1;
                    sheet.Cell(i + 9, 2).Value = datC[i].Nama;
                    if (datC[i].Siap)
                    {
                        sheet.Cell(i + 9, 3).Value = "1";
                    }
                }



                var filename = $"REKOD {Tajuk} TINGKATAN {Tingkatan} {NamaKelas}.xlsx";
                wbook.SaveAs($"{dlg.ResultPath}/{filename}");
                MessageBox.Show($"Rekod berjaya disimpan di:{Environment.NewLine}{dlg.ResultPath}\\{filename}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
