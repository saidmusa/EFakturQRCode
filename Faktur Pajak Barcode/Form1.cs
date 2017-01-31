using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Faktur_Pajak_Barcode
{
    public partial class Form1 : Form
    {
        private int rowIndex = 0;
        public Form1()
        {
            InitializeComponent();
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = true;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        resValidateFakturPm deserializeXML(string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<resValidateFakturPm>), new XmlRootAttribute("faktur"));
            StringReader stringReader = new StringReader(xmlString);
            List<resValidateFakturPm> resValidateFakturPms = (List<resValidateFakturPm>)serializer.Deserialize(stringReader);
            return resValidateFakturPms[0];
        }

        string reformatXML(string xml)
        {
            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>", "");
            xml = "<faktur>" + xml + "</faktur>";
            return xml;
        }

        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
                CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        string getResponse(string url)
        {
            try
            {
                WebClient client = new WebClient();

                client.Headers["User-Agent"] =
                    "Googlebot/2.1 (+http://www.googlebot.com/bot.html)";

                byte[] arr = client.DownloadData(url);

                string contentEncoding = client.ResponseHeaders["Content-Encoding"];

                string response = System.Text.Encoding.UTF8.GetString(arr);
                //Console.WriteLine(response);
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

        void addDataToGridView(resValidateFakturPm efaktur)
        {
            BindingSource gvBs = (BindingSource)dataGridView1.DataSource;
            if (gvBs == null)
            {
                BindingSource newBs = new BindingSource();
                newBs.DataSource = typeof(resValidateFakturPm);
                newBs.Add(efaktur);
                dataGridView1.DataSource = newBs;
                dataGridView1.AutoGenerateColumns = true;
            }
            else
            {
                gvBs.Add(efaktur);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string response = getResponse(textBox1.Text);
                if (response == null) return;

                response = reformatXML(response);
                resValidateFakturPm efaktur = deserializeXML(response);
                if (efaktur.statusApproval != "Faktur Valid, Sudah Diapprove oleh DJP")
                {
                    MessageBox.Show("Faktur tidak valid");
                    return;
                }

                addDataToGridView(efaktur);                
                textBox1.Text = "";
            }
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataGridView gv = (DataGridView)sender;
            foreach (DataGridViewColumn col in gv.Columns)
            {
                col.ReadOnly = true;
            }
        }
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.dataGridView1.Rows[e.RowIndex].Selected = true;
                this.rowIndex = e.RowIndex;
                this.dataGridView1.CurrentCell = this.dataGridView1.Rows[e.RowIndex].Cells[1];
                this.contextMenuStrip1.Show(this.dataGridView1, e.Location);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            if (!this.dataGridView1.Rows[this.rowIndex].IsNewRow)
            {
                this.dataGridView1.Rows.RemoveAt(this.rowIndex);
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string csvString = "FM,KD_JENIS_TRANSAKSI,FG_PENGGANTI,NOMOR_FAKTUR,MASA_PAJAK,TAHUN_PAJAK,TANGGAL_FAKTUR,NPWP,NAMA,ALAMAT_LENGKAP,JUMLAH_DPP,JUMLAH_PPN,JUMLAH_PPNBM,IS_CREDITABLE\n";
            BindingSource gvBs = (BindingSource)dataGridView1.DataSource;

            foreach (resValidateFakturPm efaktur in gvBs.List)
            {
                csvString += "FM,";
                csvString += "\"" + efaktur.kdJenisTransaksi + "\"" + ",";
                csvString += "\"" + efaktur.fgPengganti + "\"" + ",";
                csvString += "\"" + efaktur.nomorFaktur + "\"" + ",";
                csvString += "\"" + efaktur.tanggalFaktur.Substring(3, 2) + "\"" + ",";
                csvString += "\"" + efaktur.tanggalFaktur.Substring(6, 4) + "\"" + ",";
                csvString += "\"" + efaktur.tanggalFaktur + "\"" + ",";
                csvString += "\"" + efaktur.npwpPenjual + "\"" + ",";
                csvString += "\"" + efaktur.namaPenjual + "\"" + ",";
                csvString += "\"" + efaktur.alamatPenjual + "\"" + ",";
                csvString += "\"" + efaktur.jumlahDpp + "\"" + ",";
                csvString += "\"" + efaktur.jumlahPpn + "\"" + ",";
                csvString += "\"" + efaktur.jumlahPpnBm + "\"" + ",";
                csvString += "1\n";
            }

            using (FileStream stream = File.Open(saveFileDialog1.FileName, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(csvString);
            }
        }
    }
}
