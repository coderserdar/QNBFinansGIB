namespace QNBFinansGIB
{
    partial class frmAnaSayfa
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnXmlOlustur = new System.Windows.Forms.Button();
            this.btnGIBOnizleme = new System.Windows.Forms.Button();
            this.btnServiseGonder = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnFaturaSil = new System.Windows.Forms.Button();
            this.btnBelgeOidKontrol = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lbEFaturaKullaniciListesi = new System.Windows.Forms.ListBox();
            this.dtpFaturaTarihi = new System.Windows.Forms.DateTimePicker();
            this.btnEFaturaKullaniciListesi = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnXmlOlustur
            // 
            this.btnXmlOlustur.Location = new System.Drawing.Point(6, 30);
            this.btnXmlOlustur.Name = "btnXmlOlustur";
            this.btnXmlOlustur.Size = new System.Drawing.Size(116, 31);
            this.btnXmlOlustur.TabIndex = 0;
            this.btnXmlOlustur.Text = "XML Oluştur";
            this.btnXmlOlustur.UseVisualStyleBackColor = true;
            this.btnXmlOlustur.Click += new System.EventHandler(this.btnXmlOlustur_Click);
            // 
            // btnGIBOnizleme
            // 
            this.btnGIBOnizleme.Location = new System.Drawing.Point(128, 30);
            this.btnGIBOnizleme.Name = "btnGIBOnizleme";
            this.btnGIBOnizleme.Size = new System.Drawing.Size(174, 31);
            this.btnGIBOnizleme.TabIndex = 1;
            this.btnGIBOnizleme.Text = "GIB Fatura Çıktı Önizleme";
            this.btnGIBOnizleme.UseVisualStyleBackColor = true;
            this.btnGIBOnizleme.Click += new System.EventHandler(this.btnGIBOnizleme_Click);
            // 
            // btnServiseGonder
            // 
            this.btnServiseGonder.Location = new System.Drawing.Point(308, 30);
            this.btnServiseGonder.Name = "btnServiseGonder";
            this.btnServiseGonder.Size = new System.Drawing.Size(165, 31);
            this.btnServiseGonder.TabIndex = 2;
            this.btnServiseGonder.Text = "Servise Gönder";
            this.btnServiseGonder.UseVisualStyleBackColor = true;
            this.btnServiseGonder.Click += new System.EventHandler(this.btnServiseGonder_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnFaturaSil);
            this.groupBox1.Controls.Add(this.btnBelgeOidKontrol);
            this.groupBox1.Controls.Add(this.btnXmlOlustur);
            this.groupBox1.Controls.Add(this.btnServiseGonder);
            this.groupBox1.Controls.Add(this.btnGIBOnizleme);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(651, 119);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "E-Fatura ve E-Arşiv";
            // 
            // btnFaturaSil
            // 
            this.btnFaturaSil.Location = new System.Drawing.Point(223, 72);
            this.btnFaturaSil.Name = "btnFaturaSil";
            this.btnFaturaSil.Size = new System.Drawing.Size(165, 31);
            this.btnFaturaSil.TabIndex = 4;
            this.btnFaturaSil.Text = "Fatura Sil";
            this.btnFaturaSil.UseVisualStyleBackColor = true;
            this.btnFaturaSil.Click += new System.EventHandler(this.btnFaturaSil_Click);
            // 
            // btnBelgeOidKontrol
            // 
            this.btnBelgeOidKontrol.Location = new System.Drawing.Point(479, 30);
            this.btnBelgeOidKontrol.Name = "btnBelgeOidKontrol";
            this.btnBelgeOidKontrol.Size = new System.Drawing.Size(165, 31);
            this.btnBelgeOidKontrol.TabIndex = 3;
            this.btnBelgeOidKontrol.Text = "Belge Oid Kontrol";
            this.btnBelgeOidKontrol.UseVisualStyleBackColor = true;
            this.btnBelgeOidKontrol.Click += new System.EventHandler(this.btnBelgeOidKontrol_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lbEFaturaKullaniciListesi);
            this.groupBox2.Controls.Add(this.dtpFaturaTarihi);
            this.groupBox2.Controls.Add(this.btnEFaturaKullaniciListesi);
            this.groupBox2.Location = new System.Drawing.Point(12, 121);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(651, 128);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "E-Fatura";
            // 
            // lbEFaturaKullaniciListesi
            // 
            this.lbEFaturaKullaniciListesi.FormattingEnabled = true;
            this.lbEFaturaKullaniciListesi.Location = new System.Drawing.Point(319, 28);
            this.lbEFaturaKullaniciListesi.Name = "lbEFaturaKullaniciListesi";
            this.lbEFaturaKullaniciListesi.Size = new System.Drawing.Size(325, 95);
            this.lbEFaturaKullaniciListesi.TabIndex = 5;
            // 
            // dtpFaturaTarihi
            // 
            this.dtpFaturaTarihi.Location = new System.Drawing.Point(6, 31);
            this.dtpFaturaTarihi.Name = "dtpFaturaTarihi";
            this.dtpFaturaTarihi.Size = new System.Drawing.Size(116, 20);
            this.dtpFaturaTarihi.TabIndex = 4;
            // 
            // btnEFaturaKullaniciListesi
            // 
            this.btnEFaturaKullaniciListesi.Location = new System.Drawing.Point(137, 28);
            this.btnEFaturaKullaniciListesi.Name = "btnEFaturaKullaniciListesi";
            this.btnEFaturaKullaniciListesi.Size = new System.Drawing.Size(165, 23);
            this.btnEFaturaKullaniciListesi.TabIndex = 3;
            this.btnEFaturaKullaniciListesi.Text = "E-Fatura Kullanıcı Listesi";
            this.btnEFaturaKullaniciListesi.UseVisualStyleBackColor = true;
            this.btnEFaturaKullaniciListesi.Click += new System.EventHandler(this.btnEFaturaKullaniciListesi_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Location = new System.Drawing.Point(12, 255);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(651, 78);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "E-Müstahsil";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(479, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(165, 31);
            this.button1.TabIndex = 3;
            this.button1.Text = "Makbuz Sil";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 30);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 31);
            this.button2.TabIndex = 0;
            this.button2.Text = "XML Oluştur";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(308, 30);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(165, 31);
            this.button3.TabIndex = 2;
            this.button3.Text = "Servise Gönder";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(128, 30);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(174, 31);
            this.button4.TabIndex = 1;
            this.button4.Text = "GIB Makbuz Çıktı Önizleme";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // frmAnaSayfa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(675, 345);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmAnaSayfa";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QNB Finans GİB Servis Uygulaması";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAnaSayfa_FormClosed);
            this.Shown += new System.EventHandler(this.FormAnaSayfa_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btnEFaturaKullaniciListesi;
        private System.Windows.Forms.DateTimePicker dtpFaturaTarihi;
        private System.Windows.Forms.ListBox lbEFaturaKullaniciListesi;

        #endregion

        private System.Windows.Forms.Button btnXmlOlustur;
        private System.Windows.Forms.Button btnGIBOnizleme;
        private System.Windows.Forms.Button btnServiseGonder;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnBelgeOidKontrol;
        private System.Windows.Forms.Button btnFaturaSil;
        private System.Windows.Forms.Button btnMakbuzSil;
    }
}

