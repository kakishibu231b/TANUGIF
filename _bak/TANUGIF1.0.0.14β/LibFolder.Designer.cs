
namespace TANUGIF
{
    partial class LibFolder
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
            this.listBoxLibFolder = new System.Windows.Forms.ListBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.checkBoxFolder = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBoxLibFolder
            // 
            this.listBoxLibFolder.FormattingEnabled = true;
            this.listBoxLibFolder.ItemHeight = 12;
            this.listBoxLibFolder.Location = new System.Drawing.Point(13, 61);
            this.listBoxLibFolder.Name = "listBoxLibFolder";
            this.listBoxLibFolder.Size = new System.Drawing.Size(723, 340);
            this.listBoxLibFolder.TabIndex = 0;
            this.listBoxLibFolder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLibFolder_KeyDown);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(224, 406);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(525, 406);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "キャンセル";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // buttonUp
            // 
            this.buttonUp.Location = new System.Drawing.Point(742, 61);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(30, 160);
            this.buttonUp.TabIndex = 3;
            this.buttonUp.Text = "↑";
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // buttonDown
            // 
            this.buttonDown.Location = new System.Drawing.Point(742, 241);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(30, 160);
            this.buttonDown.TabIndex = 4;
            this.buttonDown.Text = "↓";
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(12, 12);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 5;
            this.buttonAdd.Text = "追加";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDel
            // 
            this.buttonDel.Location = new System.Drawing.Point(93, 12);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(75, 23);
            this.buttonDel.TabIndex = 6;
            this.buttonDel.Text = "削除";
            this.buttonDel.UseVisualStyleBackColor = true;
            this.buttonDel.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "ライブラリフォルダ一覧";
            // 
            // checkBoxFolder
            // 
            this.checkBoxFolder.AutoSize = true;
            this.checkBoxFolder.Location = new System.Drawing.Point(175, 18);
            this.checkBoxFolder.Name = "checkBoxFolder";
            this.checkBoxFolder.Size = new System.Drawing.Size(111, 16);
            this.checkBoxFolder.TabIndex = 8;
            this.checkBoxFolder.Text = "フォルダを展開する";
            this.checkBoxFolder.UseVisualStyleBackColor = true;
            // 
            // LibFolder
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 441);
            this.Controls.Add(this.checkBoxFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonDel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonDown);
            this.Controls.Add(this.buttonUp);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listBoxLibFolder);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LibFolder";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ライブラリフォルダ一覧";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LibFolder_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LibFolder_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LibFolder_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxLibFolder;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox checkBoxFolder;
    }
}