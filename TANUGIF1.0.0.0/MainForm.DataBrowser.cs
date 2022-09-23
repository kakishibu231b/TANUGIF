using System.Collections.Generic;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// データブラウザ表示状態更新
        /// </summary>
        /// <param name="selectedNode"></param>
        private void updateDataBrowser(TreeNode selectedNode, bool bolUpdateDataBrowser = true)
        {
            if (!bolUpdateDataBrowser)
            {
                return;
            }

            // 列再構築準備
            DataGridViewTextBoxColumn[] dataGridViewTextBoxColumns = new DataGridViewTextBoxColumn[dataGridViewGif.Columns.Count];
            for (int ii = 0; ii < dataGridViewGif.Columns.Count; ++ii)
            {
                DataGridViewTextBoxColumn dataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
                dataGridViewTextBoxColumn.HeaderText = dataGridViewGif.Columns[ii].HeaderText;
                dataGridViewTextBoxColumn.Width = dataGridViewGif.Columns[ii].Width;
                dataGridViewTextBoxColumn.DataPropertyName = "col_" + ii.ToString();
                dataGridViewTextBoxColumns[ii] = dataGridViewTextBoxColumn;
            }

            dataGridViewGif.Visible = false;
            dataGridViewGif.SuspendLayout();

            // 行削除
            dataGridViewGif.DataSource = null;
            dataGridViewGif.Rows.Clear();

            // 列再構築
            dataGridViewGif.Columns.Clear();
            dataGridViewGif.Columns.AddRange(dataGridViewTextBoxColumns);

            // 行再構築
            List<DataGritViewBindData> listDataGritViewBindData = new List<DataGritViewBindData>();
            updateDataBrowser(selectedNode, listDataGritViewBindData);
            dataGridViewGif.DataSource = listDataGritViewBindData;

            dataGridViewGif.ResumeLayout(false);
            dataGridViewGif.PerformLayout();
            dataGridViewGif.Visible = true;
        }

        /// <summary>
        /// データブラウザ更新
        /// </summary>
        /// <param name="selectedNode"></param>
        private void updateDataBrowser(TreeNode selectedNode, List<DataGritViewBindData> listDataGritViewBindData, int number = 0)
        {
            if (selectedNode == null)
            {
                return;
            }

            TreeNodeTag treeNodeTag = selectedNode.Tag as TreeNodeTag;

            // 属性パネル更新
            if (selectedNode.Parent == null)
            {
                listDataGritViewBindData.Add(new DataGritViewBindData("", "", "名前", selectedNode.Text));
                listDataGritViewBindData.Add(new DataGritViewBindData("", "", "出力幅(pixel)", treeNodeTag.Bitmap.Width.ToString()));
                listDataGritViewBindData.Add(new DataGritViewBindData("", "", "出力高(pixel)", treeNodeTag.Bitmap.Height.ToString()));
                foreach (TreeNode node in selectedNode.Nodes)
                {
                    updateDataBrowser(node, listDataGritViewBindData);
                }
            }
            else if (selectedNode.Parent.Parent == null)
            {
                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Text, "", "名前", selectedNode.Text));
                foreach (TreeNode node in selectedNode.Nodes)
                {
                    updateDataBrowser(node, listDataGritViewBindData, ++number);
                }
            }
            else
            {
                if (number == 0)
                {
                    foreach (TreeNode node in selectedNode.Parent.Nodes)
                    {
                        ++number;
                        if (node == selectedNode)
                        {
                            break;
                        }
                    }
                }

                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Parent.Text, number.ToString(), "名前", selectedNode.Text));
                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Parent.Text, number.ToString(), "ファイル名", treeNodeTag.FilePath));
                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Parent.Text, number.ToString(), "フレーム番号", treeNodeTag.FrameNumber.ToString()));
                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Parent.Text, number.ToString(), "X座標", treeNodeTag.Point.X.ToString()));
                listDataGritViewBindData.Add(new DataGritViewBindData(selectedNode.Parent.Text, number.ToString(), "Y座標", treeNodeTag.Point.Y.ToString()));
            }
        }
    }
}
