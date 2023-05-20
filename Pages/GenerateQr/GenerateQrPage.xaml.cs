using Dapper;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Win32;
using QRCoder;
using qrStudent.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static QRCoder.QRCodeGenerator;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace qrStudent.Pages.GenerateQr
{
    /// <summary>
    /// Interaction logic for GenerateQrPage.xaml
    /// </summary>
    public partial class GenerateQrPage : Page
    {
        public GenerateQrPage()
        {
            InitializeComponent();
            InitializeTingkatan();
            InitializeKelas();
            DownloadQr.IsEnabled = false;
            GetQrLogo();
        }

        private void GetQrLogo()
        {
            bool exists = System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Assets");

            if (!exists)
                System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "Assets");
            string selectedFileName = System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png";
            if (File.Exists(selectedFileName))
            {
                using (var fs = new FileStream(selectedFileName, FileMode.Open))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = fs;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    QrLogo.Source = bitmap;
                }
            }
            else
            {
                QrLogo.Source = new BitmapImage(new Uri(@"/Resources/PlaceHolderImage.png", UriKind.RelativeOrAbsolute));
            }
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

        //private void InitializeSubjek()
        //{
        //    using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
        //    {

        //        selectSubjek.Items.Add("Sila Pilih");
        //        var sql = "SELECT value FROM parameter where kategori='Subjek'"; 
        //            var dat = conn.Query<string>(sql).ToList();
        //        if (dat.Count>0)
        //        {
        //            for (int i = 0; i < dat.Count; i++)
        //            {
        //                selectSubjek.Items.Add(dat[i]);
        //            }

        //        }
        //        selectSubjek.SelectedIndex = 0;



        //    }
        //}
        private void checkIfSelected()
        {
            if (selectKelas.SelectedIndex == 0 || selectTingkatan.SelectedIndex == 0)
            {
                DownloadQr.IsEnabled = false;
                return;
            }
            DownloadQr.IsEnabled = true;
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
        public Bitmap MakeSquarePhoto(Bitmap bmp, int size)
        {

            Bitmap res = new Bitmap(size, size);
            Graphics g = Graphics.FromImage(res);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, size, size);
            int t = 0, l = 0;
            if (bmp.Height > bmp.Width)
                t = (bmp.Height - bmp.Width) / 2;
            else
                l = (bmp.Width - bmp.Height) / 2;
            g.DrawImage(bmp, new Rectangle(0, 0, size, size), new Rectangle(l, t, bmp.Width - l * 2, bmp.Height - t * 2), GraphicsUnit.Pixel);
            return res;

        }
        private void selectTingkatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string tingkatan = selectTingkatan.SelectedItem.ToString()!;

            checkIfSelected();

        }
        private void selectKelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSelected();
        }

        private void DownloadQr_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                var sql = "SELECT Nama,Tingkatan,Kelas FROM SenaraiPelajar where Tingkatan=@t and Kelas=@k COLLATE NOCASE";
                var dat = conn.Query<StudentModel>(sql, new { t = selectTingkatan.SelectedItem.ToString()!.Last(), k = selectKelas.SelectedItem.ToString()! }).ToList();
                bool exists = System.IO.Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Qr Pelajar\\"+ selectTingkatan.SelectedItem.ToString()+" Kelas "+ selectKelas.SelectedItem.ToString());

                if (!exists)
                    System.IO.Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "Qr Pelajar\\" + selectTingkatan.SelectedItem.ToString() + " Kelas " + selectKelas.SelectedItem.ToString());
                if (dat.Count > 0)
                {
                    foreach (var item in dat)
                    {
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(item.Nama + "," + item.Tingkatan + "," + item.Kelas, ECCLevel.Q);

                        QRCode qrCode = new QRCode(qrCodeData);
                        if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png"))
                        {
                            using (Bitmap myBitmap = new Bitmap(System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png"))
                            {
                                using (Bitmap qrCodeImage = qrCode.GetGraphic(20, Color.Black, Color.White, icon: myBitmap, iconSizePercent: 20))
                                {
                                    var qrName = System.AppDomain.CurrentDomain.BaseDirectory + "Qr Pelajar\\" + selectTingkatan.SelectedItem.ToString() + " Kelas " + selectKelas.SelectedItem.ToString() + "\\" + item.Nama + ".png";
                                    if (System.IO.File.Exists(qrName))
                                    {

                                        System.IO.File.Delete(qrName);
                                    }
                                    qrCodeImage.Save(qrName, ImageFormat.Png);
                                    qrCodeImage.Dispose();
                                }
                            }
                        }
                        else
                        {
                            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                            {
                                var qrName = System.AppDomain.CurrentDomain.BaseDirectory + "Qr Pelajar\\" + selectTingkatan.SelectedItem.ToString() + " Kelas " + selectKelas.SelectedItem.ToString() + "\\" + item.Nama + ".png";
                                if (System.IO.File.Exists(qrName))
                                {

                                    System.IO.File.Delete(qrName);
                                }
                                qrCodeImage.Save(qrName, ImageFormat.Png);
                                qrCodeImage.Dispose();
                            }
                        }
                    }
                    MessageBox.Show( "Download Sukses! Direktori boleh didapati di "+Environment.NewLine+ System.AppDomain.CurrentDomain.BaseDirectory + "Qr Pelajar\\" + selectTingkatan.SelectedItem.ToString() + " Kelas " + selectKelas.SelectedItem.ToString(), "Success.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Tiada maklumat pelajar!", "Empty File.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            }

        }

        private void pilihLogo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                var imageDat = MakeSquarePhoto(new Bitmap(openFileDialog.FileName), 256);
                QrLogo.Source = null;
                if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png"))
                {

                    System.IO.File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png");
                }
                imageDat.Save(System.AppDomain.CurrentDomain.BaseDirectory + "Assets\\LogoQr.png", ImageFormat.Png);
                imageDat.Dispose();


                GetQrLogo();
            }
        }
    }
}
