using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// 画像ノード追加
        /// </summary>
        private void addImageNodeFromListView()
        {
            if (listViewLib.SelectedItems.Count == 0)
            {
                return;
            }

            TreeNode targetNode = null;

            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                // GIF構造にプロジェクト追加
                targetNode = addProjectNode(treeViewGif.Nodes, "DefaultProject", new Size(500, 500));

                //メニューボタン表示状態更新
                updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
            }

            TreeView tv = treeViewGif;
            if (targetNode == null)
            {
                // 選択先のノード取得
                targetNode = tv.SelectedNode;
            }
            if (targetNode == null)
            {
                // 先頭プロジェクト取得
                targetNode = tv.Nodes[0];
            }

            foreach (ListViewItem srcItem in listViewLib.SelectedItems)
            {
                addImageNode(targetNode, srcItem);
            }
        }

        /// <summary>
        /// 画像ノード追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addImageNode(TreeNode targetNode, ListViewItem srcItem)
        {
            TreeNode parentNode = targetNode.Parent;

            // フレーム単位の画像追加
            if (listViewLib.Visible)
            {
                // 追加先がプロジェクトの場合
                if (parentNode == null)
                {
                    // フレーム生成
                    TreeViewControl.addFrameNode(targetNode, srcItem);

                    // フレームにイメージ追加
                    addImageNodeToProjectFrame(targetNode, srcItem);
                }
                // 追加先がフレームの場合
                else if (parentNode.Parent == null)
                {
                    // フレームにイメージ追加
                    addImageNodeToFrame(targetNode, srcItem);
                }
            }
            // フレーム全体の画像追加
            else
            {
                // 追加先がプロジェクトの場合
                if (parentNode == null)
                {

                }
                // 追加先がフレームの場合
                else if (parentNode.Parent == null)
                {
                    // フレームにイメージ追加
                    addImageNodeToFrame(targetNode, srcItem);
                }
            }
        }

        /// <summary>
        /// フレーム展開画像をプロジェクトの各フレームに追加
        /// </summary>
        /// <param name="projectNode"></param>
        /// <param name="srcItem"></param>
        private void addImageNodeToProjectFrame(TreeNode projectNode, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            if (!File.Exists(strFilePath))
            {
                return;
            }

            Image image = Image.FromFile(strFilePath);
            FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
            int intFrameCount = image.GetFrameCount(frameDimension);
            if (intFrameCount == 1)
            {
                // シングルフレーム画像はプロジェクト配下の全フレームに追加する。
                foreach (TreeNode frameNode in projectNode.Nodes)
                {
                    addImageNodeToFrame(frameNode, "", strFilePath, 1);
                }
            }
            else
            {
                // マルチフレーム画像はプロジェクト配下の各フレームに追加する。
                for (int ii = 0; ii < intFrameCount; ++ii)
                {
                    // フレーム取得
                    TreeNode frameNode = projectNode.Nodes[ii];
                    int intFrameNumber = ii + 1;
                    addImageNodeToFrame(frameNode, "", strFilePath, intFrameNumber);
                }
            }
            image.Dispose();
        }

        /// <summary>
        /// フレーム指定画像をフレームに追加
        /// </summary>
        /// <param name="frameNode"></param>
        /// <param name="srcItem"></param>
        private void addImageNodeToFrame(TreeNode frameNode, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            if (!File.Exists(strFilePath))
            {
                return;
            }

            // フレーム指定画像からフレーム番号を取得する。
            string strFileName = srcItem.Text;
            string[] strFileNameSplit = strFileName.Split(':');
            int intFrameNumber = 1;
            if (strFileNameSplit.Length > 1)
            {
                intFrameNumber = int.Parse(strFileNameSplit[1]);
            }
            addImageNodeToFrame(frameNode, "", strFilePath, intFrameNumber);
        }

        /// <summary>
        /// フレームノード追加
        /// </summary>
        private void addFrameNode()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // 選択要素なし
            if (selectedNode == null)
            {
                return;
            }

            // プロジェクト選択中(デフォルト)
            TreeNode targetNode = selectedNode;
            TreeNode parentNode = selectedNode.Parent;

            if (parentNode == null)
            {
                ;
            }
            // フレーム選択中
            else if (parentNode.Parent == null)
            {
                targetNode = parentNode;
            }
            // その他選択中
            else
            {
                return;
            }

            // 要素追加開始
            treeViewGif.BeginUpdate();

            TreeNode createNode = null;
            int intFrameCount = targetNode.GetNodeCount(false);
            for (int ii = intFrameCount + 1; ; ++ii)
            {
                string strFrameKey = "フレーム" + ii.ToString();
                if (!targetNode.Nodes.ContainsKey(strFrameKey))
                {
                    targetNode.Nodes.Add(strFrameKey, strFrameKey);
                    foreach (TreeNode node in targetNode.Nodes.Find(strFrameKey, false))
                    {
                        node.Checked = true;
                        node.Expand();
                        createNode = node;
                    }
                }
                if (createNode != null)
                {
                    break;
                }
            }

            if (parentNode != null && parentNode.Parent == null)
            {
                int intSelectedFrameCount = 0;
                intFrameCount = parentNode.GetNodeCount(false);
                for (int ii = 0; ii < intFrameCount; ++ii)
                {
                    if (parentNode.Nodes[ii] == selectedNode)
                    {
                        intSelectedFrameCount = ii;
                        break;
                    }
                }
                if (intSelectedFrameCount < intFrameCount - 1)
                {
                    parentNode.Nodes.Remove(createNode);
                    parentNode.Nodes.Insert(intSelectedFrameCount + 1, createNode);
                    treeViewGif.SelectedNode = createNode;
                }
            }

            // 要素追加終了
            treeViewGif.EndUpdate();
        }

        /// <summary>
        /// フレームノード挿入
        /// </summary>
        private void insertFrameNode_Click()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // 選択要素なし
            if (selectedNode == null)
            {
                return;
            }

            // プロジェクト選択中(デフォルト)
            TreeNode targetNode = selectedNode;
            TreeNode parentNode = selectedNode.Parent;

            if (parentNode == null)
            {
                ;
            }
            // フレーム選択中
            else if (parentNode.Parent == null)
            {
                targetNode = parentNode;
            }
            // その他選択中
            else
            {
                return;
            }

            // 要素追加開始
            treeViewGif.BeginUpdate();

            TreeNode createNode = null;
            int intFrameCount = targetNode.GetNodeCount(false);
            for (int ii = intFrameCount + 1; ; ++ii)
            {
                string strFrameKey = "フレーム" + ii.ToString();
                if (!targetNode.Nodes.ContainsKey(strFrameKey))
                {
                    targetNode.Nodes.Add(strFrameKey, strFrameKey);
                    foreach (TreeNode node in targetNode.Nodes.Find(strFrameKey, false))
                    {
                        node.Checked = true;
                        node.Expand();
                        createNode = node;
                    }
                }
                if (createNode != null)
                {
                    break;
                }
            }

            if (parentNode != null && parentNode.Parent == null)
            {
                int intSelectedFrameCount = 0;
                intFrameCount = parentNode.GetNodeCount(false);
                for (int ii = 0; ii < intFrameCount; ++ii)
                {
                    if (parentNode.Nodes[ii] == selectedNode)
                    {
                        intSelectedFrameCount = ii;
                        break;
                    }
                }
                if (intSelectedFrameCount < intFrameCount - 1)
                {
                    parentNode.Nodes.Remove(createNode);
                    parentNode.Nodes.Insert(intSelectedFrameCount, createNode);
                    treeViewGif.SelectedNode = createNode;
                }
            }

            // 要素追加終了
            treeViewGif.EndUpdate();
        }

        /// <summary>
        /// ノード削除
        /// </summary>
        private void removeNode()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // 削除なし
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;

            // プロジェクト削除
            if (parentNode == null)
            {
                treeViewGif.BeginUpdate();

                TreeViewControl.removeNode(selectedNode);

                treeViewGif.EndUpdate();

                if (pictureBoxPreview.Image != null)
                {
                    pictureBoxPreview.Image.Dispose();
                    pictureBoxPreview.Image = null;
                }

                if (treeViewGif.GetNodeCount(false) == 0)
                {
                    strProjectFileName = "";
                }
            }
            // フレーム、イメージ削除
            else
            {
                int intNodeCount = parentNode.GetNodeCount(false);
                for (int ii = 0; ii < intNodeCount; ii++)
                {
                    if (parentNode.Nodes[ii] == selectedNode)
                    {
                        treeViewGif.BeginUpdate();
                        TreeViewControl.removeNode(selectedNode);

                        if (ii == 0)
                        {
                            if (intNodeCount == 1)
                            {
                                treeViewGif.SelectedNode = parentNode;
                            }
                            else
                            {
                                treeViewGif.SelectedNode = parentNode.Nodes[ii];
                            }
                        }
                        else
                        {
                            treeViewGif.SelectedNode = parentNode.Nodes[ii - 1];
                        }

                        treeViewGif.EndUpdate();
                        break;
                    }
                }
            }

            // データブラウザ表示状態更新
            updateDataBrowser(treeViewGif.SelectedNode);

            //メニューボタン表示状態更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);

            GC.Collect();
        }

        /// <summary>
        /// 選択ノードを上へ移動
        /// </summary>
        private void moveUpNode()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // 選択要素なし
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;

            // プロジェクト選択中
            if (parentNode == null)
            {
                return;
            }

            // 先頭要素選択中
            int intNodeCount = parentNode.GetNodeCount(false);
            if (selectedNode == parentNode.Nodes[0])
            {
                return;
            }

            // 要素移動開始
            treeViewGif.BeginUpdate();

            for (int ii = 1; ii < intNodeCount; ++ii)
            {
                if (parentNode.Nodes[ii] == selectedNode)
                {
                    parentNode.Nodes.Remove(selectedNode);
                    parentNode.Nodes.Insert(ii - 1, selectedNode);
                    treeViewGif.SelectedNode = selectedNode;
                    break;
                }
            }

            // 要素移動終了
            treeViewGif.EndUpdate();
        }

        /// <summary>
        /// 選択ノードを下へ移動
        /// </summary>
        private void moveDownNode()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // 選択要素なし
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;

            // プロジェクト選択中
            if (parentNode == null)
            {
                return;
            }

            // 末尾要素選択中
            int intNodeCount = parentNode.GetNodeCount(false);
            if (selectedNode == parentNode.Nodes[intNodeCount - 1])
            {
                return;
            }

            // 要素移動開始
            treeViewGif.BeginUpdate();

            for (int ii = 0; ii < intNodeCount - 1; ++ii)
            {
                if (parentNode.Nodes[ii] == selectedNode)
                {
                    parentNode.Nodes.Remove(selectedNode);
                    parentNode.Nodes.Insert(ii + 1, selectedNode);
                    treeViewGif.SelectedNode = selectedNode;
                    break;
                }
            }

            // 要素移動終了
            treeViewGif.EndUpdate();
        }

        /// <summary>
        /// 画像を移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moveImage(int mode)
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            if (selectedNode == treeViewGif.Nodes[0])
            {
                return;
            }

            int intMovePixel = (int)numericUpDownMovePixel.Value;

            int intX = 0;
            int intY = 0;
            switch (mode)
            {
                case 1: // 上
                    intY = -1 * intMovePixel;
                    break;
                case 2: // 下
                    intY = intMovePixel;
                    break;
                case 3: // 左
                    intX = -1 * intMovePixel;
                    break;
                case 4: // 右
                    intX = intMovePixel;
                    break;
                default:
                    break;
            }

            // 画像位置変更
            moveImage(selectedNode, intX, intY);

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowser(selectedNode);

            // プレビュー再描画
            pictureBoxPreview_Redraw(selectedNode);
        }

        /// <summary>
        /// プロパティ
        /// </summary>
        private void viewPropertyDialog()
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;


            FormProperty formProperty = new FormProperty();

            TreeNodeTag selectedNodeTag = selectedNode.Tag as TreeNodeTag;

            // プロジェクト
            if (parentNode == null)
            {
                formProperty.textBoxProjectName.Text = selectedNode.Text;
                formProperty.numericUpDownWidth.Value = selectedNodeTag.Bitmap.Width;
                formProperty.numericUpDownHeight.Value = selectedNodeTag.Bitmap.Height;

                formProperty.numericUpDownWidth.Enabled = true;
                formProperty.numericUpDownHeight.Enabled = true;
            }
            // フレーム
            else if (parentNode.Parent == null)
            {
                TreeNodeTag parentNodeTag = parentNode.Tag as TreeNodeTag;
                formProperty.textBoxProjectName.Text = selectedNode.Text;
                formProperty.numericUpDownWidth.Value = parentNodeTag.Bitmap.Width;
                formProperty.numericUpDownHeight.Value = parentNodeTag.Bitmap.Height;

                formProperty.numericUpDownWidth.Enabled = false;
                formProperty.numericUpDownHeight.Enabled = false;
            }
            else
            {
                TreeNodeTag parentNodeTag = parentNode.Parent.Tag as TreeNodeTag;
                formProperty.textBoxProjectName.Text = selectedNode.Text;
                formProperty.numericUpDownWidth.Value = parentNodeTag.Bitmap.Width;
                formProperty.numericUpDownHeight.Value = parentNodeTag.Bitmap.Height;

                formProperty.numericUpDownWidth.Enabled = false;
                formProperty.numericUpDownHeight.Enabled = false;
            }

            if (formProperty.ShowDialog() == DialogResult.OK)
            {
                selectedNode.Text = formProperty.textBoxProjectName.Text;

                if (parentNode == null)
                {
                    int width = (int)formProperty.numericUpDownWidth.Value;
                    int height = (int)formProperty.numericUpDownHeight.Value;

                    Bitmap canvas = new Bitmap(width, height);

                    selectedNodeTag.Bitmap.Dispose();
                    selectedNodeTag.Bitmap = canvas;

                    foreach (TreeNode frame in selectedNode.Nodes)
                    {
                        foreach (TreeNode image in frame.Nodes)
                        {
                            TreeNodeTag imageNodeTag = image.Tag as TreeNodeTag;

                            canvas = new Bitmap(width, height);
                            Graphics graphics = Graphics.FromImage(canvas);

                            int posX = (width - imageNodeTag.Bitmap.Width) / 2;
                            int posY = (height - imageNodeTag.Bitmap.Height) / 2;
                            graphics.DrawImage(imageNodeTag.Bitmap, posX, posY, imageNodeTag.Bitmap.Width, imageNodeTag.Bitmap.Height);
                            imageNodeTag.Point = new Point(posX, posY);

                            imageNodeTag.Bitmap.Dispose();
                            imageNodeTag.Bitmap = canvas;

                            graphics.Dispose();
                        }
                    }

                    // コントロールパネル更新
                    updateControlPanel(selectedNode);

                    // 属性パネル更新
                    updateDataBrowser(selectedNode);

                    // プレビュー再描画
                    pictureBoxPreview_Redraw(selectedNode);

                    toolStripLabelImageSize.Text = "縦:" + height.ToString() + "(pixel) 横:" + width + "(pixel)";
                }
            }
        }
    }
}
