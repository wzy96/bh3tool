using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetPackageTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_opendump_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_dumppath.Text = fileDialog.FileName;
            }
        }

        private void btn_openhexdump_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_hexdumppath.Text = fileDialog.FileName;
            }
        }

        private void btn_cmdidopen_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                TB_cmdidpath.Text = fileDialog.FileName;
            }
        }

        private void TB_dumppath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TB_dumppath_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            TB_dumppath.Text = path;
        }

        private void TB_pcappath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TB_pcappath_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            TB_hexdumppath.Text = path;
        }

        private void TB_cmdidpath_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TB_cmdidpath_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            TB_cmdidpath.Text = path;
        }

        private void btn_dodump_Click(object sender, EventArgs e)
        {
            CmdidList.SaveListToFile(TB_dumppath.Text);
        }

        private void btn_dohexdump_Click(object sender, EventArgs e)
        {
            NetPacket.ExportFromHexDump(TB_hexdumppath.Text, TB_cmdidpath.Text);
        }


    }
}
