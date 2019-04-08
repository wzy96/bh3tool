namespace NetPackageTool
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TB_dumppath = new System.Windows.Forms.TextBox();
            this.TB_hexdumppath = new System.Windows.Forms.TextBox();
            this.btn_dodump = new System.Windows.Forms.Button();
            this.btn_dohexdump = new System.Windows.Forms.Button();
            this.btn_opendump = new System.Windows.Forms.Button();
            this.btn_openhexdump = new System.Windows.Forms.Button();
            this.TB_cmdidpath = new System.Windows.Forms.TextBox();
            this.btn_cmdidopen = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Generate cmd_id list from dump.cs";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "dump.cs Path:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(215, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "Export NetPackage from HexDump file";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "HexDump Path:";
            // 
            // TB_dumppath
            // 
            this.TB_dumppath.AllowDrop = true;
            this.TB_dumppath.Location = new System.Drawing.Point(101, 38);
            this.TB_dumppath.Name = "TB_dumppath";
            this.TB_dumppath.Size = new System.Drawing.Size(162, 21);
            this.TB_dumppath.TabIndex = 1;
            this.TB_dumppath.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_dumppath_DragDrop);
            this.TB_dumppath.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_dumppath_DragEnter);
            // 
            // TB_hexdumppath
            // 
            this.TB_hexdumppath.AllowDrop = true;
            this.TB_hexdumppath.Location = new System.Drawing.Point(101, 94);
            this.TB_hexdumppath.Name = "TB_hexdumppath";
            this.TB_hexdumppath.Size = new System.Drawing.Size(162, 21);
            this.TB_hexdumppath.TabIndex = 2;
            this.TB_hexdumppath.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_pcappath_DragDrop);
            this.TB_hexdumppath.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_pcappath_DragEnter);
            // 
            // btn_dodump
            // 
            this.btn_dodump.Location = new System.Drawing.Point(309, 36);
            this.btn_dodump.Name = "btn_dodump";
            this.btn_dodump.Size = new System.Drawing.Size(75, 23);
            this.btn_dodump.TabIndex = 3;
            this.btn_dodump.Text = "Generate";
            this.btn_dodump.UseVisualStyleBackColor = true;
            this.btn_dodump.Click += new System.EventHandler(this.btn_dodump_Click);
            // 
            // btn_dohexdump
            // 
            this.btn_dohexdump.Location = new System.Drawing.Point(309, 109);
            this.btn_dohexdump.Name = "btn_dohexdump";
            this.btn_dohexdump.Size = new System.Drawing.Size(75, 23);
            this.btn_dohexdump.TabIndex = 4;
            this.btn_dohexdump.Text = "Export";
            this.btn_dohexdump.UseVisualStyleBackColor = true;
            this.btn_dohexdump.Click += new System.EventHandler(this.btn_dohexdump_Click);
            // 
            // btn_opendump
            // 
            this.btn_opendump.Location = new System.Drawing.Point(268, 38);
            this.btn_opendump.Name = "btn_opendump";
            this.btn_opendump.Size = new System.Drawing.Size(33, 21);
            this.btn_opendump.TabIndex = 5;
            this.btn_opendump.Text = "...";
            this.btn_opendump.UseVisualStyleBackColor = true;
            this.btn_opendump.Click += new System.EventHandler(this.btn_opendump_Click);
            // 
            // btn_openhexdump
            // 
            this.btn_openhexdump.Location = new System.Drawing.Point(269, 94);
            this.btn_openhexdump.Name = "btn_openhexdump";
            this.btn_openhexdump.Size = new System.Drawing.Size(32, 21);
            this.btn_openhexdump.TabIndex = 6;
            this.btn_openhexdump.Text = "...";
            this.btn_openhexdump.UseVisualStyleBackColor = true;
            this.btn_openhexdump.Click += new System.EventHandler(this.btn_openhexdump_Click);
            // 
            // TB_cmdidpath
            // 
            this.TB_cmdidpath.AllowDrop = true;
            this.TB_cmdidpath.Location = new System.Drawing.Point(101, 128);
            this.TB_cmdidpath.Name = "TB_cmdidpath";
            this.TB_cmdidpath.Size = new System.Drawing.Size(162, 21);
            this.TB_cmdidpath.TabIndex = 2;
            this.TB_cmdidpath.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_cmdidpath_DragDrop);
            this.TB_cmdidpath.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_cmdidpath_DragEnter);
            // 
            // btn_cmdidopen
            // 
            this.btn_cmdidopen.Location = new System.Drawing.Point(269, 128);
            this.btn_cmdidopen.Name = "btn_cmdidopen";
            this.btn_cmdidopen.Size = new System.Drawing.Size(32, 21);
            this.btn_cmdidopen.TabIndex = 6;
            this.btn_cmdidopen.Text = "...";
            this.btn_cmdidopen.UseVisualStyleBackColor = true;
            this.btn_cmdidopen.Click += new System.EventHandler(this.btn_cmdidopen_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 131);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "Cmdid Path:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 161);
            this.Controls.Add(this.btn_cmdidopen);
            this.Controls.Add(this.btn_openhexdump);
            this.Controls.Add(this.btn_opendump);
            this.Controls.Add(this.btn_dohexdump);
            this.Controls.Add(this.btn_dodump);
            this.Controls.Add(this.TB_cmdidpath);
            this.Controls.Add(this.TB_hexdumppath);
            this.Controls.Add(this.TB_dumppath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "BH3NetPackageTool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TB_dumppath;
        private System.Windows.Forms.TextBox TB_hexdumppath;
        private System.Windows.Forms.Button btn_dodump;
        private System.Windows.Forms.Button btn_dohexdump;
        private System.Windows.Forms.Button btn_opendump;
        private System.Windows.Forms.Button btn_openhexdump;
        private System.Windows.Forms.TextBox TB_cmdidpath;
        private System.Windows.Forms.Button btn_cmdidopen;
        private System.Windows.Forms.Label label5;
    }
}

