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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnXmlOlusturMustahsil = new System.Windows.Forms.Button();
            this.btnServiseGonderMustahsil = new System.Windows.Forms.Button();
            this.btnGIBOnizlemeMustahsil = new System.Windows.Forms.Button();
            this.btnBelgeOidKontrol = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnXmlOlustur
            // 
            this.btnXmlOlustur.Location = new System.Drawing.Point(6, 30);
            this.btnXmlOlustur.Name = "btnXmlOlustur";
            this.btnXmlOlustur.Size = new System.Drawing.Size(116, 58);
            this.btnXmlOlustur.TabIndex = 0;
            this.btnXmlOlustur.Text = "XML Oluştur";
            this.btnXmlOlustur.UseVisualStyleBackColor = true;
            this.btnXmlOlustur.Click += new System.EventHandler(this.btnXmlOlustur_Click);
            // 
            // btnGIBOnizleme
            // 
            this.btnGIBOnizleme.Location = new System.Drawing.Point(128, 30);
            this.btnGIBOnizleme.Name = "btnGIBOnizleme";
            this.btnGIBOnizleme.Size = new System.Drawing.Size(174, 58);
            this.btnGIBOnizleme.TabIndex = 1;
            this.btnGIBOnizleme.Text = "GIB Fatura Çıktı Önizleme";
            this.btnGIBOnizleme.UseVisualStyleBackColor = true;
            this.btnGIBOnizleme.Click += new System.EventHandler(this.btnGIBOnizleme_Click);
            // 
            // btnServiseGonder
            // 
            this.btnServiseGonder.Location = new System.Drawing.Point(308, 30);
            this.btnServiseGonder.Name = "btnServiseGonder";
            this.btnServiseGonder.Size = new System.Drawing.Size(165, 58);
            this.btnServiseGonder.TabIndex = 2;
            this.btnServiseGonder.Text = "Servise Gönder";
            this.btnServiseGonder.UseVisualStyleBackColor = true;
            this.btnServiseGonder.Click += new System.EventHandler(this.btnServiseGonder_Click);
            // 
            // groupBox1
            // 
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnXmlOlusturMustahsil);
            this.groupBox2.Controls.Add(this.btnServiseGonderMustahsil);
            this.groupBox2.Controls.Add(this.btnGIBOnizlemeMustahsil);
            this.groupBox2.Location = new System.Drawing.Point(12, 137);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(651, 119);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "E-Müstahsil";
            // 
            // btnXmlOlusturMustahsil
            // 
            this.btnXmlOlusturMustahsil.Location = new System.Drawing.Point(6, 30);
            this.btnXmlOlusturMustahsil.Name = "btnXmlOlusturMustahsil";
            this.btnXmlOlusturMustahsil.Size = new System.Drawing.Size(116, 58);
            this.btnXmlOlusturMustahsil.TabIndex = 0;
            this.btnXmlOlusturMustahsil.Text = "XML Oluştur";
            this.btnXmlOlusturMustahsil.UseVisualStyleBackColor = true;
            this.btnXmlOlusturMustahsil.Click += new System.EventHandler(this.btnXmlOlusturMustahsil_Click);
            // 
            // btnServiseGonderMustahsil
            // 
            this.btnServiseGonderMustahsil.Location = new System.Drawing.Point(308, 30);
            this.btnServiseGonderMustahsil.Name = "btnServiseGonderMustahsil";
            this.btnServiseGonderMustahsil.Size = new System.Drawing.Size(165, 58);
            this.btnServiseGonderMustahsil.TabIndex = 2;
            this.btnServiseGonderMustahsil.Text = "Servise Gönder";
            this.btnServiseGonderMustahsil.UseVisualStyleBackColor = true;
            this.btnServiseGonderMustahsil.Click += new System.EventHandler(this.btnServiseGonderMustahsil_Click);
            // 
            // btnGIBOnizlemeMustahsil
            // 
            this.btnGIBOnizlemeMustahsil.Location = new System.Drawing.Point(128, 30);
            this.btnGIBOnizlemeMustahsil.Name = "btnGIBOnizlemeMustahsil";
            this.btnGIBOnizlemeMustahsil.Size = new System.Drawing.Size(174, 58);
            this.btnGIBOnizlemeMustahsil.TabIndex = 1;
            this.btnGIBOnizlemeMustahsil.Text = "GIB Makbuz Çıktı Önizleme";
            this.btnGIBOnizlemeMustahsil.UseVisualStyleBackColor = true;
            this.btnGIBOnizlemeMustahsil.Click += new System.EventHandler(this.btnGIBOnizlemeMustahsil_Click);
            // 
            // btnBelgeOidKontrol
            // 
            this.btnBelgeOidKontrol.Location = new System.Drawing.Point(479, 30);
            this.btnBelgeOidKontrol.Name = "btnBelgeOidKontrol";
            this.btnBelgeOidKontrol.Size = new System.Drawing.Size(165, 58);
            this.btnBelgeOidKontrol.TabIndex = 3;
            this.btnBelgeOidKontrol.Text = "Belge Oid Kontrol";
            this.btnBelgeOidKontrol.UseVisualStyleBackColor = true;
            this.btnBelgeOidKontrol.Click += new System.EventHandler(this.btnBelgeOidKontrol_Click);
            // 
            // frmAnaSayfa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(675, 272);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmAnaSayfa";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QNB Finans GİB Servis Uygulaması";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmAnaSayfa_FormClosed);
            this.Shown += new System.EventHandler(this.FormAnaSayfa_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnXmlOlustur;
        private System.Windows.Forms.Button btnGIBOnizleme;
        private System.Windows.Forms.Button btnServiseGonder;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnXmlOlusturMustahsil;
        private System.Windows.Forms.Button btnServiseGonderMustahsil;
        private System.Windows.Forms.Button btnGIBOnizlemeMustahsil;
        private System.Windows.Forms.Button btnBelgeOidKontrol;
    }
}

