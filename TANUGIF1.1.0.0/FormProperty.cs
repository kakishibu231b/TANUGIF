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
    public partial class FormProperty : Form
    {
        public FormProperty()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 新規プロジェクトダイアログ閉じるフラグ
        /// </summary>
        bool bolFormClosing = true;

        /// <summary>
        /// プロパティダイアログ閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormProperty_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!bolFormClosing)
            {
                bolFormClosing = true;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxName.Text == "")
            {
                MessageBox.Show("プロジェクト名を入力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // ダイアログは閉じない
                bolFormClosing = false;

                // 入力が必要な箇所を対象に背景色を変更する。
                textBoxName.BackColor = Color.Pink;

                // 入力が必要な箇所をアクティブにする。
                ActiveControl = textBoxName;
            }
        }
    }
}
