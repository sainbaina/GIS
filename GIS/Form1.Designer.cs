namespace GIS
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            map1 = new Map();
            button1 = new Button();
            label1 = new Label();
            scaleLabel = new Label();
            SuspendLayout();
            // 
            // map1
            // 
            map1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            map1.BackColor = SystemColors.HighlightText;
            map1.Location = new System.Drawing.Point(238, 52);
            map1.Name = "map1";
            map1.ScaleFactor = 1F;
            map1.Size = new Size(550, 386);
            map1.TabIndex = 0;
            map1.Load += map1_Load;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(220, 37);
            button1.TabIndex = 1;
            button1.Text = "Add Layer";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(238, 24);
            label1.Name = "label1";
            label1.Size = new Size(61, 25);
            label1.TabIndex = 4;
            label1.Text = "Scale: ";
            label1.Click += label1_Click;
            // 
            // scaleLabel
            // 
            scaleLabel.AutoSize = true;
            scaleLabel.Location = new System.Drawing.Point(292, 24);
            scaleLabel.Name = "scaleLabel";
            scaleLabel.Size = new Size(62, 25);
            scaleLabel.TabIndex = 5;
            scaleLabel.Text = "00000";
            scaleLabel.Click += label2_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(800, 450);
            Controls.Add(scaleLabel);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(map1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Map map1;
        private Button button1;
        private Label label1;
        private Label scaleLabel;
    }
}
