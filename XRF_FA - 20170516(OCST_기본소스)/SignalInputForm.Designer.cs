namespace XRF_FA
{
    partial class SignalInputForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblApplication = new System.Windows.Forms.Label();
            this.txtApplication = new System.Windows.Forms.TextBox();
            this.rb3 = new System.Windows.Forms.RadioButton();
            this.rb2 = new System.Windows.Forms.RadioButton();
            this.rb1 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.cmbData = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbTag = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.msgLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.rb4 = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.rb4);
            this.panel1.Controls.Add(this.lblApplication);
            this.panel1.Controls.Add(this.txtApplication);
            this.panel1.Controls.Add(this.rb3);
            this.panel1.Controls.Add(this.rb2);
            this.panel1.Controls.Add(this.rb1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.btnSend);
            this.panel1.Controls.Add(this.txtData);
            this.panel1.Controls.Add(this.cmbData);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.cmbTag);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(454, 259);
            this.panel1.TabIndex = 6;
            // 
            // lblApplication
            // 
            this.lblApplication.AutoSize = true;
            this.lblApplication.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblApplication.Location = new System.Drawing.Point(18, 161);
            this.lblApplication.Name = "lblApplication";
            this.lblApplication.Size = new System.Drawing.Size(75, 13);
            this.lblApplication.TabIndex = 17;
            this.lblApplication.Text = "application";
            // 
            // txtApplication
            // 
            this.txtApplication.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtApplication.Location = new System.Drawing.Point(99, 158);
            this.txtApplication.Name = "txtApplication";
            this.txtApplication.Size = new System.Drawing.Size(233, 22);
            this.txtApplication.TabIndex = 16;
            // 
            // rb3
            // 
            this.rb3.AutoSize = true;
            this.rb3.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rb3.Location = new System.Drawing.Point(231, 30);
            this.rb3.Name = "rb3";
            this.rb3.Size = new System.Drawing.Size(97, 16);
            this.rb3.TabIndex = 15;
            this.rb3.TabStop = true;
            this.rb3.Text = "FA 동작신호";
            this.rb3.UseVisualStyleBackColor = true;
            this.rb3.CheckedChanged += new System.EventHandler(this.rb1_CheckedChanged);
            // 
            // rb2
            // 
            this.rb2.AutoSize = true;
            this.rb2.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rb2.Location = new System.Drawing.Point(148, 30);
            this.rb2.Name = "rb2";
            this.rb2.Size = new System.Drawing.Size(75, 16);
            this.rb2.TabIndex = 14;
            this.rb2.TabStop = true;
            this.rb2.Text = "버퍼정보";
            this.rb2.UseVisualStyleBackColor = true;
            this.rb2.CheckedChanged += new System.EventHandler(this.rb1_CheckedChanged);
            // 
            // rb1
            // 
            this.rb1.AutoSize = true;
            this.rb1.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rb1.Location = new System.Drawing.Point(15, 30);
            this.rb1.Name = "rb1";
            this.rb1.Size = new System.Drawing.Size(123, 16);
            this.rb1.TabIndex = 13;
            this.rb1.TabStop = true;
            this.rb1.Text = "FA설비 위치정보";
            this.rb1.UseVisualStyleBackColor = true;
            this.rb1.CheckedChanged += new System.EventHandler(this.rb1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(159, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 37);
            this.button1.TabIndex = 12;
            this.button1.Text = "닫기";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnSend.Location = new System.Drawing.Point(343, 129);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(78, 31);
            this.btnSend.TabIndex = 11;
            this.btnSend.Text = "송신";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtData
            // 
            this.txtData.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtData.Location = new System.Drawing.Point(99, 128);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(233, 22);
            this.txtData.TabIndex = 10;
            // 
            // cmbData
            // 
            this.cmbData.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cmbData.FormattingEnabled = true;
            this.cmbData.Location = new System.Drawing.Point(96, 124);
            this.cmbData.Name = "cmbData";
            this.cmbData.Size = new System.Drawing.Size(233, 21);
            this.cmbData.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(16, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "송신 Data";
            // 
            // cmbTag
            // 
            this.cmbTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTag.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.cmbTag.FormattingEnabled = true;
            this.cmbTag.Location = new System.Drawing.Point(99, 89);
            this.cmbTag.Name = "cmbTag";
            this.cmbTag.Size = new System.Drawing.Size(318, 21);
            this.cmbTag.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Gulim", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(33, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "TAG 명";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.msgLabel,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 237);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(454, 22);
            this.statusStrip1.TabIndex = 7;
            // 
            // msgLabel
            // 
            this.msgLabel.Name = "msgLabel";
            this.msgLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(199, 17);
            this.toolStripStatusLabel1.Text = "수동으로 전송할 정보를 선택하세요";
            // 
            // rb4
            // 
            this.rb4.AutoSize = true;
            this.rb4.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.rb4.Location = new System.Drawing.Point(334, 30);
            this.rb4.Name = "rb4";
            this.rb4.Size = new System.Drawing.Size(96, 16);
            this.rb4.TabIndex = 18;
            this.rb4.TabStop = true;
            this.rb4.Text = "X2 동작신호";
            this.rb4.UseVisualStyleBackColor = true;
            // 
            // SignalInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 259);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Name = "SignalInputForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "수동 Data 송신";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.ComboBox cmbData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbTag;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ToolStripStatusLabel msgLabel;
        private System.Windows.Forms.RadioButton rb1;
        private System.Windows.Forms.RadioButton rb3;
        private System.Windows.Forms.RadioButton rb2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label lblApplication;
        private System.Windows.Forms.TextBox txtApplication;
        private System.Windows.Forms.RadioButton rb4;
    }
}