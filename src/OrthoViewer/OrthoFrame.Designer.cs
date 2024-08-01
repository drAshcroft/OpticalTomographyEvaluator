namespace OrthoViewer
{
    partial class OrthoFrame
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
            this.lbStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // lbStatus
            // 
            this.lbStatus.ForeColor = System.Drawing.Color.White;
            this.lbStatus.Location = new System.Drawing.Point(838, 9);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(558, 55);
            this.lbStatus.TabIndex = 4;
            this.lbStatus.Text = "label1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1315, 436);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button2_Click);
            // 
            // zedGraphControl1
            // 
            this.zedGraphControl1.IsEnableHEdit = true;
            this.zedGraphControl1.IsEnableVEdit = true;
            this.zedGraphControl1.IsEnableWheelZoom = false;
            this.zedGraphControl1.Location = new System.Drawing.Point(841, 58);
            this.zedGraphControl1.Name = "zedGraphControl1";
            this.zedGraphControl1.ScrollGrace = 0D;
            this.zedGraphControl1.ScrollMaxX = 0D;
            this.zedGraphControl1.ScrollMaxY = 0D;
            this.zedGraphControl1.ScrollMaxY2 = 0D;
            this.zedGraphControl1.ScrollMinX = 0D;
            this.zedGraphControl1.ScrollMinY = 0D;
            this.zedGraphControl1.ScrollMinY2 = 0D;
            this.zedGraphControl1.Size = new System.Drawing.Size(556, 287);
            this.zedGraphControl1.TabIndex = 6;
            this.zedGraphControl1.PointEditEvent += new ZedGraph.ZedGraphControl.PointEditHandler(this.zedGraphControl1_PointEditEvent);
            this.zedGraphControl1.AddPointEditEvent += new ZedGraph.ZedGraphControl.AddPointEditHandler(this.zedGraphControl1_AddPointEditEvent);
            this.zedGraphControl1.Load += new System.EventHandler(this.zedGraphControl1_Load);
            // 
            // OrthoFrame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1409, 762);
            this.Controls.Add(this.zedGraphControl1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lbStatus);
            this.Name = "OrthoFrame";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OrthoFrame_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Button button1;
        private ZedGraph.ZedGraphControl zedGraphControl1;
    }
}

