using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// ウィンドウ表示初期化
        /// </summary>
        private void initWindow()
        {
            // 設定ファイル取得
            Settings settings = Settings.Instance;

            // ウィンドウ表示状態の復元
            int intWindowPosX = settings.WindowLocationX;
            int intWindowPosY = settings.WindowLocationY;
            int intWindowWidth = settings.WindowWidth;
            int intWindowHeight = settings.WindowHeight;

            if (intWindowWidth > 0 && intWindowHeight > 0)
            {
                Location = new Point(intWindowPosX, intWindowPosY);
                Width = intWindowWidth;
                Height = intWindowHeight;
                switch (settings.WindowState)
                {
                    case (int)FormWindowState.Normal:
                    case (int)FormWindowState.Maximized:
                        WindowState = (FormWindowState)settings.WindowState;
                        break;
                    case (int)FormWindowState.Minimized:
                        WindowState = FormWindowState.Normal;
                        break;
                    default:
                        WindowState = FormWindowState.Normal;
                        break;
                }
                splitContainerMain.SplitterDistance = settings.SplitContainerMainSplitterDistance;
                splitContainerLibPreview.SplitterDistance = settings.SsplitContainerLibPreviewSplitterDistance;
            }

            // ライブラリ設定状態の復元
            List<string> libPath = settings.LibPath;

            toolStripComboBoxLibFolder.Items.Clear();
            toolStripComboBoxLibFolder.Items.Add("ALL");
            foreach (string path in libPath)
            {
                string strFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                toolStripComboBoxLibFolder.Items.Add(strFileNameWithoutExtension);
            }

            int widthLarge = settings.LibIconSizeLarge;
            int heightLarge = settings.LibIconSizeLarge;
            imageListLibLarge.ImageSize = new Size(widthLarge, heightLarge);

            int widthSmall = settings.LibIconSizeSmall;
            int heightSmall = settings.LibIconSizeSmall;
            imageListLibSmall.ImageSize = new Size(widthSmall, heightSmall);

            toolStripStatusLabel1.Text = "";
            toolStripStatusLabelLib.Text = "";
            toolStripLabelImageSize.Text = "";
            toolStripLabelMousePos.Text = "";
        }

        //------------------------------------------------------------------------------------------
        // メニューボタン押下可否制御
        //------------------------------------------------------------------------------------------

        enum UpdateMenuButtonEnabledMode
        {
            Begin = 0,
            Normal,
            OutputGif,
            OutputGifEnd,
            PlayPreview,
            StopPreview,
            End
        }

        /// <summary>
        /// メニューボタン表示状態更新
        /// </summary>
        private void updateMenuButtonEnabled(UpdateMenuButtonEnabledMode updateMenuButtonEnabledMode)
        {
            bool bolUseProjectFile = false;
            bool bolExistProjectNode = false;
            bool bolExistImageNode = false;
            bool bolSelectListViewItem = false;
            bool bolSelectNode = false;

            if (strProjectFileName != "")
            {
                bolUseProjectFile = true;
            }
            if (treeViewGif.GetNodeCount(false) > 0)
            {
                bolExistProjectNode = true;

                foreach (TreeNode projectNode in treeViewGif.Nodes)
                {
                    foreach (TreeNode frameNode in projectNode.Nodes)
                    {
                        foreach (TreeNode imageNode in frameNode.Nodes)
                        {
                            bolExistImageNode = true;
                            break;
                        }
                        if (bolExistImageNode)
                        {
                            break;
                        }
                    }
                    if (bolExistImageNode)
                    {
                        break;
                    }
                }
            }
            if (listViewLib.SelectedItems.Count > 0)
            {
                bolSelectListViewItem = true;
            }

            TreeNode selectedNode = treeViewGif.SelectedNode;
            if (selectedNode != null)
            {
                bolSelectNode = true;
            }

            // プロジェクト保存
            toolStripMenuItemSaveProject.Enabled = bolUseProjectFile;
            toolStripButtonSaveProject.Enabled = bolUseProjectFile;

            // プロジェクト名前を付けてプロジェクト保存
            toolStripMenuItemSaveAsProject.Enabled = bolExistProjectNode;
            toolStripButtonSaveAsProject.Enabled = bolExistProjectNode;

            // GIF生成
            toolStripMenuItemOutputGif.Enabled = bolExistImageNode;
            toolStripButtonOutputGif.Enabled = bolExistImageNode;

            // GIF生成中止
            toolStripMenuItemCancelOutputGif.Enabled = false;
            toolStripButtonCancelOutputGif.Enabled = false;

            // ノードを上へ移動
            toolStripMenuItemMoveUpNode.Enabled = bolExistProjectNode;
            toolStripButtonMoveUpNode.Enabled = bolExistProjectNode;

            // ノードを下へ移動
            toolStripMenuItemMoveDownNode.Enabled = bolExistProjectNode;
            toolStripButtonMoveDownNode.Enabled = bolExistProjectNode;

            // フレームノード追加
            toolStripMenuItemAddFrameNode.Enabled = bolExistProjectNode;
            toolStripButtonAddFrameNode.Enabled = bolExistProjectNode;

            // フレームノード挿入
            toolStripMenuItemInsertFrameNode.Enabled = bolExistProjectNode;
            toolStripButtonInsertFrameNode.Enabled = bolExistProjectNode;

            // ノード削除
            toolStripMenuItemRemoveNode.Enabled = bolExistProjectNode;
            toolStripButtonRemoveNode.Enabled = bolExistProjectNode;

            // 画像上下左右へ
            toolStripMenuItemPreviewUp.Enabled = bolSelectNode;
            toolStripMenuItemPreviewDown.Enabled = bolSelectNode;
            toolStripMenuItemPreviewLeft.Enabled = bolSelectNode;
            toolStripMenuItemPreviewRight.Enabled = bolSelectNode;

            // GIF構造に追加
            toolStripMenuItemAddTreeNode.Enabled = bolSelectListViewItem;
            toolStripButtonAddTreeNode.Enabled = bolSelectListViewItem;

            // GIF展開表示
            toolStripMenuItemExpanGifFrame.Enabled = bolSelectListViewItem;
            toolStripButtonExpandGifFrame.Enabled = bolSelectListViewItem;

            // 再読込
            toolStripMenuItemReloadImage.Enabled = bolExistProjectNode;
            toolStripButtonReloadImage.Enabled = bolExistProjectNode;

            // プロパティ
            toolStripMenuItemProperty.Enabled = bolSelectNode;
            toolStripButtonProperty.Enabled = bolSelectNode;

            // GIF再生
            toolStripMenuItemPlayPreview.Enabled = bolExistProjectNode;
            toolStripButtonPreviewPlay.Enabled = bolExistProjectNode;

            // GIF停止
            toolStripMenuItemStopPreview.Enabled = false;
            toolStripButtonPreviewStop.Enabled = false;

            switch (updateMenuButtonEnabledMode)
            {
                case UpdateMenuButtonEnabledMode.OutputGif:
                    updateGifOutputMenuButtonEnabled(false);
                    break;
                case UpdateMenuButtonEnabledMode.OutputGifEnd:
                    updateGifOutputMenuButtonEnabled(true);
                    break;
                case UpdateMenuButtonEnabledMode.PlayPreview:
                    updatePreviewPlayMenuButtonEnabled(false);
                    break;
                case UpdateMenuButtonEnabledMode.StopPreview:
                    updatePreviewPlayMenuButtonEnabled(true);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// GIF出力メニューボタン更新
        /// </summary>
        /// <param name="busy"></param>
        private void updateGifOutputMenuButtonEnabled(bool enabled)
        {
            //------------------------------------------------------------

            // 新しいプロジェクト
            toolStripMenuItemNewProject.Enabled = enabled;

            // プロジェクトを開く
            toolStripMenuItemOepnProject.Enabled = enabled;

            // プロジェクト名前を付けて保存
            toolStripButtonSaveAsProject.Enabled = enabled;

            // GIF生成
            toolStripMenuItemOutputGif.Enabled = enabled;

            // GIF生成中止
            toolStripMenuItemCancelOutputGif.Enabled = !enabled;

            // 編集
            toolStripMenuItemEdit.Enabled = enabled;

            // 表示
            toolStripMenuItemView.Enabled = enabled;

            // 設定
            toolStripMenuItemConfig.Enabled = enabled;

            //------------------------------------------------------------

            // 新しいプロジェクト
            toolStripButtonNewProject.Enabled = enabled;

            // プロジェクトを開く
            toolStripButtonOpenProject.Enabled = enabled;

            // GIF生成
            toolStripButtonOutputGif.Enabled = enabled;

            // GIF生成中止
            toolStripButtonCancelOutputGif.Enabled = !enabled;

            //------------------------------------------------------------

            // 操作パネル
            flowLayoutPanelControl.Enabled = enabled;

            // プレビュー
            toolStripPreview.Enabled = enabled;
            pictureBoxPreview.Enabled = enabled;

            //------------------------------------------------------------

            // GIF構成
            toolStripTree.Enabled = enabled;
            treeViewGif.Enabled = enabled;
            dataGridViewGif.Enabled = enabled;
        }

        /// <summary>
        /// GIF再生メニューボタン更新
        /// </summary>
        /// <param name="busy"></param>
        private void updatePreviewPlayMenuButtonEnabled(bool enabled)
        {
            //------------------------------------------------------------

            // 新しいプロジェクト
            toolStripMenuItemNewProject.Enabled = enabled;

            // プロジェクト開く
            toolStripMenuItemOepnProject.Enabled = enabled;

            // プロジェクト名前を付けて保存
            toolStripMenuItemSaveAsProject.Enabled = enabled;

            // GIF出力
            toolStripMenuItemOutputGif.Enabled = enabled;

            // GIF出力中止
            toolStripMenuItemCancelOutputGif.Enabled = enabled;

            // 編集
            toolStripMenuItemEdit.Enabled = enabled;

            // GIF再生
            toolStripMenuItemPlayPreview.Enabled = enabled;

            // GIF停止
            toolStripMenuItemStopPreview.Enabled = !enabled;

            // 設定
            toolStripMenuItemConfig.Enabled = enabled;

            //------------------------------------------------------------

            // 新しいプロジェクト
            toolStripButtonNewProject.Enabled = enabled;

            // プロジェクト開く
            toolStripButtonOpenProject.Enabled = enabled;

            // プロジェクト名前を付けて保存
            toolStripButtonSaveAsProject.Enabled = enabled;

            // GIF生成
            toolStripButtonOutputGif.Enabled = enabled;

            // GIF生成中止
            toolStripButtonCancelOutputGif.Enabled = enabled;

            //------------------------------------------------------------

            // 移動量
            numericUpDownMovePixel.Enabled = enabled;

            // X座標(pixel)
            numericUpDownPosX.Enabled = enabled;

            // Y座標(pixel)
            numericUpDownPosY.Enabled = enabled;

            // GIF再生
            toolStripButtonPreviewPlay.Enabled = enabled;

            // GIF停止
            toolStripButtonPreviewStop.Enabled = !enabled;

            // プレビュー
            pictureBoxPreview.Enabled = enabled;

            //------------------------------------------------------------

            // GIF構成
            treeViewGif.Enabled = enabled;
            toolStripTree.Enabled = enabled;
            dataGridViewGif.Enabled = enabled;
        }
    }
}
