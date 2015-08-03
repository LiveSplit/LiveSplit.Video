namespace LiveSplit.Video
{
    partial class VideoSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoSettings));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.txtVideoPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.trkHeightWidth = new System.Windows.Forms.TrackBar();
            this.lblHeightWidth = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkHeightWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSelectFile, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtVideoPath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtOffset, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.trkHeightWidth, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblHeightWidth, 0, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // btnSelectFile
            // 
            resources.ApplyResources(this.btnSelectFile, "btnSelectFile");
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // txtVideoPath
            // 
            resources.ApplyResources(this.txtVideoPath, "txtVideoPath");
            this.txtVideoPath.Name = "txtVideoPath";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // txtOffset
            // 
            resources.ApplyResources(this.txtOffset, "txtOffset");
            this.tableLayoutPanel1.SetColumnSpan(this.txtOffset, 2);
            this.txtOffset.Name = "txtOffset";
            // 
            // trkHeightWidth
            // 
            resources.ApplyResources(this.trkHeightWidth, "trkHeightWidth");
            this.tableLayoutPanel1.SetColumnSpan(this.trkHeightWidth, 2);
            this.trkHeightWidth.Name = "trkHeightWidth";
            this.trkHeightWidth.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // lblHeightWidth
            // 
            resources.ApplyResources(this.lblHeightWidth, "lblHeightWidth");
            this.lblHeightWidth.Name = "lblHeightWidth";
            // 
            // VideoSettings
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VideoSettings";
            this.Load += new System.EventHandler(this.VideoSettings_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkHeightWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOffset;
        public System.Windows.Forms.TextBox txtVideoPath;
        private System.Windows.Forms.TrackBar trkHeightWidth;
        private System.Windows.Forms.Label lblHeightWidth;
    }
}
