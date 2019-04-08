using System;
using System.Windows.Forms;

namespace FileDecryptTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_decrypt_Click(object sender, EventArgs e)
        {
            FileDecryptTool.AB_Decrypt(TB_Apath.Text);
        }

        private void btn_Encrypt_Click(object sender, EventArgs e)
        {
            FileDecryptTool.AB_Encrypt(TB_Apath.Text);
        }

        private void btn_xor_Click(object sender, EventArgs e)
        {
            FileDecryptTool.Data_xor(TB_pathX.Text);
        }

        private void btn_open1_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_Apath.Text = fileDialog.FileName;
            }

        }

        private void btn_open2_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_pathX.Text = fileDialog.FileName;
            }
        }

        private void TB_Apath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TB_Apath_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            TB_Apath.Text = path;
        }

        private void TB_pathX_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TB_pathX_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            TB_pathX.Text = path;
        }
    }
}
