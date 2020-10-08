using MetroFramework.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YilanOyunu.Properties;

/*
UYGULAMAMI NASIL YAYINLARIM?
https://docs.microsoft.com/en-us/visualstudio/deployment/deploying-applications-services-and-components?view=vs-2019
1. ClickOnce Publishing
2. InstallShield LE
https://info.revenera.com/IS-EVAL-InstallShield-Limited-Edition-Visual-Studio
*/

namespace YilanOyunu
{
    public partial class Form1 : MetroForm
    {
        int satirSayisi = 10;
        int sutunSayisi = 10;
        int bogumBoyut;
        List<Point> yilan;
        int xYon = 1, yYon = 0; // DEFAULT: Sağa gitsin
        bool yonDegisti = false;
        Point yem;
        Random rnd = new Random();
        bool oyunBittiMi = false;
        int puan = 0;
        bool kolayMi = false;
        string oyuncu;
        List<string> skorlar = new List<string>();

        public Form1()
        {
            EskiSkorlariOku();
            Icon = Resources.favicon;
            InitializeComponent();
            TitremeyiAzalt();
            bogumBoyut = saha.Height / satirSayisi;
            YilanUret();
            YemUret();
        }

        private void EskiSkorlariOku()
        {
            try
            {
                skorlar = File.ReadAllLines("skorlar.txt").ToList();
            }
            catch (Exception)
            {
            }
        }

        private void TitremeyiAzalt()
        {
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, saha, new object[] { true });
        }

        private void YemUret()
        {
            int x, y;

            do
            {
                x = rnd.Next(0, sutunSayisi);
                y = rnd.Next(0, satirSayisi);
            } while (YilaninUzerindeMi(x, y));

            yem = new Point(x, y);
        }

        private bool YilaninUzerindeMi(int x, int y)
        {
            foreach (Point nokta in yilan)
            {
                if (nokta.X == x && nokta.Y == y)
                {
                    return true;
                }
            }

            return false;
        }

        // bu metotta yılanın boğumlarının satır ve sütun değerleri hesaplanır
        private void YilanUret()
        {
            Point orta = new Point(
                sutunSayisi / 2,
                satirSayisi / 2);

            yilan = new List<Point>
            {
                orta, // bas
                new Point(orta.X - 1, orta.Y),
                new Point(orta.X - 2, orta.Y) // kuyruk
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                if (oyunBittiMi)
                {
                    OyunuYenidenBaslat();
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                if (timer1.Enabled)
                {
                    timer1.Stop();
                    lblDurdu.Show();
                }
                else
                {
                    timer1.Start();
                    lblDurdu.Hide();
                    lblAciklama.Hide();
                }
            }

            // 1 tick süresi içinde zaten bir kere yön değişti
            if (yonDegisti || !timer1.Enabled)
                return base.ProcessCmdKey(ref msg, keyData);

            int xYeniYon, yYeniYon;
            switch (keyData)
            {
                case Keys.Up:
                    xYeniYon = 0; yYeniYon = -1;
                    break;
                case Keys.Down:
                    xYeniYon = 0; yYeniYon = +1;
                    break;
                case Keys.Right:
                    xYeniYon = +1; yYeniYon = 0;
                    break;
                case Keys.Left:
                    xYeniYon = -1; yYeniYon = 0;
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            // yeni yön eski yönle zıt değilse
            if (xYeniYon * xYon != -1 && yYeniYon * yYon != -1)
            {
                xYon = xYeniYon;
                yYon = yYeniYon;
                yonDegisti = true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // yeni oyun için tüm değerleri varsayılan değerlerine eşitliyoruz
        private void OyunuYenidenBaslat()
        {
            puan = 0;
            lblPuan.Text = "000";
            oyunBittiMi = false;
            yonDegisti = false;
            xYon = +1;
            yYon = 0;
            lblOyunBitti.Hide();
            YilanUret();
            YemUret();
            saha.Refresh();
            timer1.Interval = 500;
            timer1.Start();
        }

        private void saha_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            YilanCiz(g);
            YemCiz(g);
        }

        private void YemCiz(Graphics g)
        {
            int x = yem.X * bogumBoyut;
            int y = yem.Y * bogumBoyut;
            g.FillRectangle(Brushes.Red, x, y, bogumBoyut, bogumBoyut);
            g.DrawRectangle(Pens.Black, x, y, bogumBoyut - 1, bogumBoyut - 1);
        }

        private void YilanCiz(Graphics g)
        {
            BogumCiz(g, yilan[0].X, yilan[0].Y, true); // BAŞ

            for (int i = 1; i < yilan.Count; i++)
            {
                BogumCiz(g, yilan[i].X, yilan[i].Y);
            }
        }

        private void BogumCiz(Graphics g, int sutunNo, int satirNo, bool basMi = false)
        {
            int x = sutunNo * bogumBoyut;
            int y = satirNo * bogumBoyut;
            g.FillRectangle(basMi ? Brushes.Gray : Brushes.White, x, y, bogumBoyut, bogumBoyut);
            g.DrawRectangle(Pens.Black, x, y, bogumBoyut - 1, bogumBoyut - 1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // metro form bug fix
            BringToFront();
        }

        private void tsmiKolay_Click(object sender, EventArgs e)
        {
            kolayMi = true;
            tsmiKolay.Checked = true;
            tsmiZor.Checked = false;
        }

        private void tsmiZor_Click(object sender, EventArgs e)
        {
            kolayMi = false;
            tsmiKolay.Checked = false;
            tsmiZor.Checked = true;
        }

        private void tsmiSkorlar_Click(object sender, EventArgs e)
        {
            SkorlarForm frmSkorlar = new SkorlarForm();
            frmSkorlar.ShowDialog();
        }

        private void btnOyunuBaslat_Click(object sender, EventArgs e)
        {
            string ad = txtOyuncu.Text.Replace(";", "").Trim();

            if (ad == "")
            {
                MessageBox.Show("Lütfen adınızı giriniz.");
                return;
            }

            oyuncu = ad;
            Text = "Yılan Oyunu - " + ad;
            Refresh();
            pnlGiris.Hide();
            saha.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point bas = yilan[0];
            Point yeniBas = kolayMi 
                ? new Point((bas.X + 1 * xYon + sutunSayisi) % sutunSayisi, (bas.Y + 1 * yYon + satirSayisi) % satirSayisi)
                : new Point(bas.X + 1 * xYon, bas.Y + 1 * yYon);

            // YENİ BAŞI İNSERT ETMEDEN ÖNCE KAFASINI BİR YERE VURUYOR MU DİYE KONTROL EDELİM
            if (YilaninUzerindeMi(yeniBas.X, yeniBas.Y)
                || yeniBas.X < 0 || yeniBas.X >= sutunSayisi
                || yeniBas.Y < 0 || yeniBas.Y >= satirSayisi)
            {
                SkorKaydet();
                timer1.Stop();
                lblOyunBitti.Text = string.Format("OYUN BİTTİ\r\n\r\nSKORUNUZ: {0:000}\r\n\r\nTEKRAR OYNA (F2)", puan);
                lblOyunBitti.Show();
                oyunBittiMi = true;
                return;
            }

            yilan.Insert(0, yeniBas);

            // YEMİ YUTTU MU?
            if (yeniBas == yem)
            {
                puan += kolayMi ? 5 : 10;
                YemUret();
                if (timer1.Interval > 50)
                    timer1.Interval -= 10;
            }
            else
            {
                // yutmadıysa kuyruğunu sil
                yilan.RemoveAt(yilan.Count - 1);
            }

            lblPuan.Text = puan.ToString("000");
            saha.Refresh();
            yonDegisti = false; // yılan hareket etti, tekrar yön değiştirebilir
        }

        private void SkorKaydet()
        {
            string skorMetin = 
                string.Format("{0:00000};{1};{2}", puan, DateTime.Now.ToString("s"), oyuncu);
            skorlar.Add(skorMetin);
            skorlar.Sort();
            skorlar.Reverse();
            File.WriteAllLines("skorlar.txt", skorlar);
        }
    }
}
