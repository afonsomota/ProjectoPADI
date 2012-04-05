namespace PuppetMaster
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
            this.label1 = new System.Windows.Forms.Label();
            this.listCliOnline = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listCliOffline = new System.Windows.Forms.ListBox();
            this.startCliButton = new System.Windows.Forms.Button();
            this.stopCliButton = new System.Windows.Forms.Button();
            this.stopServButton = new System.Windows.Forms.Button();
            this.startServButton = new System.Windows.Forms.Button();
            this.listServOffline = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listServOnline = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(267, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Clients Offline";
            // 
            // listCliOnline
            // 
            this.listCliOnline.FormattingEnabled = true;
            this.listCliOnline.Location = new System.Drawing.Point(270, 30);
            this.listCliOnline.Name = "listCliOnline";
            this.listCliOnline.Size = new System.Drawing.Size(116, 82);
            this.listCliOnline.TabIndex = 1;
            this.listCliOnline.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(267, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Clients Online";
            // 
            // listCliOffline
            // 
            this.listCliOffline.FormattingEnabled = true;
            this.listCliOffline.Location = new System.Drawing.Point(270, 143);
            this.listCliOffline.Name = "listCliOffline";
            this.listCliOffline.Size = new System.Drawing.Size(116, 82);
            this.listCliOffline.TabIndex = 3;
            // 
            // startCliButton
            // 
            this.startCliButton.Location = new System.Drawing.Point(393, 30);
            this.startCliButton.Name = "startCliButton";
            this.startCliButton.Size = new System.Drawing.Size(40, 82);
            this.startCliButton.TabIndex = 4;
            this.startCliButton.Text = "Start";
            this.startCliButton.UseVisualStyleBackColor = true;
            this.startCliButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopCliButton
            // 
            this.stopCliButton.Location = new System.Drawing.Point(393, 143);
            this.stopCliButton.Name = "stopCliButton";
            this.stopCliButton.Size = new System.Drawing.Size(40, 82);
            this.stopCliButton.TabIndex = 5;
            this.stopCliButton.Text = "Stop";
            this.stopCliButton.UseVisualStyleBackColor = true;
            this.stopCliButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // stopServButton
            // 
            this.stopServButton.Location = new System.Drawing.Point(591, 143);
            this.stopServButton.Name = "stopServButton";
            this.stopServButton.Size = new System.Drawing.Size(40, 82);
            this.stopServButton.TabIndex = 11;
            this.stopServButton.Text = "Stop";
            this.stopServButton.UseVisualStyleBackColor = true;
            this.stopServButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // startServButton
            // 
            this.startServButton.Location = new System.Drawing.Point(591, 30);
            this.startServButton.Name = "startServButton";
            this.startServButton.Size = new System.Drawing.Size(40, 82);
            this.startServButton.TabIndex = 10;
            this.startServButton.Text = "Start";
            this.startServButton.UseVisualStyleBackColor = true;
            this.startServButton.Click += new System.EventHandler(this.button4_Click);
            // 
            // listServOffline
            // 
            this.listServOffline.FormattingEnabled = true;
            this.listServOffline.Location = new System.Drawing.Point(468, 143);
            this.listServOffline.Name = "listServOffline";
            this.listServOffline.Size = new System.Drawing.Size(116, 82);
            this.listServOffline.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(465, 127);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Servers Online";
            // 
            // listServOnline
            // 
            this.listServOnline.FormattingEnabled = true;
            this.listServOnline.Location = new System.Drawing.Point(468, 30);
            this.listServOnline.Name = "listServOnline";
            this.listServOnline.Size = new System.Drawing.Size(116, 82);
            this.listServOnline.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(465, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Servers Offline";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(132, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 35);
            this.button1.TabIndex = 12;
            this.button1.Text = "Run New Script";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(22, 190);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 35);
            this.button2.TabIndex = 13;
            this.button2.Text = "Run Selected Script";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "User Scripts";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(25, 30);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(211, 154);
            this.textBox1.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(645, 234);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.stopServButton);
            this.Controls.Add(this.startServButton);
            this.Controls.Add(this.listServOffline);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listServOnline);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.stopCliButton);
            this.Controls.Add(this.startCliButton);
            this.Controls.Add(this.listCliOffline);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listCliOnline);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "PADITable PuppetMaster";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listCliOnline;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listCliOffline;
        private System.Windows.Forms.Button startCliButton;
        private System.Windows.Forms.Button stopCliButton;
        private System.Windows.Forms.Button stopServButton;
        private System.Windows.Forms.Button startServButton;
        private System.Windows.Forms.ListBox listServOffline;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listServOnline;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
    }
}

