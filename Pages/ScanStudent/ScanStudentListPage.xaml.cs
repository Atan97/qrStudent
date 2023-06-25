using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
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
            Tingkatan = e.kodKelas.Split('$')[1];
            createNamaSubjekFull();



        }

        private void studentList(ScanStudentModel data)
        {
            StudentListGrid.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "SELECT a.Id, a.Nama,b." + data.kodKelas + " Siap FROM SenaraiPelajar a LEFT JOIN PelajarToKandungan b on a.Id=b.IdPelajar where a.Tingkatan=@tingkatan and a.Kelas=@kelas COLLATE NOCASE";
                List<DisplayStudentModel> studList = new();
                var dat = conn.Query<ScanStudentModel>(sql, new { tingkatan = data.kodKelas.Split('$')[1], kelas = data.Kelas }).ToList();
                for (int i = 0; i < dat.Count; i++)
                {
                    ScanStudentModel? row = dat[i];
                    DisplayStudentModel stud = new DisplayStudentModel { Nama = row.Nama, No = i + 1, Siap = row.Siap, Id = row.Id };
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
            int stat = 0;
            if (checkedData)
            {
                stat = 1;
            }
            PelajarToKandunganData(stat, data.Id);
            e.Handled = true;
        }
        private void BackToSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("Pages/ScanStudent/ScanStudentSelect.xaml", UriKind.Relative));
        }
        private void PelajarToKandunganData(int stat, int IdPelajar)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "update PelajarToKandungan set " + kodKelas + " = @stat where IdPelajar=@IdPelajar";
                List<DisplayStudentModel> studList = new();
                conn.Execute(sql, new { stat, IdPelajar });

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
                    if ((data.Nama + "," + Tingkatan + "," + NamaKelas).ToLower() == scanText.Text.ToLower())
                    {
                        if (data.Siap == false)
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
        private void createNamaSubjekFull()
        {
            var kodKelasSplit = kodKelas.Split('$');
            var subjek = kodKelasSplit[0].Replace("_", " ");
            var tingkatan = kodKelasSplit[1];
            var tema = kodKelasSplit[2];
            var bidang = kodKelasSplit[3];
            var kandungan = kodKelasSplit[4];
            var sp = "";
            if (kodKelasSplit.Length == 6)
            {
                sp = kodKelasSplit[5];
            }
            getTajukFull(subjek, tingkatan, tema, bidang, kandungan, out string? temaFull, out string? bidangFull, out string? kandunganFull);

            tajukDat.Content = "Tingkatan " + tingkatan + " Kelas " + NamaKelas + Environment.NewLine
                 + "Subjek: " + subjek + Environment.NewLine
                 + "Tema: " + tema + ") " + temaFull + Environment.NewLine
                 + "Bidang: " + bidang + ") " + bidangFull + Environment.NewLine
                 + "Kandungan: " + kandungan + ") " + kandunganFull + Environment.NewLine
                 + "Standard Pembelajaran: " + sp;
        }

        private static void getTajukFull(string subjek, string tingkatan, string tema, string bidang, string kandungan, out string temaFull, out string bidangFull, out string kandunganFull)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {


                temaFull = conn.QuerySingleOrDefault<string>("""
                    select [Desc] from KandunganTema where [Index]=@index and Matapelajaran=@subjek and Tingkatan=@tingkatan
                    """, new { index = tema, subjek, tingkatan });
                bidangFull = conn.QuerySingleOrDefault<string>("""
                    select [Desc] from KandunganBidang where [Index]=@index and Matapelajaran=@subjek and Tingkatan=@tingkatan and Tema=@tema
                    """, new { index = bidang, subjek, tingkatan, tema });
                kandunganFull = conn.QuerySingleOrDefault<string>("""
                    select [Desc] from KandunganStandard where [Index]=@index and Matapelajaran=@subjek and Tingkatan=@tingkatan and Tema=@tema and Bidang=@bidang
                    """, new { index = kandungan, subjek, tingkatan, tema, bidang });



            }
        }

        private void DownloadExcel_Click(object sender, RoutedEventArgs e)
        {


            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {
                var kodKelasSplit = kodKelas.Split('$');
                var subjek = kodKelasSplit[0].Replace("_", " ");
                var tingkatan = kodKelasSplit[1];
                var tema = kodKelasSplit[2];
                var bidang = kodKelasSplit[3];
                var kandungan = kodKelasSplit[4];
                var sp = "";
                if (kodKelasSplit.Length == 6)
                {
                    sp = kodKelasSplit[5];
                }
                getTajukFull(subjek, tingkatan, tema, bidang, kandungan, out string? temaFull, out string? bidangFull, out string? kandunganFull);

                using var wbook = new XLWorkbook();

                var sheet = wbook.Worksheets.Add("Rekod Transit");
                
                sheet.Cell("A1").Value = "KELAS: "+ NamaKelas;
                sheet.Cell("B1").Value = "TINGKATAN: "+ tingkatan;
                sheet.Cell("C1").Value = "MATAPELAJARAN: "+ subjek;

                sheet.Cell("B3").Value = "TEMA";
                sheet.Cell("C3").Value = tema + ") " + temaFull;
                sheet.Cell("B4").Value = "BIDANG";
                sheet.Cell("C4").Value = bidang + ") " + bidangFull;
                sheet.Cell("B5").Value = "KANDUNGAN";
                sheet.Cell("C5").Value = kandungan + ") " + kandunganFull;
                if (!string.IsNullOrWhiteSpace(sp))
                {
                    sheet.Cell("B6").Value = "STANDARD PEMBELAJARAN";
                    sheet.Cell("C6").Value = kandungan + "." + sp;
                }
                sheet.Cell("B7").Value = "Tarikh";
                sheet.Cell("C7").Value = DateTime.Now.ToString("dd/MM/yyyy"); 
                
                sheet.Cell("A9").Value = "Bil";
                sheet.Cell("B9").Value = "Nama Murid";
               var datC= (List<DisplayStudentModel>)StudentListGrid.ItemsSource;
                for (int i = 0; i < datC.Count; i++)
                {
                    sheet.Cell("A"+(i+10)).Value = i+1;
                    sheet.Cell("B" + (i + 10)).Value = datC[i].Nama;
                    if (datC[i].Siap)
                    {
                        sheet.Cell("C" + (i + 10)).Value = "1";
                    }
                }
                sheet.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);



                wbook.SaveAs(dlg.ResultPath + "/"+ NamaKelas+"_" +tingkatan + "_" + subjek+DateTime.Now.ToString("yyyyMMdd")+ ".xlsx");
                MessageBox.Show("Template berjaya dibuat di:" + Environment.NewLine + dlg.ResultPath + "\\TemplateDataSubjek.xlsx", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }


}
