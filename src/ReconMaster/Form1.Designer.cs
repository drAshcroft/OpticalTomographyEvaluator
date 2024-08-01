namespace ReconMaster
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
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tbWatchFolder2 = new System.Windows.Forms.TextBox();
            this.tbWatchFolder1 = new System.Windows.Forms.TextBox();
            this.bWatch = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.bBrowse1 = new System.Windows.Forms.Button();
            this.bBrowse2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nProcesses = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbShellScript = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.rtbFolders = new System.Windows.Forms.RichTextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nProcesses)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 23);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(134, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start Folder Processing";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.RunEverything_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 68);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(134, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Pause";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbWatchFolder2
            // 
            this.tbWatchFolder2.Location = new System.Drawing.Point(13, 71);
            this.tbWatchFolder2.Name = "tbWatchFolder2";
            this.tbWatchFolder2.Size = new System.Drawing.Size(180, 20);
            this.tbWatchFolder2.TabIndex = 2;
            this.tbWatchFolder2.Text = "X:\\Rejected_Datasets";
            // 
            // tbWatchFolder1
            // 
            this.tbWatchFolder1.Location = new System.Drawing.Point(13, 32);
            this.tbWatchFolder1.Name = "tbWatchFolder1";
            this.tbWatchFolder1.Size = new System.Drawing.Size(180, 20);
            this.tbWatchFolder1.TabIndex = 3;
            this.tbWatchFolder1.Text = "w:\\";
            // 
            // bWatch
            // 
            this.bWatch.Location = new System.Drawing.Point(136, 108);
            this.bWatch.Name = "bWatch";
            this.bWatch.Size = new System.Drawing.Size(147, 23);
            this.bWatch.TabIndex = 4;
            this.bWatch.Text = "Start Watching";
            this.bWatch.UseVisualStyleBackColor = true;
            this.bWatch.Click += new System.EventHandler(this.bWatch_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Watch Folder 2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Watch Folder 1";
            // 
            // bBrowse1
            // 
            this.bBrowse1.Location = new System.Drawing.Point(208, 29);
            this.bBrowse1.Name = "bBrowse1";
            this.bBrowse1.Size = new System.Drawing.Size(75, 23);
            this.bBrowse1.TabIndex = 7;
            this.bBrowse1.Text = "Browse";
            this.bBrowse1.UseVisualStyleBackColor = true;
            this.bBrowse1.Click += new System.EventHandler(this.bBrowse1_Click);
            // 
            // bBrowse2
            // 
            this.bBrowse2.Location = new System.Drawing.Point(208, 68);
            this.bBrowse2.Name = "bBrowse2";
            this.bBrowse2.Size = new System.Drawing.Size(75, 23);
            this.bBrowse2.TabIndex = 8;
            this.bBrowse2.Text = "Browse";
            this.bBrowse2.UseVisualStyleBackColor = true;
            this.bBrowse2.Click += new System.EventHandler(this.bBrowse2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nProcesses);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 148);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Folder Processing";
            // 
            // nProcesses
            // 
            this.nProcesses.Location = new System.Drawing.Point(12, 122);
            this.nProcesses.Name = "nProcesses";
            this.nProcesses.Size = new System.Drawing.Size(120, 20);
            this.nProcesses.TabIndex = 3;
            this.nProcesses.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Number Processes";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 138);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(134, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Review Processing";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Review_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bWatch);
            this.groupBox2.Controls.Add(this.tbWatchFolder2);
            this.groupBox2.Controls.Add(this.bBrowse2);
            this.groupBox2.Controls.Add(this.tbWatchFolder1);
            this.groupBox2.Controls.Add(this.bBrowse1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(219, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(293, 148);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Networks";
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.tbShellScript);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(219, 171);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(288, 119);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Run Script";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(141, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "DOS rules, must be one line.";
            // 
            // tbShellScript
            // 
            this.tbShellScript.Location = new System.Drawing.Point(8, 55);
            this.tbShellScript.Name = "tbShellScript";
            this.tbShellScript.Size = new System.Drawing.Size(274, 20);
            this.tbShellScript.TabIndex = 1;
            this.tbShellScript.Text = "run(\'c:\\\\matlab\\\\DoReconstruction.m\')";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Shell Script";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button3);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.rtbFolders);
            this.groupBox4.Location = new System.Drawing.Point(0, 158);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(198, 173);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Reruns";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Folders";
            // 
            // rtbFolders
            // 
            this.rtbFolders.Location = new System.Drawing.Point(6, 36);
            this.rtbFolders.Name = "rtbFolders";
            this.rtbFolders.Size = new System.Drawing.Size(177, 96);
            this.rtbFolders.TabIndex = 0;
            this.rtbFolders.Text = "";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(416, 303);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 12;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 338);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Recon Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nProcesses)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbWatchFolder2;
        private System.Windows.Forms.TextBox tbWatchFolder1;
        private System.Windows.Forms.Button bWatch;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bBrowse1;
        private System.Windows.Forms.Button bBrowse2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.NumericUpDown nProcesses;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox rtbFolders;
        private System.Windows.Forms.TextBox tbShellScript;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button4;
    }
}

