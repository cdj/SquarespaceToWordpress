namespace SquarespaceToWordpress
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
            this.openSquarespaceXmlDialog = new System.Windows.Forms.OpenFileDialog();
            this.loadSquarespaceXmlButton = new System.Windows.Forms.Button();
            this.blogSelectionBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.exportWordpressButton = new System.Windows.Forms.Button();
            this.saveWordpressExportFile = new System.Windows.Forms.SaveFileDialog();
            this.entriesTextBox = new System.Windows.Forms.TextBox();
            this.referencesTextBox = new System.Windows.Forms.TextBox();
            this.commentsTextBox = new System.Windows.Forms.TextBox();
            this.categoriesTextBox = new System.Windows.Forms.TextBox();
            this.tagsTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // openSquarespaceXmlDialog
            // 
            this.openSquarespaceXmlDialog.Filter = "XML Files|*.xml";
            this.openSquarespaceXmlDialog.Title = "Open Squarespace backup XML file";
            // 
            // loadSquarespaceXmlButton
            // 
            this.loadSquarespaceXmlButton.Location = new System.Drawing.Point(12, 12);
            this.loadSquarespaceXmlButton.Name = "loadSquarespaceXmlButton";
            this.loadSquarespaceXmlButton.Size = new System.Drawing.Size(198, 23);
            this.loadSquarespaceXmlButton.TabIndex = 0;
            this.loadSquarespaceXmlButton.Text = "Load Squarespace Backup XML...";
            this.loadSquarespaceXmlButton.UseVisualStyleBackColor = true;
            this.loadSquarespaceXmlButton.Click += new System.EventHandler(this.loadSquarespaceXmlButton_Click);
            // 
            // blogSelectionBox
            // 
            this.blogSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blogSelectionBox.FormattingEnabled = true;
            this.blogSelectionBox.Location = new System.Drawing.Point(54, 41);
            this.blogSelectionBox.Name = "blogSelectionBox";
            this.blogSelectionBox.Size = new System.Drawing.Size(156, 21);
            this.blogSelectionBox.TabIndex = 1;
            this.blogSelectionBox.SelectedIndexChanged += new System.EventHandler(this.blogSelectionBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Blogs:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Entries:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "References:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Comments:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 149);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Categories:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 175);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Tags:";
            // 
            // exportWordpressButton
            // 
            this.exportWordpressButton.Location = new System.Drawing.Point(38, 198);
            this.exportWordpressButton.Name = "exportWordpressButton";
            this.exportWordpressButton.Size = new System.Drawing.Size(147, 23);
            this.exportWordpressButton.TabIndex = 8;
            this.exportWordpressButton.Text = "Export Wordpress File";
            this.exportWordpressButton.UseVisualStyleBackColor = true;
            this.exportWordpressButton.Click += new System.EventHandler(this.exportWordpressButton_Click);
            // 
            // saveWordpressExportFile
            // 
            this.saveWordpressExportFile.DefaultExt = "wxr";
            this.saveWordpressExportFile.Filter = "Wordpress Export File|*.wxr";
            this.saveWordpressExportFile.Title = "Save Wordpress Export File";
            // 
            // entriesTextBox
            // 
            this.entriesTextBox.Enabled = false;
            this.entriesTextBox.Location = new System.Drawing.Point(57, 68);
            this.entriesTextBox.Name = "entriesTextBox";
            this.entriesTextBox.Size = new System.Drawing.Size(50, 20);
            this.entriesTextBox.TabIndex = 9;
            // 
            // referencesTextBox
            // 
            this.referencesTextBox.Enabled = false;
            this.referencesTextBox.Location = new System.Drawing.Point(80, 94);
            this.referencesTextBox.Name = "referencesTextBox";
            this.referencesTextBox.Size = new System.Drawing.Size(50, 20);
            this.referencesTextBox.TabIndex = 10;
            // 
            // commentsTextBox
            // 
            this.commentsTextBox.Enabled = false;
            this.commentsTextBox.Location = new System.Drawing.Point(74, 120);
            this.commentsTextBox.Name = "commentsTextBox";
            this.commentsTextBox.Size = new System.Drawing.Size(50, 20);
            this.commentsTextBox.TabIndex = 11;
            // 
            // categoriesTextBox
            // 
            this.categoriesTextBox.Enabled = false;
            this.categoriesTextBox.Location = new System.Drawing.Point(74, 146);
            this.categoriesTextBox.Name = "categoriesTextBox";
            this.categoriesTextBox.Size = new System.Drawing.Size(50, 20);
            this.categoriesTextBox.TabIndex = 12;
            // 
            // tagsTextBox
            // 
            this.tagsTextBox.Enabled = false;
            this.tagsTextBox.Location = new System.Drawing.Point(49, 172);
            this.tagsTextBox.Name = "tagsTextBox";
            this.tagsTextBox.Size = new System.Drawing.Size(50, 20);
            this.tagsTextBox.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 233);
            this.Controls.Add(this.tagsTextBox);
            this.Controls.Add(this.categoriesTextBox);
            this.Controls.Add(this.commentsTextBox);
            this.Controls.Add(this.referencesTextBox);
            this.Controls.Add(this.entriesTextBox);
            this.Controls.Add(this.exportWordpressButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.blogSelectionBox);
            this.Controls.Add(this.loadSquarespaceXmlButton);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openSquarespaceXmlDialog;
        private System.Windows.Forms.Button loadSquarespaceXmlButton;
        private System.Windows.Forms.ComboBox blogSelectionBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button exportWordpressButton;
        private System.Windows.Forms.SaveFileDialog saveWordpressExportFile;
        private System.Windows.Forms.TextBox entriesTextBox;
        private System.Windows.Forms.TextBox referencesTextBox;
        private System.Windows.Forms.TextBox commentsTextBox;
        private System.Windows.Forms.TextBox categoriesTextBox;
        private System.Windows.Forms.TextBox tagsTextBox;
    }
}

