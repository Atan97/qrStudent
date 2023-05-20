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

        }

        private void UploadStudent_Click(object sender, RoutedEventArgs e)
        {
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
                            Tingkatan = row.Cell(3).Value.ToString(),
                            Kelas = row.Cell(4).Value.ToString(),
                        };
                        students.Add(studentModel);

                    }
                    using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
                    {
                        var sql = "";
                        foreach (var row in students)
                        {
                            sql = "INSERT INTO SenaraiPelajar (Nama,NoPendaftaran,Tingkatan,Kelas) VALUES('" + row.Nama + "','" + row.NoPendaftaran + "','" + row.Tingkatan + "','" + row.Kelas + "'); SELECT last_insert_rowid()";
                            var dat = conn.QuerySingle<string>(sql);
                            conn.Execute("insert into PelajarToKandungan (IdPelajar) values(" + dat + ")");

                        }
                    }
                    MessageBox.Show("Success.", "Upload success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to upload file, try again.", "Upload error", MessageBoxButton.OK, MessageBoxImage.Error);
                   
                }

            }

        }
    }
}
