using System;
using System.Windows.Forms;

namespace QNBFinansGIB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnXmlOlustur_Click(object sender, EventArgs e)
        {
            #region Klasör Seçimi

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {

                }
            }

            #endregion
        }
    }
}