using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// 操作パネル表示状態更新
        /// </summary>
        /// <param name="selectedNode"></param>
        private void updateControlPanel(TreeNode selectedNode)
        {
            TreeNodeTag treeNodeTag = selectedNode.Tag as TreeNodeTag;

            // プロジェクト選択
            if (selectedNode.Parent == null)
            {
                numericUpDownPosX.Enabled = false;
                numericUpDownPosY.Enabled = false;
            }
            // フレーム選択
            else if (selectedNode.Parent.Parent == null)
            {
                numericUpDownPosX.Enabled = false;
                numericUpDownPosY.Enabled = false;

                updateControlPanel(selectedNode.FirstNode);
            }
            // 画像選択
            else
            {
                numericUpDownPosX.Enabled = true;
                numericUpDownPosY.Enabled = true;

                numericUpDownPosX.Value = treeNodeTag.Point.X;
                numericUpDownPosY.Value = treeNodeTag.Point.Y;
            }
        }

        /// <summary>
        /// 画像位置X座標(pixel)/Y座標(pixel)値変更終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosXY_Leave(object sender, EventArgs e)
        {
            if (numericUpDownPosXY.X == (int)numericUpDownPosX.Value && numericUpDownPosXY.Y == (int)numericUpDownPosY.Value)
            {
                return;
            }

            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;

            // プロジェクト選択時処理なし
            if (parentNode == null)
            {
                return;
            }

            // フレーム選択時
            if (parentNode.Parent == null)
            {
                // フレーム配下に画像がない場合処理なし
                if (selectedNode.GetNodeCount(false) == 0)
                {
                    return;
                }
            }

            // データブラウザ更新
            updateDataBrowser(selectedNode);
        }

        /// <summary>
        /// 画像位置Y座標(pixel)マウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_MouseWheel(object sender, MouseEventArgs e)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            if (0 < e.Delta)
            {
                numericUpDown.DownButton();
            }
            else if (e.Delta < 0)
            {
                numericUpDown.UpButton();
            }
            else
            {
                // DO NOTHING
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }
    }
}
