using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// プレビュー再描画
        /// </summary>
        /// <param name="selectedNode"></param>
        private void pictureBoxPreview_Redraw(TreeNode selectedNode)
        {
            // 未選択状態は処理なし
            if (selectedNode == null)
            {
                return;
            }

            //単一画像選択時
            if (selectedNode.GetNodeCount(false) == 0)
            {
                // コントロールパネル更新
                updateControlPanel(selectedNode);

                // 属性パネル更新
                updateDataBrowser(selectedNode);

                // プレビュー更新
                // フレーム選択時の動作とする。
                pictureBoxPreview_Redraw(selectedNode.Parent);
                return;
            }

            TreeNodeTag projectNodeTag = (TreeNodeTag)treeViewGif.Nodes[0].Tag;
            Bitmap canvas = new Bitmap(projectNodeTag.Bitmap);
            Graphics graphics = Graphics.FromImage(canvas);

            // プロジェクト選択時 or フレーム選択時
            if (selectedNode == treeViewGif.Nodes[0] || selectedNode.Parent == treeViewGif.Nodes[0])
            {
                // フレーム選択時(デフォルト)
                TreeNode nodeFrame = selectedNode;

                // プロジェクト選択時
                if (selectedNode == treeViewGif.Nodes[0])
                {
                    nodeFrame = selectedNode.Nodes[0];
                }

                for (int ii = nodeFrame.Nodes.Count - 1; 0 <= ii; --ii)
                {
                    TreeNode nodeImage = nodeFrame.Nodes[ii];
                    if (nodeImage.Checked)
                    {
                        TreeNodeTag treeNodeTag = (TreeNodeTag)nodeImage.Tag;
                        Bitmap bitmap = treeNodeTag.Bitmap;
                        if (bitmap != null)
                        {
                            graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                        }
                    }
                }
            }

            // 枠描画
            graphics.DrawRectangle(Pens.White, 0, 0, canvas.Width - 1, canvas.Height - 1);
            graphics.DrawRectangle(Pens.White, 0, 0, canvas.Width / 2, canvas.Height / 2);
            graphics.DrawRectangle(Pens.White, canvas.Width / 2, canvas.Height / 2, canvas.Width - 1, canvas.Height - 1);

            graphics.Dispose();

            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
            }
            pictureBoxPreview.Image = canvas;
        }

        /// <summary>
        /// GIF再生中
        /// </summary>
        int m_intFrameCount;
        private void timerGifPlay_Tick2(object sender, EventArgs e)
        {
            TreeNode nodeProject = treeViewGif.Nodes[0];
            TreeNode nodeFrame;
            if (m_intFrameCount >= nodeProject.GetNodeCount(false))
            {
                m_intFrameCount = 0;
            }
            nodeFrame = nodeProject.Nodes[m_intFrameCount++];
            pictureBoxDraw(nodeFrame);
        }

        /// <summary>
        /// プレビュー描画(GIF再生用)
        /// </summary>
        /// <param name="nodeFrame"></param>
        private void pictureBoxDraw(TreeNode nodeFrame)
        {
            TreeNodeTag projectNodeTag = (TreeNodeTag)treeViewGif.Nodes[0].Tag;
            Bitmap canvas = new Bitmap(projectNodeTag.Bitmap);
            Graphics graphics = Graphics.FromImage(canvas);

            for (int ii = nodeFrame.Nodes.Count - 1; 0 <= ii; --ii)
            {
                TreeNode node = nodeFrame.Nodes[ii];
                TreeNodeTag treeNodeTag = (TreeNodeTag)node.Tag;
                Bitmap image = treeNodeTag.Bitmap;
                if (image != null)
                {
                    graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }

            // 枠描画
            graphics.DrawRectangle(Pens.White, 0, 0, canvas.Width - 1, canvas.Height - 1);
            graphics.DrawRectangle(Pens.White, 0, 0, canvas.Width / 2, canvas.Height / 2);
            graphics.DrawRectangle(Pens.White, canvas.Width / 2, canvas.Height / 2, canvas.Width - 1, canvas.Height - 1);
            graphics.Dispose();

            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
            }
            pictureBoxPreview.Image = canvas;
        }

        // プレビュー座標値記録用
        Point m_mousePoint;

        /// <summary>
        /// プレビュー座標値変更モード開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseDown2(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                m_mousePoint = e.Location;

                TreeNode selectedNode = treeViewGif.SelectedNode;

                // GIF構成選択要素なし
                if (selectedNode == null)
                {
                    return;
                }

                // プロジェクト選択時
                if (selectedNode.Parent == null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// プレビュー座標値変更処理中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseMove2(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                TreeNode selectedNode = treeViewGif.SelectedNode;

                // GIF構成選択要素なし
                if (selectedNode == null)
                {
                    return;
                }

                // プロジェクト選択時
                if (selectedNode.Parent == null)
                {
                    return;
                }

                int intX = e.Location.X - m_mousePoint.X;
                int intY = e.Location.Y - m_mousePoint.Y;

                // 画像位置変更
                moveImage(selectedNode, intX, intY);

                // プレビュー再描画
                pictureBoxPreview_Redraw(selectedNode);

                // マウス座標値記録
                m_mousePoint = e.Location;
            }

            if (pictureBoxPreview.Image != null)
            {
                // プレビュー領域サイズ
                Point point = e.Location;
                point.X = point.X - ((pictureBoxPreview.Width - pictureBoxPreview.Image.Width) / 2);
                point.Y = point.Y - ((pictureBoxPreview.Height - pictureBoxPreview.Image.Height) / 2);
                toolStripLabelMousePos.Text = point.ToString();
            }
        }

        /// <summary>
        /// プレビュー座標値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void moveImage(TreeNode targetNode, int posX, int posY)
        {
            if (targetNode.GetNodeCount(false) == 0)
            {
                if (targetNode.Checked)
                {
                    TreeNodeTag treeNodeTag = (TreeNodeTag)targetNode.Tag;
                    Bitmap soruce = treeNodeTag.Bitmap;
                    Bitmap canvas = new Bitmap(soruce.Width, soruce.Height);
                    Graphics graphics = Graphics.FromImage(canvas);
                    graphics.DrawImage(soruce, posX, posY, soruce.Width, soruce.Height);

                    Point point = treeNodeTag.Point;
                    point.X += posX;
                    point.Y += posY;
                    treeNodeTag.Point = point;

                    treeNodeTag.Bitmap.Dispose();
                    treeNodeTag.Bitmap = canvas;

                    targetNode.Tag = treeNodeTag;
                    graphics.Dispose();
                }
            }
            else
            {
                foreach (TreeNode childNode in targetNode.Nodes)
                {
                    moveImage(childNode, posX, posY);
                }
            }
        }

        /// <summary>
        /// プレビュー座標値変更処理終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseUp2(object sender, MouseEventArgs e)
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;

            // GIF構成選択要素なし
            if (selectedNode == null)
            {
                return;
            }

            // プロジェクト選択時
            if (selectedNode.Parent == null)
            {
                return;
            }

            // コントロールパネル更新
            bolUpdateControlPanel = false;
            updateControlPanel(selectedNode);
            bolUpdateControlPanel = true;

            // 属性パネル更新
            updateDataBrowser(selectedNode);
        }

        /// <summary>
        /// プレビューマウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseWheel(object sender, MouseEventArgs e)
        {
            bool bolControlKeyPress = false;
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                bolControlKeyPress = true;
            }

            if (bolControlKeyPress)
            {
                if (0 < e.Delta)
                {
                    numericUpDownPosX.DownButton();
                }
                else if (e.Delta < 0)
                {
                    numericUpDownPosX.UpButton();
                }
                else
                {
                    // DO NOTHING
                }
            }
            else
            {
                if (0 < e.Delta)
                {
                    numericUpDownPosY.DownButton();
                }
                else if (e.Delta < 0)
                {
                    numericUpDownPosY.UpButton();
                }
                else
                {
                    // DO NOTHING
                }
            }

            ((HandledMouseEventArgs)e).Handled = true;
        }

        /// <summary>
        /// 画像位置X座標(pixel)/Y座標(pixel)値変更
        /// </summary>
        private void changeImagePos(TreeNode selectedNode)
        {
            if (bolUpdateControlPanel == false)
            {
                return;
            }

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

            int posX = 0;
            int posY = 0;

            // フレーム選択時
            if (parentNode.Parent == null)
            {
                // 先頭画像を基準とする。
                TreeNodeTag treeNodeTag = selectedNode.Nodes[0].Tag as TreeNodeTag;
                Point point = treeNodeTag.Point;
                posX = (int)numericUpDownPosX.Value - point.X;
                posY = (int)numericUpDownPosY.Value - point.Y;
            }
            // 画像選択時
            else
            {
                TreeNodeTag treeNodeTag = selectedNode.Tag as TreeNodeTag;
                Point point = treeNodeTag.Point;
                posX = (int)numericUpDownPosX.Value - point.X;
                posY = (int)numericUpDownPosY.Value - point.Y;
            }

            // 画像位置変更
            moveImage(selectedNode, posX, posY);

            // プレビュー再描画
            pictureBoxPreview_Redraw(selectedNode);
        }
    }
}
