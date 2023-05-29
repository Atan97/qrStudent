﻿using Dapper;
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
    /// Interaction logic for ScanStudentSelect.xaml
    /// </summary>
    public partial class ScanStudentSelect : Page
    {

        public ScanStudentSelect()
        {
            InitializeComponent();
            InitializeTingkatan();
            InitializeMatapelajaran();
            InitializeKelas();

            CariKelas.IsEnabled = false;

            selectStandard.IsEnabled = false;
            selectTema.IsEnabled = false;
            selectBidang.IsEnabled = false;

        }



        private void selectKelas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkMainSelected();
        }

        private void selectMatapelajaran_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkMainSelected();
        }

        private void selectTema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectMatapelajaran.SelectedIndex!=0)
            {


                selectBidang.Items.Clear();
                selectStandard.Items.Clear();
                selectStandard.IsEnabled = false;
                if (selectTema.SelectedIndex > 0)
                {

                    InitializeBidang(selectMatapelajaran.SelectedItem.ToString()!, selectTingkatan.SelectedItem.ToString()!, selectTema.SelectedItem.ToString()!.Split(")")[0]);
                    selectBidang.IsEnabled = true;
                }
                else
                {
                    selectBidang.IsEnabled = false;
                }
            }

        }

        private void selectBidang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectTema.SelectedIndex != 0)
            {
                selectStandard.Items.Clear();
                if (selectBidang.SelectedIndex > 0)
                {

                    InitializeStandard(selectMatapelajaran.SelectedItem.ToString()!, selectTingkatan.SelectedItem.ToString()!, selectTema.SelectedItem.ToString()!.Split(")")[0], selectBidang.SelectedItem.ToString()!.Split(")")[0]);
                    selectStandard.IsEnabled = true;
                }
                else
                {
                    selectStandard.IsEnabled = false;
                }
            }
        }

        private void selectStandard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectStandard.SelectedIndex>0)
            {
                CariKelas.IsEnabled = true;
            }
            else
            {
                CariKelas.IsEnabled = false;
            }
        }

        private void CariKelas_Click(object sender, RoutedEventArgs e)
        {

            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                
                var sql = "SELECT count(Kelas) FROM SenaraiPelajar where Tingkatan=@tingkatan and Kelas=@kelas COLLATE NOCASE";
                var dat = conn.QuerySingleOrDefault<int>(sql,new { tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1] , kelas = selectKelas.SelectedItem.ToString()! });

                if (dat>0)
                {
                    ScanStudentListPage ad=new ScanStudentListPage(new ScanStudentModel { Tingkatan = selectTingkatan.SelectedItem.ToString()!.Split(" ")[1], Kelas = selectKelas.SelectedItem.ToString()! });
                    this.NavigationService.Navigate(ad);
                }
                else
                {
                    MessageBox.Show("Tiada maklumat pelajar bagi "+ selectTingkatan.SelectedItem.ToString() + " Kelas "+ selectKelas.SelectedItem.ToString() + "!", "Empty File.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }



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
        private void InitializeTema(string matapelajaran, string tingkatan)
        {
            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectTema.Items.Add("Sila Pilih");
                var sql = "SELECT [Index]||') ' ||Desc FROM KandunganTema where Tingkatan=@tingkatan and Matapelajaran=@matapelajaran  ORDER BY [Index]";
                var dat = conn.Query<string>(sql, new { tingkatan = tingkatan.Split(" ")[1], matapelajaran }).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectTema.Items.Add(dat[i]);
                    }

                }
                selectTema.SelectedIndex = 0;



            }
        }
        private void InitializeBidang(string matapelajaran, string tingkatan, string tema)
        {

            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectBidang.Items.Add("Sila Pilih");
                var sql = "SELECT [Index]||') ' ||Desc  FROM KandunganBidang where Tingkatan=@tingkatan and Matapelajaran=@matapelajaran and Tema=@tema  ORDER BY [Index]";
                var dat = conn.Query<string>(sql, new { tingkatan = tingkatan.Split(" ")[1], matapelajaran, tema }).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectBidang.Items.Add(dat[i]);
                    }

                }
                selectBidang.SelectedIndex = 0;



            }
        }
        private void InitializeStandard(string matapelajaran, string tingkatan, string tema, string bidang)
        {

            using (var conn = new SQLiteConnection(@"Data Source= qrStudentDB.db;Version=3;"))
            {

                selectStandard.Items.Add("Sila Pilih");
                var sql = "SELECT [Index]||') ' ||Desc  FROM KandunganStandard where Tingkatan=@tingkatan and Matapelajaran=@matapelajaran and Tema=@tema and Bidang=@bidang ORDER BY [Index]";
                var dat = conn.Query<string>(sql, new { tingkatan = tingkatan.Split(" ")[1], matapelajaran, tema, bidang }).ToList();
                if (dat.Count > 0)
                {
                    for (int i = 0; i < dat.Count; i++)
                    {
                        selectStandard.Items.Add(dat[i]);
                    }

                }
                selectStandard.SelectedIndex = 0;



            }
        }
        private void selectTingkatan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkMainSelected();

        }

        private void checkMainSelected()
        {
            selectStandard.Items.Clear();
            selectTema.Items.Clear();
            selectBidang.Items.Clear();
            selectStandard.IsEnabled = false;
            selectBidang.IsEnabled = false;
            if (selectKelas.SelectedIndex != 0 && selectTingkatan.SelectedIndex != 0 && selectMatapelajaran.SelectedIndex != 0)
            {

                selectTema.IsEnabled = true;
                InitializeTema(selectMatapelajaran.SelectedItem.ToString()!, selectTingkatan.SelectedItem.ToString()!);
            }
            else
            {
                selectTema.IsEnabled = false;
            }
        }

       
    }
}