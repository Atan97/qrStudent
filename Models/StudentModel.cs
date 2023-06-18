using DocumentFormat.OpenXml.VariantTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qrStudent.Models
{
    public class StudentModel
    {
        public string Nama { get; set; } = "";
        public string NoPendaftaran { get; set; } = "";
        public string Tingkatan { get; set; } = "";
        public string Kelas { get; set; } = "";
    }
    public class TemaModel
    {
        public int Index { get; set; }
        public string Desc { get; set; } = "";
        public List<BidangModel> bidangModels { get; set; } = new List<BidangModel>();
    }
    public class BidangModel
    {
        public int Index { get; set; }
        public string Desc { get; set; } = "";
        public List<KandunganModel> kandunganModels { get; set; } = new List<KandunganModel>();

    }
    public class KandunganModel
    {
        public int Index { get; set; }
        public string Desc { get; set; } = "";
        public List<int> Standard { get; set; }= new List<int>();

    }
   

}
