using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TANUGIF
{
    public partial class MainForm : Form
    {
 
        public MainForm()
        {
            InitializeComponent();

            numericUpDownPosY.MouseWheel += numericUpDownPosY_MouseWheel;
            pictureBoxPreview.MouseWheel += pictureBoxPreview_MouseWheel;

            splitContainerTreeData.Panel2Collapsed = true;
        }

        //------------------------------------------------------------------------------------------
        //
        // 起動時処理
        //
        //------------------------------------------------------------------------------------------
        #region 起動時処理

        /// <summary>
        /// メインフォーム起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // ウィンドウ表示状態の復元
            initWindow();

            // ライブラリ初期化
            initListViewLib();

            //メニューボタン表示状態更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // 終了時処理
        //
        //------------------------------------------------------------------------------------------
        #region 終了時処理

        /// <summary>
        /// メインフォーム終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            formClosing();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // ライブラリ画像読込
        //
        //------------------------------------------------------------------------------------------
        #region ライブラリ画像読込

        /// <summary>
        /// ライブラリ画像読込開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            initListViewLibAsync(bw);
        }

        /// <summary>
        /// ライブラリ画像読込中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            initListViewLibAsyncProgressChanged(e);
        }

        /// <summary>
        /// ライブラリ画像読込完了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initListViewLibAsyncCompleted();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // GIF生成
        //
        //------------------------------------------------------------------------------------------
        #region GIF生成

        /// <summary>
        /// GIF生成BGJ実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutputGif_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            outputGifAsync(bw);
        }

        /// <summary>
        /// GIF生成BGJ進捗報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutputGif_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            outputGifAsyncProgressChanged(e);
        }

        /// <summary>
        /// GIF生成BGJ完了報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutputGif_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            outputGifAsyncCompleted();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // メニュー
        //
        //------------------------------------------------------------------------------------------
        #region メニュー

        //------------------------------------------------------------------------------------------
        // [ファイル]
        //------------------------------------------------------------------------------------------
        #region ファイル

        //------------------------------------------------------------------------------------------
        // [ファイル]-[新しいプロジェクト]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プロジェクト新規生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNewProject_Click(object sender, EventArgs e)
        {
            createNewProject();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[プロジェクトを開く]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemOepnProject_Click(object sender, EventArgs e)
        {
            oepnProject();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[プロジェクトを保存]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プロジェクトを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemSaveProject_Click(object sender, EventArgs e)
        {
            saveProject();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[名前を付けてプロジェクトを保存]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 名前を付けてプロジェクトを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemSaveAsProject_Click(object sender, EventArgs e)
        {
            saveAsProject();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[GIF生成]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIF生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemOutputGif_Click(object sender, EventArgs e)
        {
            outputGif();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[GIF生成中止]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIF生成中止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemGifOutputCancel_Click(object sender, EventArgs e)
        {
            outputGifCancelAsync();
        }

        //------------------------------------------------------------------------------------------
        // [ファイル]-[終了]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // [編集]
        //------------------------------------------------------------------------------------------
        #region 編集

        //------------------------------------------------------------------------------------------
        // [編集]-[画像ノード追加]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 画像ノード追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemAddImageNode_Click(object sender, EventArgs e)
        {
            addImageNodeFromListView();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[フレームノード追加]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// フレームノード追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNodeAddFrameNode_Click(object sender, EventArgs e)
        {
            addFrameNode();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[フレームノード挿入]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// フレームノード挿入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemInsertFrameNode_Click(object sender, EventArgs e)
        {
            insertFrameNode_Click();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[ノード削除]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// ノード削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemRemoveNode_Click(object sender, EventArgs e)
        {
            removeNode();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[選択ノードを上へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 選択ノードを上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemMoveUpNode_Click(object sender, EventArgs e)
        {
            moveUpNode();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[選択ノードを下へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 選択ノードを下へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemMoveDownNode_Click(object sender, EventArgs e)
        {
            moveDownNode();
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[画像を上へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 画像を上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewUp_Click(object sender, EventArgs e)
        {
            moveImage(1);
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[画像を下へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 画像を下へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewDown_Click(object sender, EventArgs e)
        {
            moveImage(2);
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[画像を左へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 画像を左へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewLeft_Click(object sender, EventArgs e)
        {
            moveImage(3);
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[画像を右へ移動]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 画像を右へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewRight_Click(object sender, EventArgs e)
        {
            moveImage(4);
        }

        //------------------------------------------------------------------------------------------
        // [編集]-[プロパティ]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プロパティ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemViewPropertyDialog_Click(object sender, EventArgs e)
        {
            viewPropertyDialog();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // [表示]
        //------------------------------------------------------------------------------------------
        #region 表示

        //------------------------------------------------------------------------------------------
        // [表示]-[GIF画像を展開表示]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIF画像を展開表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemExpandGifFrame_Click(object sender, EventArgs e)
        {
            expandGifFrame();
        }

        //------------------------------------------------------------------------------------------
        // [表示]-[大アイコン]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// ライブラリ大アイコン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemLarge_Click(object sender, EventArgs e)
        {
            listViewLib.View = View.LargeIcon;
            listViewLibExpand.View = View.LargeIcon;

            toolStripMenuItemLarge.CheckState = CheckState.Checked;
            toolStripMenuItemSmall.CheckState = CheckState.Unchecked;
        }

        //------------------------------------------------------------------------------------------
        // [表示]-[小アイコン]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// ライブラリ小アイコン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemSmall_Click(object sender, EventArgs e)
        {
            listViewLib.View = View.SmallIcon;
            listViewLibExpand.View = View.SmallIcon;

            toolStripMenuItemLarge.CheckState = CheckState.Unchecked;
            toolStripMenuItemSmall.CheckState = CheckState.Checked;
        }

        //------------------------------------------------------------------------------------------
        // [表示]-[選択ノードの画像再読込]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// 選択ノードの画像再読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemReloadImage_Click(object sender, EventArgs e)
        {
            reloadImage();
        }

        //------------------------------------------------------------------------------------------
        // [表示]-[GIFアニメーション再生]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIFアニメーション再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPlayPreview_Click(object sender, EventArgs e)
        {
            playPreview();
        }

        //------------------------------------------------------------------------------------------
        // [表示]-[GIFアニメーション停止]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIFアニメーション停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemStopPreview_Click(object sender, EventArgs e)
        {
            stopPreview();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // [設定]
        //------------------------------------------------------------------------------------------
        #region 設定

        //------------------------------------------------------------------------------------------
        // [設定]-[ライブラリフォルダ一覧]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// ライブラリフォルダ一覧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemShowDialogLibFolder_Click(object sender, EventArgs e)
        {
            showDialogLibFolder();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // [バージョン情報]
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// バージョン情報
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("たぬぎふ Ver." + ProductVersion.ToString(), "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // ツールバー
        //
        //------------------------------------------------------------------------------------------
        #region ツールバー

        //------------------------------------------------------------------------------------------
        // アプリケーション ツールバー
        //------------------------------------------------------------------------------------------
        #region アプリケーション ツールバー

        /// <summary>
        /// プロジェクト新規生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonNewProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNewProject.PerformClick();
        }

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonOpenProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemOepnProject.PerformClick();
        }

        /// <summary>
        /// プロジェクトを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSaveProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSaveProject.PerformClick();
        }

        /// <summary>
        /// 名前を付けてプロジェクトを保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSaveAsProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSaveAsProject.PerformClick();
        }

        /// <summary>
        /// GIF生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonOutput_Click(object sender, EventArgs e)
        {
            toolStripMenuItemOutputGif.PerformClick();
        }

        /// <summary>
        /// GIF生成中止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonOutputCancel_Click(object sender, EventArgs e)
        {
            toolStripMenuItemCancelOutputGif.PerformClick();
        }
        #endregion

        //------------------------------------------------------------------------------------------
        // ライブラリ ツールバー
        //------------------------------------------------------------------------------------------
        #region ライブラリ ツールバー

        /// <summary>
        /// ライブラリフォルダ一覧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonShowDialogLibFolder_Click(object sender, EventArgs e)
        {
            toolStripMenuItemShowDialogLibFolder.PerformClick();
        }

        /// <summary>
        /// 画像ノード追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonAddTreeNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemAddTreeNode.PerformClick();
        }

        /// <summary>
        /// GIF画像を展開表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonExpandGifFrame_Click(object sender, EventArgs e)
        {
            toolStripMenuItemExpanGifFrame.PerformClick();
        }

        /// <summary>
        /// ライブラリ大アイコン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonLargeIcon_Click(object sender, EventArgs e)
        {
            toolStripMenuItemLarge.PerformClick();
        }

        /// <summary>
        /// ライブラリ小アイコン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSmalIcon_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSmall.PerformClick();
        }

        private int m_intTargetLibFolder = -1;

        /// <summary>
        /// ライブラリ表示対象フォルダ指定変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripComboBoxLibFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox comboBox = (ToolStripComboBox)sender;
            m_intTargetLibFolder = comboBox.SelectedIndex - 1;
            initListViewLib();
        }

        /// <summary>
        /// ライブラリ検索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonLibSearch_Click(object sender, EventArgs e)
        {
            initListViewLib();
        }

        /// <summary>
        /// ファイル名キー入力
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripTextBoxLibFilename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButtonLibSearch.PerformClick();
            }
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // GIF構成 ツールバー
        //------------------------------------------------------------------------------------------
        #region GIF構成 ツールバー

        /// <summary>
        /// フレームノード追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonAddFrameNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemAddFrameNode.PerformClick();
        }

        /// <summary>
        /// フレームノード挿入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonInsertFrameNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemInsertFrameNode.PerformClick();
        }

        /// <summary>
        /// ノード削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonRemoveNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemRemoveNode.PerformClick();
        }

        /// <summary>
        /// 選択ノードを上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonMoveUpNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemMoveUpNode.PerformClick();
        }

        /// <summary>
        /// 選択ノードを下へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonMoveDownNode_Click(object sender, EventArgs e)
        {
            toolStripMenuItemMoveDownNode.PerformClick();
        }

        /// <summary>
        /// プロパティ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonProperty_Click(object sender, EventArgs e)
        {
            toolStripMenuItemProperty.PerformClick();
        }

        /// <summary>
        /// 選択ノードの画像再読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonReloadImage_Click(object sender, EventArgs e)
        {
            toolStripMenuItemReloadImage.PerformClick();
        }

        #endregion

        //------------------------------------------------------------------------------------------
        // プレビュー ツールバー
        //------------------------------------------------------------------------------------------
        #region プレビュー ツールバー

        /// <summary>
        /// GIFアニメーション再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonPlayPreview_Click(object sender, EventArgs e)
        {
            toolStripMenuItemPlayPreview.PerformClick();
        }

        /// <summary>
        /// GIFアニメーション停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonStopPreview_Click(object sender, EventArgs e)
        {
            toolStripMenuItemStopPreview.PerformClick();
        }

        #endregion

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // ライブラリ
        //
        //------------------------------------------------------------------------------------------
        #region ライブラリ

        /// <summary>
        /// ライブラリアイコン選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            toolStripStatusLabelLib.Text = (string)e.Item.Tag;

            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
        }

        /// <summary>
        /// ライブラリアイコンダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            toolStripMenuItemAddTreeNode.PerformClick();
        }

        /// <summary>
        /// ライブラリからドラッグ可否判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>
        /// ライブラリからドラッグ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            ListViewItem listViewItem = e.Item as ListViewItem;

            treeViewGif.DoDragDrop(listViewItem, DragDropEffects.Copy);
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // GIF構成
        //
        //------------------------------------------------------------------------------------------
        #region GIF構成

        /// <summary>
        /// GIF構成へドロップ可否判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_DragEnter(object sender, DragEventArgs e)
        {
            treeViewGif_DragEnter2(sender, e);
        }

        /// <summary>
        /// GIF構成へドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_DragDrop(object sender, DragEventArgs e)
        {
            treeViewGif_DragDrop2(sender, e);
        }

        /// <summary>
        /// GIF構成ノード選択変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView1_AfterSelect2(sender, e);
        }

        /// <summary>
        /// GIF構成チェック状態変更前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            treeViewGif_BeforeCheck2(sender, e);
        }

        /// <summary>
        /// GIF構成チェック状態変更後処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_AfterCheck(object sender, TreeViewEventArgs e)
        {
            treeView_AfterCheck2(sender, e);
        }

        /// <summary>
        /// GIF構成キー押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_KeyDown(object sender, KeyEventArgs e)
        {
            treeView_KeyDown2(sender, e);
        }

        /// <summary>
        /// ラベル編集後
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewGif_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            treeViewGif_AfterLabelEdit2(sender, e);
        }
        #endregion

        //------------------------------------------------------------------------------------------
        //
        // データブラウザ
        //
        //------------------------------------------------------------------------------------------
        #region データブラウザ

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // 操作パネル
        //
        //------------------------------------------------------------------------------------------
        #region 操作パネル

        /// <summary>
        /// 遅延時間変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownDelay_ValueChanged(object sender, EventArgs e)
        {
            timerPlayPreview.Interval = (int)numericUpDownDelay.Value;
        }

        /// <summary>
        /// 移動量(pixel)値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownMovePixel_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownPosX.Increment = numericUpDownMovePixel.Value;
            numericUpDownPosY.Increment = numericUpDownMovePixel.Value;
        }

        /// <summary>
        /// 画像位置X座標(pixel)/Y座標(pixel)値変更開始時の値保存領域
        /// </summary>
        private Point numericUpDownPosXY;

        /// <summary>
        /// 画像位置X座標(pixel)/Y座標(pixel)値変更開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosXY_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY = new Point((int)numericUpDownPosX.Value, (int)numericUpDownPosY.Value);
        }

        /// <summary>
        /// 画像位置X座標(pixel)値変更開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY_Enter(sender, e);
        }

        /// <summary>
        /// 画像位置Y座標(pixel)値変更開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY_Enter(sender, e);
        }

        /// <summary>
        /// 画像位置X座標(pixel)値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_ValueChanged(object sender, EventArgs e)
        {
            changeImagePos(treeViewGif.SelectedNode);
        }

        /// <summary>
        /// 画像位置Y座標(pixel)値変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_ValueChanged(object sender, EventArgs e)
        {
            changeImagePos(treeViewGif.SelectedNode);
        }

        /// <summary>
        /// 画像位置X座標(pixel)値変更終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_Leave(object sender, EventArgs e)
        {
            numericUpDownPosXY_Leave(sender, e);
        }

        /// <summary>
        /// 画像位置Y座標(pixel)値変更終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_Leave(object sender, EventArgs e)
        {
            numericUpDownPosXY_Leave(sender, e);
        }

        #endregion

        //------------------------------------------------------------------------------------------
        //
        // プレビュー
        //
        //------------------------------------------------------------------------------------------
        #region プレビュー

        /// <summary>
        /// GIF再生中
        /// </summary>
        private void timerGifPlay_Tick(object sender, EventArgs e)
        {
            timerGifPlay_Tick2(sender, e);
        }

        bool bolUpdateControlPanel = true;

        /// <summary>
        /// プレビュー座標値変更モード開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseDown(object sender, MouseEventArgs e)
        {
            bolUpdateControlPanel = false;
            pictureBoxPreview_MouseDown2(sender, e);
        }

        /// <summary>
        /// プレビュー座標値変更処理中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBoxPreview_MouseMove2(sender, e);
        }

        /// <summary>
        /// プレビュー座標値変更処理終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseUp(object sender, MouseEventArgs e)
        {
            bolUpdateControlPanel = true;
            pictureBoxPreview_MouseUp2(sender, e);
        }

        /// <summary>
        /// プレビュー座標値変更モード終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseLeave(object sender, EventArgs e)
        {
            toolStripLabelMousePos.Text = "";
        }

        #endregion

        /// <summary>
        /// データブラウザ表示／非表示切替
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemDataBrowser_Click(object sender, EventArgs e)
        {
            splitContainerTreeData.Panel2Collapsed = !splitContainerTreeData.Panel2Collapsed;

            if (splitContainerTreeData.Panel2Collapsed)
            {

                toolStripMenuItemDataBrowser.CheckState = CheckState.Unchecked;
            }
            else
            {
                toolStripMenuItemDataBrowser.CheckState = CheckState.Checked;
            }
        }

        /// <summary>
        /// データブラウザ表示／非表示切替
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonDataowser_Click(object sender, EventArgs e)
        {
            toolStripMenuItemDataBrowser.PerformClick();
        }
    }
}
