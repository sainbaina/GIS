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
            layerCheckList = new CheckedListBox();
            checkBox1 = new CheckBox();
            LayerDelete = new Button();
            ChangeColor = new Button();
            SuspendLayout();
            // 
            // map1
            // 
            map1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            map1.BackColor = SystemColors.HighlightText;
            map1.Location = new System.Drawing.Point(256, 55);
            map1.Name = "map1";
            map1.ScaleFactor = 1F;
            map1.ShowRects = false;
            map1.Size = new Size(518, 368);
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
            label1.Location = new System.Drawing.Point(461, 18);
            label1.Name = "label1";
            label1.Size = new Size(61, 25);
            label1.TabIndex = 4;
            label1.Text = "Scale: ";
            label1.Click += label1_Click;
            // 
            // scaleLabel
            // 
            scaleLabel.AutoSize = true;
            scaleLabel.Location = new System.Drawing.Point(522, 18);
            scaleLabel.Name = "scaleLabel";
            scaleLabel.Size = new Size(62, 25);
            scaleLabel.TabIndex = 5;
            scaleLabel.Text = "00000";
            scaleLabel.Click += label2_Click;
            // 
            // layerCheckList
            // 
            layerCheckList.FormattingEnabled = true;
            layerCheckList.Location = new System.Drawing.Point(12, 139);
            layerCheckList.Name = "layerCheckList";
            layerCheckList.Size = new Size(220, 284);
            layerCheckList.TabIndex = 6;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new System.Drawing.Point(257, 17);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(176, 29);
            checkBox1.TabIndex = 8;
            checkBox1.Text = "Show boundaries";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // LayerDelete
            // 
            LayerDelete.Location = new System.Drawing.Point(12, 55);
            LayerDelete.Name = "LayerDelete";
            LayerDelete.Size = new Size(220, 37);
            LayerDelete.TabIndex = 9;
            LayerDelete.Text = "Delete Layer";
            LayerDelete.UseVisualStyleBackColor = true;
            LayerDelete.Click += LayerDelete_Click;
            // 
            // ChangeColor
            // 
            ChangeColor.Location = new System.Drawing.Point(12, 98);
            ChangeColor.Name = "ChangeColor";
            ChangeColor.Size = new Size(220, 37);
            ChangeColor.TabIndex = 10;
            ChangeColor.Text = "Change Color";
            ChangeColor.UseVisualStyleBackColor = true;
            ChangeColor.Click += ChangeColor_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.InactiveCaption;
            ClientSize = new Size(800, 450);
            Controls.Add(ChangeColor);
            Controls.Add(LayerDelete);
            Controls.Add(checkBox1);
            Controls.Add(layerCheckList);
            Controls.Add(scaleLabel);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(map1);
            Name = "Form1";
            Text = "GIS";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Map map1;
        private Button button1;
        private Label label1;
        private Label scaleLabel;
        private CheckedListBox layerCheckList;
        private CheckBox checkBox1;
        private Button LayerDelete;
        private Button ChangeColor;
    }
}
