namespace QNBFinansGIB
{
    partial class Form1
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
            this.SuspendLayout();
            // 
            // btnXmlOlustur
            // 
            this.btnXmlOlustur.Location = new System.Drawing.Point(37, 32);
            this.btnXmlOlustur.Name = "btnXmlOlustur";
            this.btnXmlOlustur.Size = new System.Drawing.Size(75, 23);
            this.btnXmlOlustur.TabIndex = 0;
            this.btnXmlOlustur.Text = "XML Oluştur";
            this.btnXmlOlustur.UseVisualStyleBackColor = true;
            this.btnXmlOlustur.Click += new System.EventHandler(this.btnXmlOlustur_Click);
            // 
            // btnGIBOnizleme
            // 
            this.btnGIBOnizleme.Location = new System.Drawing.Point(143, 32);
            this.btnGIBOnizleme.Name = "btnGIBOnizleme";
            this.btnGIBOnizleme.Size = new System.Drawing.Size(165, 23);
            this.btnGIBOnizleme.TabIndex = 1;
            this.btnGIBOnizleme.Text = "GIB Fatura Çıktı Önizleme";
            this.btnGIBOnizleme.UseVisualStyleBackColor = true;
            this.btnGIBOnizleme.Click += new System.EventHandler(this.btnGIBOnizleme_Click);
            // 
            // btnServiseGonder
            // 
            this.btnServiseGonder.Location = new System.Drawing.Point(331, 32);
            this.btnServiseGonder.Name = "btnServiseGonder";
            this.btnServiseGonder.Size = new System.Drawing.Size(165, 23);
            this.btnServiseGonder.TabIndex = 2;
            this.btnServiseGonder.Text = "Servise Gönder";
            this.btnServiseGonder.UseVisualStyleBackColor = true;
            this.btnServiseGonder.Click += new System.EventHandler(this.btnServiseGonder_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 87);
            this.Controls.Add(this.btnServiseGonder);
            this.Controls.Add(this.btnGIBOnizleme);
            this.Controls.Add(this.btnXmlOlustur);
            this.Name = "Form1";
            this.Text = "QNB Finans GİB Servis Uygulaması";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnXmlOlustur;
        private System.Windows.Forms.Button btnGIBOnizleme;
        private System.Windows.Forms.Button btnServiseGonder;
    }
}

