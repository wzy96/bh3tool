namespace FileDecryptTool
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
            this.TB_Apath = new System.Windows.Forms.TextBox();
            this.btn_decrypt = new System.Windows.Forms.Button();
            this.btn_Encrypt = new System.Windows.Forms.Button();
            this.btn_open1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.TB_pathX = new System.Windows.Forms.TextBox();
            this.btn_open2 = new System.Windows.Forms.Button();
            this.btn_xor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "excel_output.unity3d setting.unity3d";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "DataVersion.unity3d";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "File:";
            // 
            // TB_Apath
            // 
            this.TB_Apath.AllowDrop = true;
            this.TB_Apath.Location = new System.Drawing.Point(53, 34);
            this.TB_Apath.Name = "TB_Apath";
            this.TB_Apath.Size = new System.Drawing.Size(225, 21);
            this.TB_Apath.TabIndex = 3;
            this.TB_Apath.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_Apath_DragDrop);
            this.TB_Apath.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_Apath_DragEnter);
            // 
            // btn_decrypt
            // 
            this.btn_decrypt.Location = new System.Drawing.Point(339, 22);
            this.btn_decrypt.Name = "btn_decrypt";
            this.btn_decrypt.Size = new System.Drawing.Size(75, 23);
            this.btn_decrypt.TabIndex = 4;
            this.btn_decrypt.Text = "Decrypt";
            this.btn_decrypt.UseVisualStyleBackColor = true;
            this.btn_decrypt.Click += new System.EventHandler(this.btn_decrypt_Click);
            // 
            // btn_Encrypt
            // 
            this.btn_Encrypt.Location = new System.Drawing.Point(339, 51);
            this.btn_Encrypt.Name = "btn_Encrypt";
            this.btn_Encrypt.Size = new System.Drawing.Size(75, 23);
            this.btn_Encrypt.TabIndex = 5;
            this.btn_Encrypt.Text = "Encrypt";
            this.btn_Encrypt.UseVisualStyleBackColor = true;
            this.btn_Encrypt.Click += new System.EventHandler(this.btn_Encrypt_Click);
            // 
            // btn_open1
            // 
            this.btn_open1.Location = new System.Drawing.Point(284, 34);
            this.btn_open1.Name = "btn_open1";
            this.btn_open1.Size = new System.Drawing.Size(37, 23);
            this.btn_open1.TabIndex = 6;
            this.btn_open1.Text = "...";
            this.btn_open1.UseVisualStyleBackColor = true;
            this.btn_open1.Click += new System.EventHandler(this.btn_open1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "File:";
            // 
            // TB_pathX
            // 
            this.TB_pathX.AllowDrop = true;
            this.TB_pathX.Location = new System.Drawing.Point(53, 119);
            this.TB_pathX.Name = "TB_pathX";
            this.TB_pathX.Size = new System.Drawing.Size(225, 21);
            this.TB_pathX.TabIndex = 8;
            this.TB_pathX.DragDrop += new System.Windows.Forms.DragEventHandler(this.TB_pathX_DragDrop);
            this.TB_pathX.DragEnter += new System.Windows.Forms.DragEventHandler(this.TB_pathX_DragEnter);
            // 
            // btn_open2
            // 
            this.btn_open2.Location = new System.Drawing.Point(284, 117);
            this.btn_open2.Name = "btn_open2";
            this.btn_open2.Size = new System.Drawing.Size(37, 23);
            this.btn_open2.TabIndex = 6;
            this.btn_open2.Text = "...";
            this.btn_open2.UseVisualStyleBackColor = true;
            this.btn_open2.Click += new System.EventHandler(this.btn_open2_Click);
            // 
            // btn_xor
            // 
            this.btn_xor.Location = new System.Drawing.Point(339, 117);
            this.btn_xor.Name = "btn_xor";
            this.btn_xor.Size = new System.Drawing.Size(75, 23);
            this.btn_xor.TabIndex = 9;
            this.btn_xor.Text = "Xor";
            this.btn_xor.UseVisualStyleBackColor = true;
            this.btn_xor.Click += new System.EventHandler(this.btn_xor_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 157);
            this.Controls.Add(this.btn_xor);
            this.Controls.Add(this.TB_pathX);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btn_open2);
            this.Controls.Add(this.btn_open1);
            this.Controls.Add(this.btn_Encrypt);
            this.Controls.Add(this.btn_decrypt);
            this.Controls.Add(this.TB_Apath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "BH3Decrypt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TB_Apath;
        private System.Windows.Forms.Button btn_decrypt;
        private System.Windows.Forms.Button btn_Encrypt;
        private System.Windows.Forms.Button btn_open1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TB_pathX;
        private System.Windows.Forms.Button btn_open2;
        private System.Windows.Forms.Button btn_xor;
    }
}

