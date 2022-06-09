namespace XRF_FA
{
    partial class MessageFrm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageFrm));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlShowMessage = new System.Windows.Forms.Panel();
            this.lblMessageText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Error");
            this.imageList1.Images.SetKeyName(1, "Information");
            this.imageList1.Images.SetKeyName(2, "Question");
            this.imageList1.Images.SetKeyName(3, "Warning");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(9, 7);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // pnlShowMessage
            // 
            this.pnlShowMessage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.pnlShowMessage.BackColor = System.Drawing.Color.Transparent;
            this.pnlShowMessage.Location = new System.Drawing.Point(80, 40);
            this.pnlShowMessage.Name = "pnlShowMessage";
            this.pnlShowMessage.Size = new System.Drawing.Size(387, 33);
            this.pnlShowMessage.TabIndex = 4;
            // 
            // lblMessageText
            // 
            this.lblMessageText.BackColor = System.Drawing.Color.Transparent;
            this.lblMessageText.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessageText.Location = new System.Drawing.Point(79, 6);
            this.lblMessageText.Name = "lblMessageText";
            this.lblMessageText.Size = new System.Drawing.Size(378, 21);
            this.lblMessageText.TabIndex = 3;
            this.lblMessageText.Text = "label1";
            // 
            // MessageFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 78);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pnlShowMessage);
            this.Controls.Add(this.lblMessageText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageFrm";
            this.Text = "MessageFrm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlShowMessage;
        private System.Windows.Forms.Label lblMessageText;
    }
}