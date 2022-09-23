using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TANUGIF
{
    public partial class NewProject : Form
    {
        public NewProject()
        {
            InitializeComponent();
        }


        /// <summary>
        /// プロジェクト名取得
        /// </summary>
        /// <returns></returns>
        public string getProjectName()
        {
            return textBox1.Text;
        }

        public Size getProjectImageSize()
        {
            return new Size((int)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        public int getFrameCount()
        {
            return (int)numericUpDownFrameCount.Value;
        }

        private void createNewProject_Click(object sender, EventArgs e)
        {
            if ( textBox1.Text == "" )
            {
                MessageBox.Show("プロジェクト名を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Close();
            }
        }

        private void cancelNewProject_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NewProject_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Enter )
            {
                createNewProject_Click(sender, e);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonCreateNewProject.Focus();
                buttonCreateNewProject.PerformClick();
            }
        }
    }
}
