using Dapper;
using DocumentFormat.OpenXml.VariantTypes;
using qrStudent.Pages;
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

namespace qrStudent
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            Main.Content = new MainPage();
            BackToMenu.Visibility = Visibility.Hidden;
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}cnfScaler.txt";
            if (File.Exists(path))
            {
                var savedScale = File.ReadLines(path).First();
                slider1.Value = double.Parse(savedScale);
                Main.LayoutTransform = new ScaleTransform(slider1.Value, slider1.Value);
                return;
            }
            generateSqlTable();

            zoomStack.Visibility = Visibility.Visible;
        }



        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            BackToMenu.Visibility = Visibility.Hidden;
            zoomStack.Visibility = Visibility.Visible;
            Main.Content = new MainPage();
        }
        private void slider1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // The user is clicking on the slider, probably about to drag it.

            var scaler = Main.LayoutTransform as ScaleTransform;

            if (scaler != null && scaler.HasAnimatedProperties)
            {
                // This means the current ScaleX and ScaleY properties were set via
                // animation, which has a higher value precedence than a locally set
                // value, so we need to remove the animation by setting a null 
                // AnimationTimeline before we can set a local value when the user
                // drags the slider (in slider1_ValueChanged).

                scaler.ScaleX = scaler.ScaleX;
                scaler.ScaleY = scaler.ScaleY;

                // Remove the animation, causing the local values (set above) to apply.

                scaler.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scaler.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            }
        }
        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var scaler = Main.LayoutTransform as ScaleTransform;
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}cnfScaler.txt";

            if (scaler == null)
            {
                Main.LayoutTransform = new ScaleTransform(slider1.Value, slider1.Value);

            }
            else if (scaler.HasAnimatedProperties)
            {
                // Do nothing because the value is being changed by animation.
                // Setting scaler.ScaleX will cause infinite recursion due to the
                // binding specified in the XAML.
            }
            else
            {
                scaler.ScaleX = slider1.Value;
                scaler.ScaleY = slider1.Value;

                File.WriteAllText(path, slider1.Value.ToString());
            }
        }

        private void generateSqlTable()
        {
            var generateKandunganBidang = """
                    CREATE TABLE IF NOT EXISTS "KandunganBidang" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"Tingkatan"	INTEGER,
                	"Matapelajaran"	TEXT DEFAULT 'SAINS',
                	"Tema"	INTEGER,
                	"Index"	INTEGER,
                	"Desc"	TEXT,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateKandunganData = """
                    CREATE TABLE IF NOT EXISTS "KandunganData" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"Matapelajaran"	TEXT,
                	"Tingkatan"	TEXT,
                	"Tema"	INTEGER,
                	"Bidang"	INTEGER,
                	"Kandungan"	INTEGER,
                	"StandardPembelajaran"	INTEGER,
                	"DtCreated"	datetime DEFAULT current_timestamp,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateKandunganStanadrd = """
                    CREATE TABLE IF NOT EXISTS "KandunganStandard" (
                	"Id"	INTEGER UNIQUE,
                	"Tingkatan"	INTEGER,
                	"Matapelajaran"	TEXT DEFAULT 'SAINS',
                	"Tema"	INTEGER,
                	"Bidang"	INTEGER,
                	"Index"	INTEGER,
                	"Desc"	TEXT,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateKandunganTema = """
                    CREATE TABLE IF NOT EXISTS "KandunganTema" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"Tingkatan"	TEXT,
                	"Matapelajaran"	TEXT DEFAULT 'SAINS',
                	"Index"	INTEGER,
                	"Desc"	TEXT,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateParameter = """
                    CREATE TABLE IF NOT EXISTS "Parameter" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"Kategori"	TEXT,
                	"Value"	TEXT,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateP2K = """
                    CREATE TABLE IF NOT EXISTS "PelajarToKandungan" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"IdPelajar"	INTEGER,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;
            var generateSenaraiPelajar = """
                    CREATE TABLE IF NOT EXISTS "SenaraiPelajar" (
                	"Id"	INTEGER NOT NULL UNIQUE,
                	"Nama"	TEXT,
                	"NoPendaftaran"	TEXT,
                	"Tingkatan"	TEXT,
                	"Kelas"	TEXT,
                	PRIMARY KEY("Id" AUTOINCREMENT)
                );
                """;

            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {
                conn.Execute($"{generateKandunganBidang}{generateKandunganData}{generateKandunganStanadrd}{generateKandunganTema}{generateParameter}{generateP2K}{generateSenaraiPelajar}");

                var cnt = conn.QuerySingleOrDefault<int>("select count(Id) from Parameter where Kategori='Tingkatan'");
                if (cnt < 1)
                {
                    var tingkatan = "";
                    for (int i = 1; i < 6; i++)
                    {
                        tingkatan += $"('Tingkatan','{i}'),";
                    }

                    conn.Execute($"Insert into Parameter (Kategori,Value) Values {tingkatan.Remove(tingkatan.Length - 1, 1)}");
                }
            }

        }
    }
}
