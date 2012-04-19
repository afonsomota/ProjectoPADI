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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.listBox3 = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button9 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(556, 293);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Clients Offline";
            // 
            // listCliOnline
            // 
            this.listCliOnline.FormattingEnabled = true;
            this.listCliOnline.Location = new System.Drawing.Point(559, 310);
            this.listCliOnline.Name = "listCliOnline";
            this.listCliOnline.Size = new System.Drawing.Size(116, 82);
            this.listCliOnline.TabIndex = 1;
            this.listCliOnline.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(556, 407);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Clients Online";
            // 
            // listCliOffline
            // 
            this.listCliOffline.FormattingEnabled = true;
            this.listCliOffline.Location = new System.Drawing.Point(559, 423);
            this.listCliOffline.Name = "listCliOffline";
            this.listCliOffline.Size = new System.Drawing.Size(116, 82);
            this.listCliOffline.TabIndex = 3;
            this.listCliOffline.SelectedIndexChanged += new System.EventHandler(this.listCliOffline_SelectedIndexChanged);
            // 
            // startCliButton
            // 
            this.startCliButton.Location = new System.Drawing.Point(682, 310);
            this.startCliButton.Name = "startCliButton";
            this.startCliButton.Size = new System.Drawing.Size(40, 82);
            this.startCliButton.TabIndex = 4;
            this.startCliButton.Text = "Start";
            this.startCliButton.UseVisualStyleBackColor = true;
            this.startCliButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopCliButton
            // 
            this.stopCliButton.Location = new System.Drawing.Point(682, 423);
            this.stopCliButton.Name = "stopCliButton";
            this.stopCliButton.Size = new System.Drawing.Size(40, 82);
            this.stopCliButton.TabIndex = 5;
            this.stopCliButton.Text = "Stop";
            this.stopCliButton.UseVisualStyleBackColor = true;
            this.stopCliButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // stopServButton
            // 
            this.stopServButton.Location = new System.Drawing.Point(880, 423);
            this.stopServButton.Name = "stopServButton";
            this.stopServButton.Size = new System.Drawing.Size(40, 82);
            this.stopServButton.TabIndex = 11;
            this.stopServButton.Text = "Stop";
            this.stopServButton.UseVisualStyleBackColor = true;
            this.stopServButton.Click += new System.EventHandler(this.button3_Click);
            // 
            // startServButton
            // 
            this.startServButton.Location = new System.Drawing.Point(880, 310);
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
            this.listServOffline.Location = new System.Drawing.Point(757, 423);
            this.listServOffline.Name = "listServOffline";
            this.listServOffline.Size = new System.Drawing.Size(116, 82);
            this.listServOffline.TabIndex = 9;
            this.listServOffline.SelectedIndexChanged += new System.EventHandler(this.listServOffline_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(754, 407);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Servers Online";
            // 
            // listServOnline
            // 
            this.listServOnline.FormattingEnabled = true;
            this.listServOnline.Location = new System.Drawing.Point(757, 310);
            this.listServOnline.Name = "listServOnline";
            this.listServOnline.Size = new System.Drawing.Size(116, 82);
            this.listServOnline.TabIndex = 7;
            this.listServOnline.SelectedIndexChanged += new System.EventHandler(this.listServOnline_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(754, 293);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Servers Offline";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(136, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 35);
            this.button1.TabIndex = 12;
            this.button1.Text = "Enter New Script";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(246, 190);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 35);
            this.button2.TabIndex = 13;
            this.button2.Text = "Select Script";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
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
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(25, 30);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(895, 147);
            this.listBox1.TabIndex = 15;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(556, 525);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Add Operation";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(559, 541);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(208, 20);
            this.textBox1.TabIndex = 18;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(22, 483);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 13);
            this.label7.TabIndex = 19;
            // 
            // listBox3
            // 
            this.listBox3.FormattingEnabled = true;
            this.listBox3.Location = new System.Drawing.Point(22, 245);
            this.listBox3.Name = "listBox3";
            this.listBox3.Size = new System.Drawing.Size(401, 290);
            this.listBox3.TabIndex = 20;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(22, 538);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(107, 23);
            this.button3.TabIndex = 21;
            this.button3.Text = "Run Step";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(323, 538);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 23);
            this.button4.TabIndex = 22;
            this.button4.Text = "Run Script";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(784, 540);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(135, 23);
            this.button5.TabIndex = 23;
            this.button5.Text = "Run";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(25, 190);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(104, 35);
            this.button6.TabIndex = 24;
            this.button6.Text = "Enter and Run Script";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(555, 201);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 50);
            this.button7.TabIndex = 25;
            this.button7.Text = " Run Stupid Client";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(757, 201);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(75, 50);
            this.button8.TabIndex = 26;
            this.button8.Text = "Run Stupid Server";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(636, 217);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(86, 20);
            this.textBox2.TabIndex = 27;
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(849, 201);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(75, 50);
            this.button9.TabIndex = 28;
            this.button9.Text = "Kill Servers and Clients";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(936, 567);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.listBox3);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.listBox1);
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
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox listBox3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button9;
    }
}

