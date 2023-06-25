using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Win32;
using qrStudent.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace qrStudent.Pages.UpdateStudent
{
    /// <summary>
    /// Interaction logic for UpdateStudentPage.xaml
    /// </summary>
    public partial class UpdateStudentPage : System.Windows.Controls.Page
    {


        public UpdateStudentPage()
        {

            InitializeComponent();
            InitializeTingkatan();
            InitializeKelas();
            InitializeMatapelajaran();

            kSubjek.Visibility = Visibility.Collapsed;
            kSubjekData.Visibility = Visibility.Collapsed;

        }

        private void UploadStudent_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Upload Data baru bagi " + selectTingkatan.SelectedItem.ToString() + " Kelas " + selectKelas.SelectedItem.ToString() + "?" + Environment.NewLine +
                "*Record lama pelajar bagi kelas ini akan akan dihapuskan!", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // Close the window  

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xlsx;";
                List<StudentModel> students = new List<StudentModel>();
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        var workbook = new XLWorkbook(openFileDialog.FileName);
                        var Worksheet = workbook.Worksheet(1);
                        var rows = Worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row
                        foreach (var row in rows)
                        {

                            StudentModel studentModel = new()
                            {
                                Nama = row.Cell(1).Value.ToString(),
                                NoPendaftaran = row.Cell(2).Value.ToString(),
                                Tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1],
                                Kelas = selectKelas.SelectedItem.ToString()!
                            };
                            students.Add(studentModel);

                        }
                        using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                        {
                            var oldList = conn.Query<long>("select Id FROM SenaraiPelajar where  Tingkatan=@tingkatan and Kelas=@kelas COLLATE NOCASE", new { tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1], kelas = selectKelas.SelectedItem.ToString()! }).ToList();
                            if (oldList.Count > 0)
                            {
                                conn.Execute("delete from SenaraiPelajar where  Tingkatan=@tingkatan and Kelas=@kelas COLLATE NOCASE", new { tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1], kelas = selectKelas.SelectedItem.ToString()! });
                                for (int i = 0; i < oldList.Count; i++)
                                {
                                    conn.Execute("delete from PelajarToKandungan where IdPelajar=@IdPelajar", new { IdPelajar = oldList[i] });
                                }
                            }

                            var sql = "";
                            foreach (var row in students)
                            {
                                sql = "";
                                sql += "INSERT INTO SenaraiPelajar (Nama,NoPendaftaran,Tingkatan,Kelas) VALUES('" + row.Nama + "','" + row.NoPendaftaran + "','" + row.Tingkatan + "','" + row.Kelas + "'); SELECT last_insert_rowid()";
                                var dat = conn.QuerySingle<string>(sql);
                                conn.Execute("insert into PelajarToKandungan (IdPelajar) values(" + dat + ")");

                            }
                        }
                        MessageBox.Show("Upload berjaya", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Upload tidak berjaya, sila cuba lagi", "Upload error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }

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
        private void checkSelect()
        {
            if (selectTingkatan.SelectedIndex == 0 || selectKelas.SelectedIndex == 0)
            {
                UploadStudent.IsEnabled = false;
                return;
            }
            UploadStudent.IsEnabled = true;
            return;
        }

        private void InitializeTingkatan()
        {
            selectTingkatan.Items.Clear();
            selectTingkatan1.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectTingkatan.Items.Add("Sila Pilih");
                selectTingkatan1.Items.Add("Sila Pilih");

                var sql = "SELECT value FROM parameter where kategori='Tingkatan'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectTingkatan.Items.Add("Tingkatan " + dat[i]);
                        selectTingkatan1.Items.Add("Tingkatan " + dat[i]);


                    }

                }
                selectTingkatan.SelectedIndex = 0;
                selectTingkatan1.SelectedIndex = 0;




            }
        }
        private void InitializeKelas()
        {
            selectKelas.Items.Clear();
            selectKelas1.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectKelas.Items.Add("Sila Pilih");
                selectKelas1.Items.Add("Sila Pilih");
                var sql = "SELECT value FROM parameter where kategori='Kelas'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectKelas.Items.Add(dat[i]);
                        selectKelas1.Items.Add(dat[i]);
                    }

                }
                selectKelas.SelectedIndex = 0;
                selectKelas1.SelectedIndex = 0;



            }
        }

        private void TemplateStudent_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {

                using var wbook = new XLWorkbook();

                var ws = wbook.Worksheets.Add("Sheet1");
                ws.Cell("A1").Value = "Nama";
                ws.Cell("B1").Value = "No Pendaftaran";

                wbook.SaveAs(dlg.ResultPath + "/TemplateStudent.xlsx");
                MessageBox.Show("Template berjaya dibuat di:" + Environment.NewLine + dlg.ResultPath + "\\TemplateStudent.xlsx", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }

        private void AddClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (string.IsNullOrWhiteSpace(KelasBaru.Text))
                {
                    MessageBox.Show("Nama kelas tidak boleh kosong!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                {
                    var checkAvailable = conn.QuerySingleOrDefault<int>("select count (id) from Parameter where Kategori='Kelas' and Value=@namaKelas", new { namaKelas= KelasBaru.Text.Trim().ToUpper() });
                    if (checkAvailable > 0)
                    {
                        MessageBox.Show($"Nama kelas {KelasBaru.Text.Trim().ToUpper()} sudah ada dalam rekord!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    conn.Execute("insert into Parameter (Kategori,Value) values('Kelas',@namaKelas)", new { namaKelas = KelasBaru.Text.Trim().ToUpper() });
                    InitializeKelas();

                }
                MessageBox.Show("Kelas " + KelasBaru.Text.Trim().ToUpper() + " berjaya ditambah!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RemoveClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectKelas1.SelectedIndex == 0)
                {
                    MessageBox.Show("Pilih kelas untuk dibuang", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var buangkls = selectKelas1.SelectedItem.ToString()!;
                if (MessageBox.Show("Buang kelas " + buangkls + "?" + Environment.NewLine +
               "*Rekod pelajar bagi kelas ini akan akan dihapuskan!", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                    {

                        conn.Execute("delete FROM Parameter WHERE Kategori='Kelas' and Value=@namaKelas", new { namaKelas = buangkls });
                        var oldList = conn.Query<long>("select Id FROM SenaraiPelajar where  Kelas=@kelas COLLATE NOCASE", new { kelas = buangkls }).ToList();
                        if (oldList.Count > 0)
                        {
                            conn.Execute("delete from SenaraiPelajar where Kelas=@kelas COLLATE NOCASE", new { kelas = buangkls });
                            for (int i = 0; i < oldList.Count; i++)
                            {
                                conn.Execute("delete from PelajarToKandungan where IdPelajar=@IdPelajar", new { IdPelajar = oldList[i] });
                            }
                        }

                        InitializeKelas();

                    }
                    MessageBox.Show("Kelas " + buangkls + " berjaya dibuang!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void AddSubjek_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SubjekBaru.Text))
                {
                    MessageBox.Show("Nama subjek tidak boleh kosong!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var namaSubjek = Regex.Replace(SubjekBaru.Text.Trim().ToUpper(), @"\s+", "_");
                using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                {

                    var checkAvailable = conn.QuerySingleOrDefault<int>("select count (id) from Parameter where Kategori='Matapelajaran' and Value=@namaSubjek", new { namaSubjek });
                    if (checkAvailable > 0)
                    {
                        MessageBox.Show($"Nama subjek {namaSubjek} sudah ada dalam rekord!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    conn.Execute("insert into Parameter (Kategori,Value) values('Matapelajaran',@namaSubjek)", new { namaSubjek });
                    InitializeMatapelajaran();



                }
                MessageBox.Show($"Subjek {namaSubjek} berjaya ditambah!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void InitializeMatapelajaran()
        {
            selectMatapelajaran.Items.Clear();
            selectMatapelajaran1.Items.Clear();
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectMatapelajaran.Items.Add("Sila Pilih");
                selectMatapelajaran1.Items.Add("Sila Pilih");
                var sql = "SELECT value FROM parameter where kategori='Matapelajaran'";
                var dat = conn.Query<string>(sql).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectMatapelajaran.Items.Add(dat[i]);
                        selectMatapelajaran1.Items.Add(dat[i]);
                    }

                }
                selectMatapelajaran.SelectedIndex = 0;
                selectMatapelajaran1.SelectedIndex = 0;



            }
        }
        private void RemoveSubjek_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectMatapelajaran.SelectedIndex == 0)
                {
                    MessageBox.Show("Pilih Subjek untuk dibuang", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var buangsubjek = selectMatapelajaran.SelectedItem.ToString()!;
                if (MessageBox.Show("Buang subjek " + buangsubjek + "?" + Environment.NewLine +
               "*Rekod bagi subjek berkenaan akan akan dihapuskan!", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                    {

                        conn.Execute("delete FROM Parameter WHERE Kategori='Matapelajaran' and Value=@namaSubjek", new { namaSubjek = buangsubjek });
                        var oldList = conn.Query<string>("SELECT name from PRAGMA_table_info('PelajarToKandungan') WHERE name like '" + buangsubjek + "%'").ToList();
                        if (oldList.Count > 0)
                        {

                            for (int i = 0; i < oldList.Count; i++)
                            {
                                conn.Execute("ALTER TABLE PelajarToKandungan DROP COLUMN " + oldList[i]);
                            }
                        }

                        InitializeMatapelajaran();

                    }
                    MessageBox.Show("Subjek " + buangsubjek + " berjaya dibuang!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            kSubjek.Visibility = Visibility.Visible;
            kSubjekData.Visibility = Visibility.Visible;
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            kSubjek.Visibility = Visibility.Collapsed;
            kSubjekData.Visibility = Visibility.Collapsed;
        }

        private void TemplateDataSubjek_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderPicker();
            dlg.InputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (dlg.ShowDialog() == true)
            {

                using var wbook = new XLWorkbook();

                var Tema = wbook.Worksheets.Add("Tema");
                Tema.Cell("A1").Value = "Index Tema";
                Tema.Cell("B1").Value = "Tajuk Tema";

                var Bidang = wbook.Worksheets.Add("Bidang");
                Bidang.Cell("A1").Value = "Index Tema";
                Bidang.Cell("B1").Value = "Index Bidang";
                Bidang.Cell("C1").Value = "Tajuk Bidang";

                var Kandungan = wbook.Worksheets.Add("Kandungan");
                Kandungan.Cell("A1").Value = "Index Bidang";
                Kandungan.Cell("B1").Value = "Index Kandungan";
                Kandungan.Cell("C1").Value = "Tajuk Kandungan";

                var Sp = wbook.Worksheets.Add("Standard Pembelajaran");
                Sp.Cell("A1").Value = "Index Bidang";
                Sp.Cell("B1").Value = "Index Kandungan";
                Sp.Cell("C1").Value = "Index Standard Pembelajaran";

                wbook.SaveAs(dlg.ResultPath + "/TemplateDataSubjek.xlsx");
                MessageBox.Show("Template berjaya dibuat di:" + Environment.NewLine + dlg.ResultPath + "\\TemplateDataSubjek.xlsx", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UploadDataSubjek_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Upload Data baru bagi " + " Subjek " + selectMatapelajaran1.SelectedItem.ToString() + " " + selectTingkatan1.SelectedItem.ToString() + "?" + Environment.NewLine +
               "*Record lama subjek bagi tingkatan ini akan dihapsukan!", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // Close the window  

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xlsx;";
                List<TemaModel> data = new List<TemaModel>();
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        var workbook = new XLWorkbook(openFileDialog.FileName);
                        var Worksheet1 = workbook.Worksheet(1);
                        var rows1 = Worksheet1.RangeUsed().RowsUsed().Skip(1); // Skip header row
                        foreach (var row1 in rows1)
                        {

                            TemaModel Tema = new TemaModel { Index = int.Parse(row1.Cell(1).Value.ToString()), Desc = row1.Cell(2).Value.ToString() };
                            var Worksheet2 = workbook.Worksheet(2);
                            var rows2 = Worksheet2.RangeUsed().RowsUsed().Skip(1); // Skip header row
                            foreach (var row2 in rows2)
                            {
                                if (int.Parse(row2.Cell(1).Value.ToString()) == int.Parse(row1.Cell(1).Value.ToString()))
                                {
                                    var Bidang = new BidangModel { Index = int.Parse(row2.Cell(2).Value.ToString()), Desc = row2.Cell(3).Value.ToString() };


                                    var Worksheet3 = workbook.Worksheet(3);
                                    var rows3 = Worksheet3.RangeUsed().RowsUsed().Skip(1); // Skip header row
                                    foreach (var row3 in rows3)
                                    {
                                        if (int.Parse(row3.Cell(1).Value.ToString()) == int.Parse(row2.Cell(2).Value.ToString()))
                                        {
                                            var kandungan = new KandunganModel { Index = int.Parse(row3.Cell(2).Value.ToString()), Desc = row3.Cell(3).Value.ToString() };

                                            var Worksheet4 = workbook.Worksheet(4);
                                            var rows4 = Worksheet4.RangeUsed().RowsUsed().Skip(1); // Skip header row
                                            foreach (var row4 in rows4)
                                            {
                                                if (int.Parse(row4.Cell(1).Value.ToString()) == int.Parse(row2.Cell(2).Value.ToString()) && int.Parse(row4.Cell(2).Value.ToString()) == int.Parse(row3.Cell(2).Value.ToString()))
                                                {
                                                    kandungan.Standard.Add(int.Parse(row4.Cell(3).Value.ToString()));
                                                }
                                            }

                                            Bidang.kandunganModels.Add(kandungan);

                                        }
                                    }
                                    Tema.bidangModels.Add(Bidang);
                                }
                            }

                            data.Add(Tema);

                        }
                        using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                        {
                            conn.Execute("""
                                            delete from KandunganTema where Tingkatan=@Tingkatan and Matapelajaran=@Matapelajaran;
                                            delete from KandunganBidang where Tingkatan=@Tingkatan and Matapelajaran=@Matapelajaran;
                                            delete from KandunganStandard where Tingkatan=@Tingkatan and Matapelajaran=@Matapelajaran;
                                            delete from KandunganData where Tingkatan=@Tingkatan and Matapelajaran=@Matapelajaran;
                                            """, new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString() });
                            foreach (var tema in data)
                            {
                                conn.Execute("insert into KandunganTema (Tingkatan,Matapelajaran,[Index],[Desc]) Values (@Tingkatan,@Matapelajaran,@Index,@Desc)", new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString(), tema.Index, tema.Desc });

                                foreach (var bidang in tema.bidangModels)
                                {
                                    conn.Execute("insert into KandunganBidang (Tingkatan,Matapelajaran,[Index],[Desc],Tema) Values (@Tingkatan,@Matapelajaran,@Index,@Desc,@Tema)", new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString(), bidang.Index, bidang.Desc, Tema = tema.Index });
                                    foreach (var kandungan in bidang.kandunganModels)
                                    {
                                        conn.Execute("insert into KandunganStandard (Tingkatan,Matapelajaran,[Index],[Desc],Tema,Bidang) Values (@Tingkatan,@Matapelajaran,@Index,@Desc,@Tema,@Bidang)", new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString(), kandungan.Index, kandungan.Desc, Tema = tema.Index, Bidang = bidang.Index });
                                        if (kandungan.Standard.Count > 0)
                                        {
                                            foreach (var standard in kandungan.Standard)
                                            {
                                                conn.Execute("insert into KandunganData (Tingkatan,Matapelajaran,StandardPembelajaran,Tema,Bidang,Kandungan) Values (@Tingkatan,@Matapelajaran,@Index,@Tema,@Bidang,@Kandungan)", new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString(), Index = standard, Tema = tema.Index, Bidang = bidang.Index, Kandungan = kandungan.Index });
                                            }
                                        }
                                        else
                                        {
                                            conn.Execute("insert into KandunganData (Tingkatan,Matapelajaran,StandardPembelajaran,Tema,Bidang,Kandungan) Values (@Tingkatan,@Matapelajaran,null,@Tema,@Bidang,@Kandungan)", new { Tingkatan = selectTingkatan1.SelectedItem.ToString()!.Split(" ")[1], Matapelajaran = selectMatapelajaran1.SelectedItem.ToString(), Tema = tema.Index, Bidang = bidang.Index, Kandungan = kandungan.Index });
                                        }

                                    }
                                }
                            }


                        }
                        MessageBox.Show("Upload berjaya", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Upload tidak berjaya, sila cuba lagi", "Upload error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }

            }
        }
    }
}
