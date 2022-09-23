using System.Drawing;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// GIF構成へドロップ可否判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_DragEnter2(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            if ((e.Data.GetDataPresent(typeof(ListViewItem))))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// GIF構成へドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_DragDrop2(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            bolAddNode = true;

            TreeNode targetNode = null;

            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                // GIF構造にプロジェクト追加
                targetNode = addProjectNode(treeViewGif.Nodes, "DefaultProject", new Size(500, 500));

                //メニューボタン表示状態更新
                updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
            }

            TreeView tv = sender as TreeView;
            if (targetNode == null)
            {
                // ドロップ先のノード取得
                targetNode = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
            }
            if (targetNode == null)
            {
                // 先頭プロジェクト取得
                targetNode = tv.Nodes[0];
            }

            ListViewItem srcItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            addImageNode(targetNode, srcItem);

            bolAddNode = false;

            // 未選択状態は処理なし
            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowser(selectedNode);

            // プレビュー更新
            pictureBoxPreview_Redraw(selectedNode);

        }

        /// <summary>
        /// GIF構成ノード選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect2(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            TreeNode selectedNode = e.Node;
            TreeNodeTag treeNodeTag = selectedNode.Tag as TreeNodeTag;

            // コントロールパネル更新
            bolUpdateControlPanel = false;
            updateControlPanel(selectedNode);
            bolUpdateControlPanel = true;

            // 属性パネル更新
            updateDataBrowser(selectedNode);

            // プレビュー更新
            pictureBoxPreview_Redraw(selectedNode);

            //メニューボタン表示状態更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
        }

        /// <summary>
        /// GIF構成チェック状態変更前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_BeforeCheck2(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode targetNode = e.Node;

            // プロジェクト、フレームはチェックON状態からOFF状態に変更不可
            if( e.Node.Checked )
            {
                // プロジェクト
                if (targetNode.Parent == null)
                {
                    e.Cancel = true;
                }
                // フレーム
                else if (targetNode.Parent.Parent == null)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// ノード追加モード
        /// </summary>
        bool bolAddNode = false;

        /// <summary>
        /// GIF構成チェック状態変更後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_AfterCheck2(object sender, TreeViewEventArgs e)
        {
            // 未選択状態は処理なし
            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            // フレームなしの場合処理なし
            if (treeViewGif.Nodes[0] == selectedNode)
            {
                if (selectedNode.GetNodeCount(false) == 0)
                {
                    return;
                }
            }

            if (bolAddNode)
            {
                return;
            }

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowser(selectedNode);

            // プレビュー更新
            pictureBoxPreview_Redraw(selectedNode);
        }

        /// <summary>
        /// GIF構成キー押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_KeyDown2(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                toolStripButtonRemoveNode.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.Up)
            {
                toolStripButtonMoveUpNode.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                toolStripButtonMoveDownNode.PerformClick();
                e.Handled = true;
            }
        }

        /// <summary>
        /// ラベル編集後
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_AfterLabelEdit2(object sender, NodeLabelEditEventArgs e)
        {
            if(e.Label == null)
            {
                return;
            }

            e.Node.Text = e.Label;

            // 属性パネル更新
            updateDataBrowser(e.Node);
        }
    }
}
