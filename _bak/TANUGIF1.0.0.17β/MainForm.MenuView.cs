using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// GIF画像を展開表示
        /// </summary>
        private void expandGifFrame()
        {
            if (listViewLib.SelectedItems.Count == 0)
            {
                return;
            }

            if (listViewLib.Visible)
            {
                listViewLibExpand.Items.Clear();

                // 設定ファイル取得
                Settings settings = Settings.Instance;

                int widthLarge = settings.LibIconSizeLarge;
                int heightLarge = settings.LibIconSizeLarge;

                int widthSmall = settings.LibIconSizeSmall;
                int heightSmall = settings.LibIconSizeSmall;

                foreach (ListViewItem listViewItem in listViewLib.SelectedItems)
                {
                    string strFilePath = (string)listViewItem.Tag;
                    string strLibFileName = listViewItem.Text;

                    if (!File.Exists(strFilePath))
                    {
                        continue;
                    }

                    Image image = Image.FromFile(strFilePath);
                    FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
                    int intFrameCount = image.GetFrameCount(frameDimension);
                    for (int ii = 0; ii < intFrameCount; ++ii)
                    {
                        int intFrameNumber = ii + 1;
                        image.SelectActiveFrame(frameDimension, ii);

                        string strLibKey;
                        strLibKey = strLibFileName + ":" + intFrameNumber.ToString();

                        if (!imageListLibLarge.Images.ContainsKey(strLibKey))
                        {
                            Image thumbnailLarge = TanugifCommon.createThumbnail(image, widthLarge, heightLarge);
                            Image thumbnailSmall = TanugifCommon.createThumbnail(image, widthSmall, heightSmall);
                            imageListLibLarge.Images.Add(strLibKey, thumbnailLarge);
                            imageListLibSmall.Images.Add(strLibKey, thumbnailSmall);
                            thumbnailLarge.Dispose();
                            thumbnailSmall.Dispose();
                        }

                        ListViewItem listViewItemExpand = new ListViewItem();
                        listViewItemExpand.Text = strLibKey;
                        listViewItemExpand.ImageKey = strLibKey;
                        listViewItemExpand.Tag = strFilePath;
                        listViewLibExpand.Items.Add(listViewItemExpand);
                    }

                    image.Dispose();
                }
                listViewLib.Visible = false;
                toolStripMenuItemExpanGifFrame.Checked = true;
            }
            else
            {
                listViewLibExpand.Items.Clear();
                listViewLib.Visible = true;
                toolStripMenuItemExpanGifFrame.Checked = false;
            }
        }

        /// <summary>
        /// 選択ノードの画像再読込
        /// </summary>
        private void reloadImage()
        {
            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            TreeViewControl.reloadTreeNodeImage(selectedNode);

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowser(selectedNode);

            // プレビュー更新
            pictureBoxPreview_Redraw(selectedNode);
        }

        /// <summary>
        /// GIFアニメーション再生
        /// </summary>
        private void playPreview()
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            // GIF再生メニューボタン更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.PlayPreview);

            m_intFrameCount = 0;
            treeViewGif.Enabled = false;
            timerPlayPreview.Interval = (int)numericUpDownDelay.Value;
            timerPlayPreview.Start();
        }

        /// <summary>
        /// GIFアニメーション停止
        /// </summary>
        private void stopPreview()
        {
            timerPlayPreview.Stop();
            treeViewGif.Enabled = true;

            // GIF再生メニューボタン更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.StopPreview);

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
    }
}
