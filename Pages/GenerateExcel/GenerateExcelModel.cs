using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qrStudent.Pages.GenerateExcel
{
	public class getStudentDatModel
	{
        public long Id { get; set; }
        public string Nama { get; set; } = "";
    }
	public class getKandunganDataModel
	{
        public string Matapelajaran { get; set; } = "";
        public int Tingkatan { get; set; } 
        public int Tema { get; set; } 
        public int Bidang { get; set; } 
        public int Kandungan { get; set; } 
        public int StandardPembelajaran { get; set; } 
    }
}
