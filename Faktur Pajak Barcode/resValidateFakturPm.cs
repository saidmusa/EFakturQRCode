using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faktur_Pajak_Barcode
{
    public class resValidateFakturPm
    {
        //Root resValidateFakturPm
        public string kdJenisTransaksi { get; set; }
        public string fgPengganti { get; set; }
        public string nomorFaktur { get; set; }
        public string tanggalFaktur { get; set; }
        public string npwpPenjual { get; set; }
        public string namaPenjual { get; set; }
        public string alamatPenjual { get; set; }
        public string npwpLawanTransaksi { get; set; }
        public string namaLawanTransaksi { get; set; }
        public string alamatLawanTransaksi { get; set; }
        public string jumlahDpp { get; set; }
        public string jumlahPpn { get; set; }
        public string jumlahPpnBm { get; set; }
        public string statusFaktur { get; set; }
        public string statusApproval { get; set; }
        public string referensi { get; set; }
    }
}
