using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qrStudent.Pages.ScanStudent
{
    public class ScanStudentModel
    {
        public int Id { get; set; }
        public string Nama { get; set; } = "";
        public string kodKelas { get; set; } = "";
        public string Kelas { get; set; } = "";
        public bool Siap { get; set; } = false;
    }
   
    public class DisplayStudentModel
    {
        public int No { get; set; }
        public int Id { get; set; }
        public string Nama { get; set; } = "";
        public bool Siap { get; set; }
    }
    
}
