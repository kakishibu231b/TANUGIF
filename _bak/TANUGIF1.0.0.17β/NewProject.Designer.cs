
namespace TANUGIF
{
    partial class NewProject
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.buttonCreateNewProject = new System.Windows.Forms.Button();
            this.buttonCancelNewProject = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownFrameCount = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrameCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "プロジェクト名";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(87, 10);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(220, 19);
            this.textBox1.TabIndex = 1;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "サイズ(縦)";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(87, 35);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 19);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "サイズ(横)";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(87, 60);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(120, 19);
            this.numericUpDown2.TabIndex = 5;
            this.numericUpDown2.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // buttonCreateNewProject
            // 
            this.buttonCreateNewProject.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonCreateNewProject.Location = new System.Drawing.Point(60, 120);
            this.buttonCreateNewProject.Name = "buttonCreateNewProject";
            this.buttonCreateNewProject.Size = new System.Drawing.Size(75, 23);
            this.buttonCreateNewProject.TabIndex = 6;
            this.buttonCreateNewProject.Text = "作成";
            this.buttonCreateNewProject.UseVisualStyleBackColor = true;
            this.buttonCreateNewProject.Click += new System.EventHandler(this.createNewProject_Click);
            // 
            // buttonCancelNewProject
            // 
            this.buttonCancelNewProject.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancelNewProject.Location = new System.Drawing.Point(180, 120);
            this.buttonCancelNewProject.Name = "buttonCancelNewProject";
            this.buttonCancelNewProject.Size = new System.Drawing.Size(75, 23);
            this.buttonCancelNewProject.TabIndex = 7;
            this.buttonCancelNewProject.Text = "キャンセル";
            this.buttonCancelNewProject.UseVisualStyleBackColor = true;
            this.buttonCancelNewProject.Click += new System.EventHandler(this.cancelNewProject_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "フレーム数";
            // 
            // numericUpDownFrameCount
            // 
            this.numericUpDownFrameCount.Location = new System.Drawing.Point(87, 85);
            this.numericUpDownFrameCount.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.numericUpDownFrameCount.Name = "numericUpDownFrameCount";
            this.numericUpDownFrameCount.Size = new System.Drawing.Size(120, 19);
            this.numericUpDownFrameCount.TabIndex = 9;
            // 
            // NewProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 161);
            this.Controls.Add(this.numericUpDownFrameCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonCancelNewProject);
            this.Controls.Add(this.buttonCreateNewProject);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewProject";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "新規プロジェクト";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NewProject_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownFrameCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Button buttonCreateNewProject;
        private System.Windows.Forms.Button buttonCancelNewProject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownFrameCount;
    }
}