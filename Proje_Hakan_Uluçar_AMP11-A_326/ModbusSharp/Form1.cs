﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SQLite;
using ToolTip = System.Windows.Forms.ToolTip;// bu satırı eklemediğimde hata veriyodu o yüzden bu satırı yazdım.
namespace ModbusSharp
{
    public partial class Form1 : Form
    {
        byte[] inDataa = new byte[100];
        int iUzunluka;
        bool hata=false;
        public Form1()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
        private void listele()
        {
            SQLiteConnection baglanti = new SQLiteConnection("Data Source=C:\\Users\\hakan\\OneDrive\\Masaüstü\\Proje_Hakan_Uluçar_AMP11-A_326\\AsenkronMotorÇalışmaSaatleri.db;version=3");
            baglanti.Open();
            SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT * FROM Motor", baglanti);
            DataSet ds = new DataSet();
            da.Fill(ds, "Motor");
            dataGridView1.DataSource = ds.Tables["Motor"];
            baglanti.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listele();
            btnBaglantiKes.Enabled = false;
            groupBox2.Enabled = false;
            if (SerialPort.GetPortNames().Length != 0)
            {
                String[] portlar = SerialPort.GetPortNames();
                cmbPortAdi.Text = portlar[0];
                foreach (string port in portlar)
                {
                    cmbPortAdi.Items.Add(port);
                }
            }
            ToolTip yourToolTip = new ToolTip();
            //The below are optional, of course,

            yourToolTip.ToolTipIcon = ToolTipIcon.Info;
            yourToolTip.ToolTipTitle = "Komutlar";
            yourToolTip.IsBalloon = true;
            yourToolTip.ShowAlways = true;

            yourToolTip.SetToolTip(label5, 
                "01- Tek Bobin Durumu Oku \n" +
                "02- Giriş Durumu Oku\n03- Tutucu Registerleri Oku \n" +
                "04- Giriş Registerleri Oku \n" +
                "05- Sadece Bir bobin durumu değiştir \n" +
                "06- Sadece Bir Register durumunu değiştir \n" +
                "0F- Birden fazla Bobin içeriği değiştir \n" +
                "10- Birden fazla Registere Değer atamak ");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.PortName = cmbPortAdi.Text;
                    serialPort1.BaudRate = Convert.ToInt16(cmbBaudrate.Text);
                    serialPort1.Open();
                    pictureBox1.Image = Properties.Resources.yesillamba;
                    groupBox1.Enabled = false;
                    btnBaglan.Enabled = false;
                    btnBaglantiKes.Enabled = true;
                    groupBox2.Enabled = true;
                    label4.Text = "Online";

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Hata :" + exception);

                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                    pictureBox1.Image = Properties.Resources.kirmizilamba;
                    groupBox1.Enabled = true;
                    btnBaglan.Enabled = true;
                    btnBaglantiKes.Enabled = false;
                    groupBox2.Enabled = false;
                    label4.Text = "Offline";

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Hata :" + exception);

                }

            }
        }
        public void veriHazirla()
        {
            int iUzunluk;
            String komut = txtAdres.Text + txtKomut.Text + txtParametre.Text;
            byte[] inData = new byte[100];
            int j, jj, ii;
            int reg_crc;

            if (komut.Length == 0)
            {
                MessageBox.Show(this, "Komut Girilmedi \n - Lütfen geçerli bir komut dizesi Giriniz...", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAdres.Focus();
                hata = true;
                return;
            }

            iUzunluk = komut.Length;
           
                if (iUzunluk % 2 != 0)
                {
                    MessageBox.Show(this, "Hatalı Komut Dizesi \n- Lütfen komutununuzun dizenizin doğruluğunu kontrol ediniz...", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtAdres.Focus();
                    hata = true;
                return;
                
                }


                // CRC Hesaplama
                iUzunluk = iUzunluk / 2;
                for (ii = 0; ii < iUzunluk; ii++)
                {
                    inData[ii] = (byte)Convert.ToInt32(komut.Substring(ii * 2, 2), 16);

                }
                ii = 0;
                jj = iUzunluk;
                reg_crc = 0XFFFF;
                do
                {
                    reg_crc = reg_crc ^ inData[ii];
                    ii++;
                    for (j = 0; j <= 7; j++)
                    {
                        if ((reg_crc & 0X1) == 1)
                        {
                            reg_crc = (reg_crc >> 1) ^ 0XA001;
                        }
                        else
                        {
                            reg_crc = (reg_crc >> 1);
                        }

                    }
                    jj--;


                } while (jj != 0);
                inData[iUzunluk] = (byte)(reg_crc & 0XFF);
                inData[iUzunluk + 1] = (byte)((reg_crc >> 8) & 0XFF);
                textBox4.Text = (Convert.ToString(inData[iUzunluk], 16) + Convert.ToString(inData[iUzunluk + 1], 16)).ToUpper();
                labelKomutDizesi.Text = "Komut Dizesi : " + komut + textBox4.Text;
            inDataa = inData;
            iUzunluka = iUzunluk;
            hata = false;

        }
        

       DateTime dt = DateTime.Now;
        private void btnGonder_Click(object sender, EventArgs e)
        {
            veriHazirla();
            if (!hata)
            {
                serialPort1.Write(inDataa, 0, iUzunluka + 2);
                txtCevap.AppendText("*");
            }
            SQLiteConnection baglanti = new SQLiteConnection("Data Source=C:\\Users\\hakan\\OneDrive\\Masaüstü\\Proje_Hakan_Uluçar_AMP11-A_326\\AsenkronMotorÇalışmaSaatleri.db;version=3");
            baglanti.Open();
            SQLiteCommand komut = new SQLiteCommand();
            komut.Connection = baglanti;
            if (txtParametre.Text == "20000002")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Çalışıyor... ')";
            else if (txtParametre.Text == "20000001")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Çalışmıyor... ')";
            else if (txtParametre.Text == "20010DAC")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Hızlı Dönüyor... ')";
            else if (txtParametre.Text == "200103E8")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Yavaş Dönüyor... ')";
            else if (txtParametre.Text == "20000022")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Ters Dönüyor... ')";
            else if (txtParametre.Text == "20000012")
                komut.CommandText = "INSERT INTO Motor VALUES('" + dt + "',' Motor Düz Dönüyor... ')";
            komut.ExecuteNonQuery();
            baglanti.Close();
            listele();
        }

       
        public delegate void veriGoster(String veri);

        public void texteYaz(String veri)
        {
            txtCevap.Text += veri;
        }
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int rSayi,ic;
            byte gelenVeri;
            string gelenVeriString=null;
            
            veriGoster ekle=new veriGoster(texteYaz);
            rSayi = serialPort1.BytesToRead;

            for (ic = 1; ic <= rSayi; ic++)
            {
                gelenVeri=(byte) serialPort1.ReadByte();
                gelenVeriString = gelenVeri.ToString("X2");
                txtCevap.Invoke(ekle,","+gelenVeriString);
            }
            
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                veriHazirla();
                timer1.Enabled = true;
            }else
            {
                timer1.Enabled = false;
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (!hata)
                {
                    serialPort1.Write(inDataa, 0, iUzunluka + 2);
                    txtCevap.AppendText("*");
                }
            }
        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbPortAdi_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            txtParametre.Text = "20000002";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtParametre.Text = "20000001";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txtParametre.Text = "20010DAC";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtParametre.Text = "200103E8";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            txtParametre.Text = "20000022";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            txtParametre.Text = "20000012";
        }

        private void button7_Click(object sender, EventArgs e)
        {
           
        }
    }
}