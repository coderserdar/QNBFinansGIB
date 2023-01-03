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
            this.gbEFaturaEArsiv = new System.Windows.Forms.GroupBox();
            this.btnFaturaSil = new System.Windows.Forms.Button();
            this.btnBelgeOidKontrol = new System.Windows.Forms.Button();
            this.gbEFatura = new System.Windows.Forms.GroupBox();
            this.btnEFaturaKullaniciListesiTemizle = new System.Windows.Forms.Button();
            this.btnEFaturaKullaniciListeAktar = new System.Windows.Forms.Button();
            this.lblMukellefListesi = new System.Windows.Forms.Label();
            this.lbEFaturaKullaniciListesi = new System.Windows.Forms.ListBox();
            this.dtpFaturaTarihi = new System.Windows.Forms.DateTimePicker();
            this.btnEFaturaKullaniciListesi = new System.Windows.Forms.Button();
            this.gbEMustahsil = new System.Windows.Forms.GroupBox();
            this.btnMakbuzSil = new System.Windows.Forms.Button();
            this.btnXmlOlusturMustahsil = new System.Windows.Forms.Button();
            this.btnServiseGonderMustahsil = new System.Windows.Forms.Button();
            this.btnGIBOnizlemeMustahsil = new System.Windows.Forms.Button();
            this.gbEFaturaEArsiv.SuspendLayout();
            this.gbEFatura.SuspendLayout();
            this.gbEMustahsil.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnXmlOlustur
            // 
            this.btnXmlOlustur.Location = new System.Drawing.Point(8, 37);
            this.btnXmlOlustur.Margin = new System.Windows.Forms.Padding(4);
            this.btnXmlOlustur.Name = "btnXmlOlustur";
            this.btnXmlOlustur.Size = new System.Drawing.Size(155, 38);
            this.btnXmlOlustur.TabIndex = 0;
            this.btnXmlOlustur.Text = "XML Oluştur";
            this.btnXmlOlustur.UseVisualStyleBackColor = true;
            this.btnXmlOlustur.Click += new System.EventHandler(this.btnXmlOlustur_Click);
            // 
            // btnGIBOnizleme
            // 
            this.btnGIBOnizleme.Location = new System.Drawing.Point(171, 37);
            this.btnGIBOnizleme.Margin = new System.Windows.Forms.Padding(4);
            this.btnGIBOnizleme.Name = "btnGIBOnizleme";
            this.btnGIBOnizleme.Size = new System.Drawing.Size(232, 38);
            this.btnGIBOnizleme.TabIndex = 1;
            this.btnGIBOnizleme.Text = "GIB Fatura Çıktı Önizleme";
            this.btnGIBOnizleme.UseVisualStyleBackColor = true;
            this.btnGIBOnizleme.Click += new System.EventHandler(this.btnGIBOnizleme_Click);
            // 
            // btnServiseGonder
            // 
            this.btnServiseGonder.Location = new System.Drawing.Point(411, 37);
            this.btnServiseGonder.Margin = new System.Windows.Forms.Padding(4);
            this.btnServiseGonder.Name = "btnServiseGonder";
            this.btnServiseGonder.Size = new System.Drawing.Size(220, 38);
            this.btnServiseGonder.TabIndex = 2;
            this.btnServiseGonder.Text = "Servise Gönder";
            this.btnServiseGonder.UseVisualStyleBackColor = true;
            this.btnServiseGonder.Click += new System.EventHandler(this.btnServiseGonder_Click);
            // 
            // gbEFaturaEArsiv
            // 
            this.gbEFaturaEArsiv.Controls.Add(this.btnFaturaSil);
            this.gbEFaturaEArsiv.Controls.Add(this.btnBelgeOidKontrol);
            this.gbEFaturaEArsiv.Controls.Add(this.btnXmlOlustur);
            this.gbEFaturaEArsiv.Controls.Add(this.btnServiseGonder);
            this.gbEFaturaEArsiv.Controls.Add(this.btnGIBOnizleme);
            this.gbEFaturaEArsiv.Location = new System.Drawing.Point(16, 15);
            this.gbEFaturaEArsiv.Margin = new System.Windows.Forms.Padding(4);
            this.gbEFaturaEArsiv.Name = "gbEFaturaEArsiv";
            this.gbEFaturaEArsiv.Padding = new System.Windows.Forms.Padding(4);
            this.gbEFaturaEArsiv.Size = new System.Drawing.Size(868, 146);
            this.gbEFaturaEArsiv.TabIndex = 3;
            this.gbEFaturaEArsiv.TabStop = false;
            this.gbEFaturaEArsiv.Text = "E-Fatura ve E-Arşiv";
            // 
            // btnFaturaSil
            // 
            this.btnFaturaSil.Location = new System.Drawing.Point(297, 89);
            this.btnFaturaSil.Margin = new System.Windows.Forms.Padding(4);
            this.btnFaturaSil.Name = "btnFaturaSil";
            this.btnFaturaSil.Size = new System.Drawing.Size(220, 38);
            this.btnFaturaSil.TabIndex = 4;
            this.btnFaturaSil.Text = "Fatura Sil";
            this.btnFaturaSil.UseVisualStyleBackColor = true;
            this.btnFaturaSil.Click += new System.EventHandler(this.btnFaturaSil_Click);
            // 
            // btnBelgeOidKontrol
            // 
            this.btnBelgeOidKontrol.Location = new System.Drawing.Point(639, 37);
            this.btnBelgeOidKontrol.Margin = new System.Windows.Forms.Padding(4);
            this.btnBelgeOidKontrol.Name = "btnBelgeOidKontrol";
            this.btnBelgeOidKontrol.Size = new System.Drawing.Size(220, 38);
            this.btnBelgeOidKontrol.TabIndex = 3;
            this.btnBelgeOidKontrol.Text = "Belge Oid Kontrol";
            this.btnBelgeOidKontrol.UseVisualStyleBackColor = true;
            this.btnBelgeOidKontrol.Click += new System.EventHandler(this.btnBelgeOidKontrol_Click);
            // 
            // gbEFatura
            // 
            this.gbEFatura.Controls.Add(this.btnEFaturaKullaniciListesiTemizle);
            this.gbEFatura.Controls.Add(this.btnEFaturaKullaniciListeAktar);
            this.gbEFatura.Controls.Add(this.lblMukellefListesi);
            this.gbEFatura.Controls.Add(this.lbEFaturaKullaniciListesi);
            this.gbEFatura.Controls.Add(this.dtpFaturaTarihi);
            this.gbEFatura.Controls.Add(this.btnEFaturaKullaniciListesi);
            this.gbEFatura.Location = new System.Drawing.Point(16, 149);
            this.gbEFatura.Margin = new System.Windows.Forms.Padding(4);
            this.gbEFatura.Name = "gbEFatura";
            this.gbEFatura.Padding = new System.Windows.Forms.Padding(4);
            this.gbEFatura.Size = new System.Drawing.Size(868, 158);
            this.gbEFatura.TabIndex = 4;
            this.gbEFatura.TabStop = false;
            this.gbEFatura.Text = "E-Fatura";
            // 
            // btnEFaturaKullaniciListesiTemizle
            // 
            this.btnEFaturaKullaniciListesiTemizle.Location = new System.Drawing.Point(8, 122);
            this.btnEFaturaKullaniciListesiTemizle.Margin = new System.Windows.Forms.Padding(4);
            this.btnEFaturaKullaniciListesiTemizle.Name = "btnEFaturaKullaniciListesiTemizle";
            this.btnEFaturaKullaniciListesiTemizle.Size = new System.Drawing.Size(155, 28);
            this.btnEFaturaKullaniciListesiTemizle.TabIndex = 8;
            this.btnEFaturaKullaniciListesiTemizle.Text = "Listeyi Temizle";
            this.btnEFaturaKullaniciListesiTemizle.UseVisualStyleBackColor = true;
            this.btnEFaturaKullaniciListesiTemizle.Click += new System.EventHandler(this.btnEFaturaKullaniciListesiTemizle_Click);
            // 
            // btnEFaturaKullaniciListeAktar
            // 
            this.btnEFaturaKullaniciListeAktar.Location = new System.Drawing.Point(183, 122);
            this.btnEFaturaKullaniciListeAktar.Margin = new System.Windows.Forms.Padding(4);
            this.btnEFaturaKullaniciListeAktar.Name = "btnEFaturaKullaniciListeAktar";
            this.btnEFaturaKullaniciListeAktar.Size = new System.Drawing.Size(220, 28);
            this.btnEFaturaKullaniciListeAktar.TabIndex = 7;
            this.btnEFaturaKullaniciListeAktar.Text = "Listeyi Dışarıya Aktar";
            this.btnEFaturaKullaniciListeAktar.UseVisualStyleBackColor = true;
            this.btnEFaturaKullaniciListeAktar.Click += new System.EventHandler(this.btnEFaturaKullaniciListeAktar_Click);
            // 
            // lblMukellefListesi
            // 
            this.lblMukellefListesi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (162)));
            this.lblMukellefListesi.ForeColor = System.Drawing.Color.Red;
            this.lblMukellefListesi.Location = new System.Drawing.Point(8, 75);
            this.lblMukellefListesi.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMukellefListesi.Name = "lblMukellefListesi";
            this.lblMukellefListesi.Size = new System.Drawing.Size(397, 28);
            this.lblMukellefListesi.TabIndex = 6;
            this.lblMukellefListesi.Text = "Seçilen Tarihten Sonra E-Fatura Mükellefi Olan Kullanıcılar =>";
            this.lblMukellefListesi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbEFaturaKullaniciListesi
            // 
            this.lbEFaturaKullaniciListesi.FormattingEnabled = true;
            this.lbEFaturaKullaniciListesi.ItemHeight = 16;
            this.lbEFaturaKullaniciListesi.Location = new System.Drawing.Point(425, 34);
            this.lbEFaturaKullaniciListesi.Margin = new System.Windows.Forms.Padding(4);
            this.lbEFaturaKullaniciListesi.Name = "lbEFaturaKullaniciListesi";
            this.lbEFaturaKullaniciListesi.Size = new System.Drawing.Size(432, 116);
            this.lbEFaturaKullaniciListesi.TabIndex = 5;
            // 
            // dtpFaturaTarihi
            // 
            this.dtpFaturaTarihi.Location = new System.Drawing.Point(8, 38);
            this.dtpFaturaTarihi.Margin = new System.Windows.Forms.Padding(4);
            this.dtpFaturaTarihi.Name = "dtpFaturaTarihi";
            this.dtpFaturaTarihi.Size = new System.Drawing.Size(153, 22);
            this.dtpFaturaTarihi.TabIndex = 4;
            // 
            // btnEFaturaKullaniciListesi
            // 
            this.btnEFaturaKullaniciListesi.Location = new System.Drawing.Point(183, 34);
            this.btnEFaturaKullaniciListesi.Margin = new System.Windows.Forms.Padding(4);
            this.btnEFaturaKullaniciListesi.Name = "btnEFaturaKullaniciListesi";
            this.btnEFaturaKullaniciListesi.Size = new System.Drawing.Size(220, 28);
            this.btnEFaturaKullaniciListesi.TabIndex = 3;
            this.btnEFaturaKullaniciListesi.Text = "E-Fatura Kullanıcı Listesi";
            this.btnEFaturaKullaniciListesi.UseVisualStyleBackColor = true;
            this.btnEFaturaKullaniciListesi.Click += new System.EventHandler(this.btnEFaturaKullaniciListesi_Click);
            // 
            // gbEMustahsil
            // 
            this.gbEMustahsil.Controls.Add(this.btnMakbuzSil);
            this.gbEMustahsil.Controls.Add(this.btnXmlOlusturMustahsil);
            this.gbEMustahsil.Controls.Add(this.btnServiseGonderMustahsil);
            this.gbEMustahsil.Controls.Add(this.btnGIBOnizlemeMustahsil);
            this.gbEMustahsil.Location = new System.Drawing.Point(16, 314);
            this.gbEMustahsil.Margin = new System.Windows.Forms.Padding(4);
            this.gbEMustahsil.Name = "gbEMustahsil";
            this.gbEMustahsil.Padding = new System.Windows.Forms.Padding(4);
            this.gbEMustahsil.Size = new System.Drawing.Size(868, 96);
            this.gbEMustahsil.TabIndex = 5;
            this.gbEMustahsil.TabStop = false;
            this.gbEMustahsil.Text = "E-Müstahsil";
            // 
            // btnMakbuzSil
            // 
            this.btnMakbuzSil.Location = new System.Drawing.Point(639, 37);
            this.btnMakbuzSil.Margin = new System.Windows.Forms.Padding(4);
            this.btnMakbuzSil.Name = "btnMakbuzSil";
            this.btnMakbuzSil.Size = new System.Drawing.Size(220, 38);
            this.btnMakbuzSil.TabIndex = 3;
            this.btnMakbuzSil.Text = "Makbuz Sil";
            this.btnMakbuzSil.UseVisualStyleBackColor = true;
            this.btnMakbuzSil.Click += new System.EventHandler(this.btnMakbuzSil_Click);
            // 
            // btnXmlOlusturMustahsil
            // 
            this.btnXmlOlusturMustahsil.Location = new System.Drawing.Point(8, 37);
            this.btnXmlOlusturMustahsil.Margin = new System.Windows.Forms.Padding(4);
            this.btnXmlOlusturMustahsil.Name = "btnXmlOlusturMustahsil";
            this.btnXmlOlusturMustahsil.Size = new System.Drawing.Size(155, 38);
            this.btnXmlOlusturMustahsil.TabIndex = 0;
            this.btnXmlOlusturMustahsil.Text = "XML Oluştur";
            this.btnXmlOlusturMustahsil.UseVisualStyleBackColor = true;
            this.btnXmlOlusturMustahsil.Click += new System.EventHandler(this.btnXmlOlusturMustahsil_Click);
            // 
            // btnServiseGonderMustahsil
            // 
            this.btnServiseGonderMustahsil.Location = new System.Drawing.Point(411, 37);
            this.btnServiseGonderMustahsil.Margin = new System.Windows.Forms.Padding(4);
            this.btnServiseGonderMustahsil.Name = "btnServiseGonderMustahsil";
            this.btnServiseGonderMustahsil.Size = new System.Drawing.Size(220, 38);
            this.btnServiseGonderMustahsil.TabIndex = 2;
            this.btnServiseGonderMustahsil.Text = "Servise Gönder";
            this.btnServiseGonderMustahsil.UseVisualStyleBackColor = true;
            this.btnServiseGonderMustahsil.Click += new System.EventHandler(this.btnServiseGonderMustahsil_Click);
            // 
            // btnGIBOnizlemeMustahsil
            // 
            this.btnGIBOnizlemeMustahsil.Location = new System.Drawing.Point(171, 37);
            this.btnGIBOnizlemeMustahsil.Margin = new System.Windows.Forms.Padding(4);
            this.btnGIBOnizlemeMustahsil.Name = "btnGIBOnizlemeMustahsil";
            this.btnGIBOnizlemeMustahsil.Size = new System.Drawing.Size(232, 38);
            this.btnGIBOnizlemeMustahsil.TabIndex = 1;
            this.btnGIBOnizlemeMustahsil.Text = "GIB Makbuz Çıktı Önizleme";
            this.btnGIBOnizlemeMustahsil.UseVisualStyleBackColor = true;
            this.btnGIBOnizlemeMustahsil.Click += new System.EventHandler(this.btnGIBOnizlemeMustahsil_Click);
            // 
            // frmAnaSayfa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(900, 425);
            this.Controls.Add(this.gbEMustahsil);
            this.Controls.Add(this.gbEFatura);
            this.Controls.Add(this.gbEFaturaEArsiv);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmAnaSayfa";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QNB Finans GİB Servis Uygulaması";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAnaSayfa_FormClosed);
            this.Shown += new System.EventHandler(this.FormAnaSayfa_Shown);
            this.gbEFaturaEArsiv.ResumeLayout(false);
            this.gbEFatura.ResumeLayout(false);
            this.gbEMustahsil.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnServiseGonderMustahsil;

        private System.Windows.Forms.Button btnEFaturaKullaniciListesiTemizle;

        private System.Windows.Forms.Button btnEFaturaKullaniciListeAktar;

        private System.Windows.Forms.Label lblMukellefListesi;

        private System.Windows.Forms.GroupBox gbEMustahsil;
        private System.Windows.Forms.Button btnMakbuzSil;
        private System.Windows.Forms.Button btnXmlOlusturMustahsil;
        private System.Windows.Forms.Button btnGIBOnizlemeMustahsil;
        private System.Windows.Forms.Button btnEFaturaKullaniciListesi;
        private System.Windows.Forms.DateTimePicker dtpFaturaTarihi;
        private System.Windows.Forms.ListBox lbEFaturaKullaniciListesi;

        #endregion

        private System.Windows.Forms.Button btnXmlOlustur;
        private System.Windows.Forms.Button btnGIBOnizleme;
        private System.Windows.Forms.Button btnServiseGonder;
        private System.Windows.Forms.GroupBox gbEFaturaEArsiv;
        private System.Windows.Forms.GroupBox gbEFatura;
        private System.Windows.Forms.Button btnBelgeOidKontrol;
        private System.Windows.Forms.Button btnFaturaSil;
    }
}

