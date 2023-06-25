using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using qrStudent.Pages.ScanStudent;
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

namespace qrStudent.Pages.GenerateExcel
{
    /// <summary>
    /// Interaction logic for GenerateExcelPage.xaml
    /// </summary>
    public partial class GenerateExcelPage : System.Windows.Controls.Page
    {
        public GenerateExcelPage()
        {
            InitializeComponent();
            InitializeTingkatan();
            InitializeKelas();
            InitializeMatapelajaran();
            GetExcel.IsEnabled = false;
        }
        private void InitializeTingkatan()
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectTingkatan.Items.Add("Sila Pilih");
                var sql = "SELECT value FROM parameter where kategori='Tingkatan'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectTingkatan.Items.Add("Tingkatan " + dat[i]);
                    }

                }
                selectTingkatan.SelectedIndex = 0;



            }
        }
        private void InitializeMatapelajaran()
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectMatapelajaran.Items.Add("Sila Pilih");
                var sql = "SELECT value FROM parameter where kategori='Matapelajaran'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectMatapelajaran.Items.Add(dat[i]);
                    }

                }
                selectMatapelajaran.SelectedIndex = 0;



            }
        }
        private void InitializeKelas()
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectKelas.Items.Add("Sila Pilih");
                var sql = "SELECT value FROM parameter where kategori='Kelas'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectKelas.Items.Add(dat[i]);
                    }

                }
                selectKelas.SelectedIndex = 0;



            }
        }

        private void selectTingkatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkSelect();
        }

        private void selectKelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkSelect();
        }

        private void selectMatapelajaran_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkSelect();
        }
        private void checkSelect()
        {
            if (selectKelas.SelectedIndex != 0 && selectTingkatan.SelectedIndex != 0 && selectMatapelajaran.SelectedIndex != 0)
            {

                GetExcel.IsEnabled = true;

            }
            else
            {
                GetExcel.IsEnabled = false;
            }
        }

        private void GetExcel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {
                var subjek = selectMatapelajaran.SelectedItem.ToString();
                var tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1];
                var kelas = selectKelas.SelectedItem.ToString();

                var getStudentClass = new List<getStudentDatModel>();
                using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                {
                    getStudentClass = conn.Query<getStudentDatModel>("select Id,Nama from SenaraiPelajar where Tingkatan=@tingkatan and Kelas=@kelas order by Id", new { tingkatan, kelas }).ToList();


                    if (getStudentClass.Count < 1)
                    {
                        MessageBox.Show("Tiada rekord pelajar!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var getData = new List<getKandunganDataModel>();

                    getData = conn.Query<getKandunganDataModel>("select * from KandunganData where Tingkatan=@tingkatan and Matapelajaran=@subjek order by Tema,Bidang,Kandungan,StandardPembelajaran", new { tingkatan, subjek }).ToList();

                    if (getData.Count < 1)
                    {
                        MessageBox.Show("Tiada data subjek!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }



                    using var wbook = new XLWorkbook();

                    var sheet = wbook.Worksheets.Add("Rekod Transit");
                    sheet.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    sheet.Cell(1, 1).Value = "REKOD TRANSIT PBD TINGKATAN " + tingkatan;
                    
                    sheet.Range(sheet.Cell(1, 1), sheet.Cell(1, 3)).Merge();
                    sheet.Cell(3, 1).Value = "NO";
                    sheet.Cell(3, 1).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
                    sheet.Range(sheet.Cell(3, 1), sheet.Cell(7, 1)).Merge();
                    sheet.Cell(3, 2).Value = "TEMA:";
                    sheet.Cell(3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    sheet.Cell(4, 2).Value = "BIDANG PEMBELAJARAN:";
                    sheet.Cell(4, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    sheet.Cell(5, 2).Value = "STANDARD KANDUNGAN:";
                    sheet.Cell(5, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    sheet.Cell(6, 2).Value = "STANDARD PEMBELAJARAN:";
                    sheet.Cell(6, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    sheet.Cell(7, 2).Value = "TARIKH:";
                    sheet.Cell(7, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                    sheet.Cell(8, 2).Value = "NAMA";
                    sheet.Cell(8, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);


                    for (int i = 0; i < getStudentClass.Count; i++)
                    {
                        getStudentDatModel? item = getStudentClass[i];
                        var no = i + 1;

                        sheet.Cell(i + 9, 1).Value = no;
                        sheet.Cell(i + 9, 2).Value = item.Nama;

                    }

                    var temaIndex = 1;
                    var temaAfteNo = 2;
                    var temaStartPoint = 3;

                    var bidangIndex = 1;
                    var bidangAfteNo = 2;
                    var bidangStartPoint = 3;

                    var kandunganIndex = 1;
                    var kandunganAfteNo = 2;
                    var kandunganStartPoint = 3;


                    var standardAfteNo = 3;
                    

                    for (int i = 0; i < getData.Count; i++)
                    {
                        getKandunganDataModel? item = getData[i];

                        if (kandunganIndex != item.Kandungan || bidangIndex != item.Bidang)
                        {
                            sheet.Range(sheet.Cell(5, kandunganStartPoint), sheet.Cell(5, kandunganAfteNo)).Merge();
                            var getKandunganFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganStandard WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and Tema=@temaIndex and Bidang=@bidangIndex and [Index]=@kandunganIndex", new { tingkatan, subjek, temaIndex, bidangIndex, kandunganIndex });
                            sheet.Cell(5, kandunganStartPoint).Value = $"{kandunganIndex} {getKandunganFull}";
                            kandunganIndex = item.Kandungan;


                            kandunganStartPoint = kandunganAfteNo + 1;
                        }

                        if (bidangIndex != item.Bidang || temaIndex != item.Tema)
                        {
                            sheet.Range(sheet.Cell(4, bidangStartPoint), sheet.Cell(4, bidangAfteNo)).Merge();
                            var getBidangFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganBidang WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and Tema=@temaIndex and [Index]=@bidangIndex", new { tingkatan, subjek, temaIndex, bidangIndex });
                            sheet.Cell(4, bidangStartPoint).Value = $"{bidangIndex} {getBidangFull}";
                            bidangIndex = item.Bidang;


                            bidangStartPoint = bidangAfteNo + 1;
                        }

                        if (temaIndex != item.Tema)
                        {
                            sheet.Range(sheet.Cell(3, temaStartPoint), sheet.Cell(3, temaAfteNo)).Merge();
                            var getTemaFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganTema WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and [Index]=@temaIndex", new { tingkatan,subjek, temaIndex });
                            sheet.Cell(3, temaStartPoint).Value = $"{temaIndex} {getTemaFull}";
                            temaIndex = item.Tema;

                            temaStartPoint = temaAfteNo + 1;
                        }

                        // sheet.Range(sheet.Cell(6, standardStartPoint), sheet.Cell(6, standardAfteNo)).Merge();


                        sheet.Cell(6, standardAfteNo).Value = $"{bidangIndex}.{kandunganIndex}.{item.StandardPembelajaran}";
                        var namatajuk = $"{subjek}${tingkatan}${item.Tema}${item.Bidang}${item.Kandungan}";
                        if (item.StandardPembelajaran!=0)
                        {
                            namatajuk += "$" + item.StandardPembelajaran;
                        }
                        var chkExistColumn = conn.QuerySingleOrDefault<int>("SELECT count(name) from PRAGMA_table_info('PelajarToKandungan') WHERE name=@name", new { name = namatajuk });
                        if (chkExistColumn != 0)
                        {
                            var line = 9;
                            foreach (var studMark in getStudentClass)
                            {
                                var mark=conn.QuerySingleOrDefault<int>($"SELECT {namatajuk} from PelajarToKandungan WHERE Id=@id and {namatajuk}='1' ", new { id = studMark.Id });
                                if (mark==1)
                                {
                                    sheet.Cell(line, standardAfteNo).Value = mark;
                                }
                                line++;
                            }
                        }




                        standardAfteNo++;
                        temaAfteNo++;
                        bidangAfteNo++;
                        kandunganAfteNo++;



                        if (getData.Count == i + 1)
                        {
                            sheet.Range(sheet.Cell(3, temaStartPoint), sheet.Cell(3, temaAfteNo)).Merge();
                            var getTemaFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganTema WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and [Index]=@temaIndex", new { tingkatan, subjek, temaIndex });
                            sheet.Cell(3, temaStartPoint).Value = temaIndex + " " + getTemaFull;

                            sheet.Range(sheet.Cell(4, bidangStartPoint), sheet.Cell(4, bidangAfteNo)).Merge();
                            var getBidangFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganBidang WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and Tema=@temaIndex and [Index]=@bidangIndex", new { tingkatan, subjek, temaIndex, bidangIndex });
                            sheet.Cell(4, bidangStartPoint).Value = bidangIndex + " " + getBidangFull;

                            sheet.Range(sheet.Cell(5, kandunganStartPoint), sheet.Cell(5, kandunganAfteNo)).Merge();
                            var getKandunganFull = conn.QuerySingleOrDefault<string>("SELECT [Desc] from KandunganStandard WHERE Tingkatan=@tingkatan and Matapelajaran=@subjek and Tema=@temaIndex and Bidang=@bidangIndex and [Index]=@kandunganIndex", new { tingkatan, subjek, temaIndex, bidangIndex, kandunganIndex });
                            sheet.Cell(5, kandunganStartPoint).Value =  kandunganIndex + " " + getKandunganFull;


                            // sheet.Range(sheet.Cell(6, standardStartPoint), sheet.Cell(6, standardAfteNo)).Merge();
                            //sheet.Cell(6, standardStartPoint).Value = standardIndex;

                            sheet.Range(sheet.Cell(8, 3), sheet.Cell(8, standardAfteNo)).Merge();
                            sheet.Cell(8, 3).Value = "TAHAP PENGUASAAN (TP)";
                        }




                    }



                    var filename = $"REKOD TRANSIT PBD {subjek} TINGKATAN {tingkatan} {kelas}.xlsx";
                    wbook.SaveAs(dlg.ResultPath + "/"+ filename);
                    MessageBox.Show($"Template berjaya dibuat di:{Environment.NewLine}{dlg.ResultPath}\\{filename}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
