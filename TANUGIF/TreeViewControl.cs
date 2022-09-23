using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;

namespace TANUGIF
{
    internal class TreeViewControl
    {
        /// <summary>
        /// プロジェクト追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static TreeNode addProjectNode(TreeNodeCollection treeNodeCollection, string strProjectName, Bitmap bitmap)
        {
            TreeNode projectNode = null;

            if (!treeNodeCollection.ContainsKey(strProjectName))
            {
                // GIF構造プロジェクト追加
                treeNodeCollection.Add(strProjectName, strProjectName);
            }

            foreach (TreeNode node in treeNodeCollection.Find(strProjectName, false))
            {
                if(node.Tag == null)
                {
                    TreeNodeTag treeNodeTag = new TreeNodeTag(bitmap, "", 0);
                    node.ImageKey = strProjectName;
                    node.SelectedImageKey = strProjectName;
                    node.StateImageKey = strProjectName;
                    node.Checked = true;
                    node.Tag = treeNodeTag;
                }
                projectNode = node;
                break;
            }

            return projectNode;
        }

        /// <summary>
        /// フレーム追加
        /// </summary>
        /// <param name="projectNode"></param>
        /// <param name="srcItem"></param>
        public static void addFrameNode(TreeNode projectNode, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            if (!File.Exists(strFilePath))
            {
                return;
            }

            Image image = Image.FromFile(strFilePath);
            FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
            int intFrameCount = image.GetFrameCount(frameDimension);
            image.Dispose();

            TreeViewControl.addFrameNode(projectNode, intFrameCount);
        }

        /// <summary>
        /// フレーム追加
        /// </summary>
        /// <param name="projectNode"></param>
        /// <param name="srcItem"></param>
        public static void addFrameNode(TreeNode projectNode, int intFrameCount)
        {
            if (projectNode.Parent != null)
            {
                return;
            }

            int intCreateFrameCount = 0;
            for (int ii = 1; ii <= intFrameCount; ++ii)
            {
                string strFrameKey = "フレーム" + ii.ToString();
                addFrameNode(projectNode, strFrameKey);
                intCreateFrameCount++;
            }
            if (0 < intCreateFrameCount)
            {
                projectNode.Expand();
            }
        }

        /// <summary>
        /// フレーム追加
        /// </summary>
        /// <param name="nodeProject"></param>
        /// <param name="srcItem"></param>
        public static TreeNode addFrameNode(TreeNode nodeProject, string strFrameKey)
        {
            bool isContains = nodeProject.Nodes.ContainsKey(strFrameKey);
            if (!isContains)
            {
                // GIF構造フレーム追加
                nodeProject.Nodes.Add(strFrameKey, strFrameKey);
            }

            TreeNode frameNode = null;
            foreach (TreeNode node in nodeProject.Nodes.Find(strFrameKey, false))
            {
                if (!isContains)
                {
                    node.Checked = true;
                    node.Expand();
                }
                frameNode = node;
                break;
            }

            return frameNode;
        }

        /// <summary>
        /// 画像追加
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        public static void addImageNode(TreeNode frameNode, string name, string strImageKey, string strImageName, Image image, string strFilePath, int intFrameNumber, Point point = new Point())
        {
            //// 同一画像は追加しない。
            //if (frameNode.Nodes.ContainsKey(strImageKey))
            //{
            //    return;
            //}

            // 出力サイズ取得
            Size size = getOutputSize(frameNode);

            // フレームに画像を追加する。
            if (name == "")
            {
                frameNode.Nodes.Add(strImageKey, strImageName);
            }
            else
            {
                frameNode.Nodes.Add(strImageKey, name);
            }

            // 画像にタグを追加する。
            foreach (TreeNode nodeImage in frameNode.Nodes.Find(strImageKey, false))
            {
                // 画像を生成する。
                Bitmap canvas = new Bitmap(size.Width, size.Height);
                Graphics graphics = Graphics.FromImage(canvas);
                graphics.DrawImage(image, point.X, point.Y, image.Width, image.Height);

                TreeNodeTag treeNodeTag = new TreeNodeTag(canvas, strFilePath, intFrameNumber);
                treeNodeTag.Point = point;

                nodeImage.ImageKey = strImageKey;
                nodeImage.SelectedImageKey = strImageKey;
                nodeImage.StateImageKey = strImageKey;
                nodeImage.Tag = treeNodeTag;
                nodeImage.Checked = true;

                graphics.Dispose();
            }

            // フレームを展開する。
            int intNodeCount = frameNode.GetNodeCount(false);
            if (intNodeCount == 1)
            {
                frameNode.Expand();
            }

        }

        /// <summary>
        /// 再読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void reloadTreeNodeImage(TreeNode targetNode)
        {
            if (targetNode == null)
            {
                return;
            }

            // プロジェクトまたはフレーム選択時
            if (targetNode.GetNodeCount(false) > 0)
            {
                foreach (TreeNode node in targetNode.Nodes)
                {
                    reloadTreeNodeImage(node);
                }
                return;
            }
            // 画像選択時
            else
            {
                // 出力サイズ取得
                Size size = getOutputSize(targetNode);

                TreeNodeTag treeNodeTag = targetNode.Tag as TreeNodeTag;
                string strFilePath = treeNodeTag.FilePath;
                int intFrameNumber = treeNodeTag.FrameNumber;
                Point point = treeNodeTag.Point;

                Image image = Image.FromFile(strFilePath);
                FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
                int intFrameCount = image.GetFrameCount(frameDimension);
                if (intFrameNumber <= intFrameCount)
                {
                    image.SelectActiveFrame(frameDimension, intFrameNumber - 1);

                    Bitmap canvas = new Bitmap(size.Width, size.Height);
                    Graphics graphics = Graphics.FromImage(canvas);
                    graphics.DrawImage(image, point.X, point.Y, image.Width, image.Height);

                    treeNodeTag.Bitmap.Dispose();
                    treeNodeTag.Bitmap = canvas;

                    graphics.Dispose();
                }
            }
        }

        /// <summary>
        /// GIF構成削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void removeNode(TreeNode targetNode)
        {
            if (targetNode == null)
            {
                return;
            }

            if (targetNode.GetNodeCount(false) == 0)
            {
                if (targetNode.Tag != null)
                {
                    TreeNodeTag treeNodeTag = targetNode.Tag as TreeNodeTag;
                    if(treeNodeTag.Bitmap != null)
                    {
                        treeNodeTag.Bitmap.Dispose();
                        treeNodeTag.Bitmap = null;
                    }
                }
                targetNode.Remove();
            }
            else
            {
                // ツリーの下から上に向かって削除する
                while (targetNode.GetNodeCount(false) > 0)
                {
                    int intNodeCount = targetNode.GetNodeCount(false);
                    removeNode(targetNode.Nodes[intNodeCount - 1]);
                }
                removeNode(targetNode);
            }
        }

        /// <summary>
        /// 出力サイズ取得
        /// </summary>
        /// <returns></returns>
        private static Size getOutputSize(TreeNode targetNode)
        {
            Size size = new Size(0, 0);
            if (targetNode == null)
            {
                return size;
            }

            if (targetNode.Parent == null)
            {
                TreeNode projectNode = targetNode;
                TreeNodeTag treeNodeTag = projectNode.Tag as TreeNodeTag;
                return treeNodeTag.Bitmap.Size;
            }
            else
            {
                return getOutputSize(targetNode.Parent);
            }
        }


    }
}
