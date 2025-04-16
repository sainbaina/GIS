namespace GIS
{
    partial class ChoseLayerType
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
            label1 = new Label();
            layerTypeComboBox = new ComboBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(83, 25);
            label1.TabIndex = 0;
            label1.Text = "Тип слоя";
            label1.Click += label1_Click;
            // 
            // layerTypeComboBox
            // 
            layerTypeComboBox.FormattingEnabled = true;
            layerTypeComboBox.Location = new System.Drawing.Point(12, 37);
            layerTypeComboBox.Name = "layerTypeComboBox";
            layerTypeComboBox.Size = new Size(426, 33);
            layerTypeComboBox.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(326, 76);
            button1.Name = "button1";
            button1.Size = new Size(112, 34);
            button1.TabIndex = 2;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // ChoseLayerType
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 450);
            Controls.Add(button1);
            Controls.Add(layerTypeComboBox);
            Controls.Add(label1);
            Name = "ChoseLayerType";
            Text = "Параметры слоя";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox layerTypeComboBox;
        private Button button1;
    }
}