using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static TANUGIF.ProjectFile;

namespace TANUGIF
{
    public partial class MainForm : Form
    {
        LibFolder m_libFolder;

        public MainForm()
        {
            InitializeComponent();

            numericUpDownPosY.MouseWheel += numericUpDownPosY_MouseWheel;
            pictureBoxPreview.MouseWheel += pictureBoxPreview_MouseWheel;
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
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

        //------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// メインフォーム起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            // 設定ファイル取得
            Settings.LoadFromXmlFile();
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

            initListViewLib();
        }

        /// <summary>
        /// プログラム終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 終了時処理フラグ
        bool m_bolFormClosing = false;

        /// <summary>
        /// 終了時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SuspendLayout();

            m_bolFormClosing = true;

            // ウィンドウ表示状態の保存
            Settings settings = Settings.Instance;
            settings.SplitContainerMainSplitterDistance = splitContainerMain.SplitterDistance;
            settings.SsplitContainerLibPreviewSplitterDistance = splitContainerLibPreview.SplitterDistance;
            settings.WindowState = (int)WindowState;
            WindowState = FormWindowState.Normal;
            settings.WindowLocationX = Location.X;
            settings.WindowLocationY = Location.Y;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            Settings.SaveToXmlFile();

            // ライブラリ読込中の場合、読込を中止する。
            if (backgroundWorkerLib.IsBusy)
            {
                backgroundWorkerLib.CancelAsync();
            }

            // GIF生成中の場合、GIF生成を中止する。
            if (backgroundWorkerOutput.IsBusy)
            {
                backgroundWorkerOutput.CancelAsync();
            }

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// バージョン情報
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("たぬぎふ " + ProductVersion.ToString(), "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// GIF生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonOutput_Click(object sender, EventArgs e)
        {
            // 処理中
            if (backgroundWorkerOutput.IsBusy)
            {
                return;
            }

            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            // フレームなし
            if (treeViewGif.GetNodeCount(true) == 1)
            {
                return;
            }

            int intStepCount = 0;
            foreach (TreeNode projectNode in treeViewGif.Nodes)
            {
                foreach (TreeNode frameNode in projectNode.Nodes)
                {
                    for (int ii = frameNode.GetNodeCount(false) - 1; ii >= 0; --ii)
                    {
                        ++intStepCount;
                    }
                    ++intStepCount;
                }
                ++intStepCount;
            }

            if (intStepCount > 0)
            {
                toolStripProgressBarMain.Maximum = intStepCount;
                backgroundWorkerOutput.RunWorkerAsync();
            }
        }

        /// <summary>
        /// GIF生成中止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonOutputCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerOutput.IsBusy)
            {
                backgroundWorkerOutput.CancelAsync();
            }
        }

        /// <summary>
        /// GIF生成BGJ実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutput_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            string strMessage;
            int intProgressCount = 0;

            var bmps = new List<MyGifEncorder.BitmapAndDelayTime>();
            foreach (TreeNode projectNode in treeViewGif.Nodes)
            {
                if (bw.CancellationPending)
                {
                    break;
                }

                TreeNodeTag projectNodeTag = (TreeNodeTag)projectNode.Tag;
                Bitmap projectBitmap = projectNodeTag.Bitmap;

                int intFrameIndex = 1;
                int intFrameCount = projectNode.GetNodeCount(false);
                foreach (TreeNode frameNode in projectNode.Nodes)
                {
                    if (bw.CancellationPending)
                    {
                        break;
                    }

                    Bitmap canvas = new Bitmap(projectBitmap.Width, projectBitmap.Height);
                    for (int ii = frameNode.GetNodeCount(false) - 1; ii >= 0; --ii)
                    {
                        if (bw.CancellationPending)
                        {
                            break;
                        }

                        TreeNode node = frameNode.Nodes[ii];
                        TreeNodeTag treeNodeTag = (TreeNodeTag)node.Tag;
                        Bitmap bitmap = treeNodeTag.Bitmap;
                        MyimageConverter.ImageMerge32bit(canvas, bitmap);

                        strMessage = string.Format("フレームを合成しています({0}/{1})", intFrameIndex, intFrameCount);
                        bw.ReportProgress(++intProgressCount, strMessage);

                        if (bw.CancellationPending)
                        {
                            break;
                        }
                    }
                    bmps.Add(new MyGifEncorder.BitmapAndDelayTime(canvas, (ushort)(numericUpDownDelay.Value / 10)));

                    if (bw.CancellationPending)
                    {
                        goto NORMAL_END;
                    }

                    ++intFrameIndex;
                }
                if (bw.CancellationPending)
                {
                    goto NORMAL_END;
                }

                strMessage = string.Format("256色以内に減色しています({0}/{1})", 0, intFrameCount);
                bw.ReportProgress(intProgressCount, strMessage);
                for (int ii = 0; ii < bmps.Count; ++ii)
                {
                    if (bw.CancellationPending)
                    {
                        break;
                    }

                    MyimageConverter.ImageConvert32bitTo8bit2(bmps[ii].bitmap);
                    Bitmap bitmap8bit = MyimageConverter.ImageConvert32bitTo8bit(bmps[ii].bitmap);
                    bmps[ii].bitmap.Dispose();
                    bmps[ii].bitmap = bitmap8bit;

                    strMessage = string.Format("256色以内に減色しています({0}/{1})", ii + 1, intFrameCount);
                    bw.ReportProgress(intProgressCount++, strMessage);

                    if (bw.CancellationPending)
                    {
                        break;
                    }
                }
                if (bw.CancellationPending)
                {
                    goto NORMAL_END;
                }

                strMessage = string.Format("GIFファイルを保存しています({0}/{1})", 0, 1);
                bw.ReportProgress(intProgressCount++, strMessage);

                string strOutputFilename = projectNode.Text + ".gif";
                MyGifEncorder.SaveAnimatedGif(strOutputFilename, bmps, 0);

                strMessage = string.Format("GIFファイルを保存しています({0}/{1})", 1, 1);
                bw.ReportProgress(intProgressCount++, strMessage);
            }

        NORMAL_END:
            for (int ii = 0; ii < bmps.Count; ++ii)
            {
                bmps[ii].bitmap.Dispose();
            }
        }

        /// <summary>
        /// GIF生成BGJ進捗報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutput_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBarMain.Value = e.ProgressPercentage;

            if (e.UserState != null)
            {
                toolStripStatusLabel1.Text = (string)e.UserState;
            }
        }

        /// <summary>
        /// GIF生成BGJ完了報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerOutput_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_bolFormClosing)
            {
                return;
            }

            // プログレスバー満タンの場合
            if (toolStripProgressBarMain.Value == toolStripProgressBarMain.Maximum)
            {
                toolStripStatusLabel1.Text = "GIF生成が完了しました";
            }
            // プログレスバー満タン未満の場合
            else
            {
                toolStripStatusLabel1.Text = "GIF生成を中止しました";
            }
            toolStripProgressBarMain.Value = 0;
        }

        //------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// ライブラリ初期化
        /// </summary>
        private void initListViewLib()
        {
            listViewLib.Clear();

            foreach (Image image in imageListLibLarge.Images)
            {
                image.Dispose();
            }
            imageListLibLarge.Images.Clear();

            foreach (Image image in imageListLibSmall.Images)
            {
                image.Dispose();
            }
            imageListLibSmall.Images.Clear();

            Settings settings = Settings.Instance;
            List<string> libPath = settings.LibPath;
            toolStripProgressBarLib.Maximum = libPath.Count();

            backgroundWorkerLib.RunWorkerAsync();
        }

        List<ListViewItem> lviLibListViewItems = new List<ListViewItem>();

        /// <summary>
        /// ライブラリ画像読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;
            string strMessage;
            int intLibPathIndex = 0;

            // 設定ファイル取得
            Settings settings = Settings.Instance;
            List<string> libPath = new List<string>(settings.LibPath);

            int widthLarge = settings.LibIconSizeLarge;
            int heightLarge = settings.LibIconSizeLarge;

            int widthSmall = settings.LibIconSizeSmall;
            int heightSmall = settings.LibIconSizeSmall;

            if (0 <= m_intTargetLibFolder)
            {
                string strTagetLibPath = libPath[m_intTargetLibFolder];
                libPath.Clear();
                libPath.Add(strTagetLibPath);
            }

            string strTargetLibFileName = toolStripTextBoxLibFilename.Text;
            if (strTargetLibFileName == "")
            {
                strTargetLibFileName = "*";
            }
            else
            {
                strTargetLibFileName = "*" + strTargetLibFileName + "*";
            }

            foreach (string strLibFolderPath in libPath)
            {
                if (bw.CancellationPending)
                {
                    break;
                }

                strMessage = string.Format("画像ファイルを読み込んでいます(フォルダ:{0}/{1})", ++intLibPathIndex, libPath.Count());
                bw.ReportProgress(intLibPathIndex, strMessage);

                string[] strLibFiles = System.IO.Directory.GetFiles(
                    strLibFolderPath, strTargetLibFileName, System.IO.SearchOption.AllDirectories);

                foreach (string strLibFile in strLibFiles)
                {
                    if (bw.CancellationPending)
                    {
                        break;
                    }

                    string extension = Path.GetExtension(strLibFile);
                    extension.ToLower();
                    if (extension != ".gif" && extension != ".png" && extension != ".bmp")
                    {
                        continue;
                    }

                    string strLibFileName = Path.GetFileName(strLibFile);
                    string strLibKey = strLibFile;

                    Image original = Bitmap.FromFile(strLibFile);
                    Image thumbnailLarge = TanugifCommon.createThumbnail(original, widthLarge, heightLarge);
                    Image thumbnailSmall = TanugifCommon.createThumbnail(original, widthSmall, heightSmall);

                    imageListLibLarge.Images.Add(strLibKey, thumbnailLarge);
                    imageListLibSmall.Images.Add(strLibKey, thumbnailSmall);

                    original.Dispose();
                    thumbnailLarge.Dispose();
                    thumbnailSmall.Dispose();

                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Text = strLibFileName;
                    listViewItem.ImageKey = strLibKey;
                    listViewItem.Tag = strLibFile;
                    lviLibListViewItems.Add(listViewItem);

                    if (bw.CancellationPending)
                    {
                        break;
                    }
                }

                if (bw.CancellationPending)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// LIB読込進捗報告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBarLib.Value = e.ProgressPercentage;

            if (e.UserState != null)
            {
                toolStripStatusLabelLib.Text = (string)e.UserState;
            }
        }

        /// <summary>
        /// LIB画像画面反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLib_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (m_bolFormClosing)
            {
                return;
            }

            for (int ii = 0; ii < lviLibListViewItems.Count; ++ii)
            {
                ListViewItem listViewItem = lviLibListViewItems[ii];
                listViewLib.Items.Add(listViewItem);
            }
            lviLibListViewItems.Clear();

            toolStripProgressBarLib.Value = 0;
            toolStripStatusLabelLib.Text = "";
        }

        /// <summary>
        /// ライブラリフォルダ一覧編集起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonEditLibFolder_Click(object sender, EventArgs e)
        {
            Settings settings = Settings.Instance;
            m_libFolder = new LibFolder(settings.LibPath);
            if (m_libFolder.ShowDialog() == DialogResult.OK)
            {
                settings.LibPath.Clear();
                settings.LibPath = m_libFolder.getLibFolderList();
                Settings.SaveToXmlFile();

                // ライブラリ初期化
                initListViewLib();
            }
        }

        private int m_intTargetLibFolder = -1;

        /// <summary>
        /// ライブラリ指定変更
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
        private void toolStripTextBoxLibFilename_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                toolStripButtonLibSearch.PerformClick();
            }
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
        /// ライブラリアイコン選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            toolStripStatusLabelLib.Text = (string)e.Item.Tag;
        }

        /// <summary>
        /// ライブラリ大アイコン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemLarge_Click(object sender, EventArgs e)
        {
            listViewLib.View = View.LargeIcon;
            listViewLibExpand.View = View.LargeIcon;
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
        private void toolStripMenuItemSmall_Click(object sender, EventArgs e)
        {
            listViewLib.View = View.SmallIcon;
            listViewLibExpand.View = View.SmallIcon;
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

        /// <summary>
        /// GIF展開
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonGifExpand_Click(object sender, EventArgs e)
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
            }
            else
            {
                listViewLibExpand.Items.Clear();
                listViewLib.Visible = true;
            }
        }

        /// <summary>
        /// リストビューダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 「GIF構造に追加」を起動
            toolStripButtonAddTreeNode.PerformClick();
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


        private void listViewLib_DragDrop(object sender, DragEventArgs e)
        {
        }

        //------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プロジェクト新規生成メニュー実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNewProject_Click(object sender, EventArgs e)
        {
            //プロジェクト新規生成ダイアログ表示
            NewProject newProject = new NewProject();
            if (newProject.ShowDialog() == DialogResult.OK)
            {
                if (treeViewGif.GetNodeCount(false) > 0)
                {
                    // GIF構造初期化
                    foreach (TreeNode projectNode in treeViewGif.Nodes)
                    {
                        TreeViewControl.removeNode(projectNode);
                    }
                }
                imageListTree.Images.Clear();

                // プロジェクト設定取得
                string strProjectName = newProject.getProjectName();
                Size size = newProject.getProjectImageSize();

                // GIF構造にプロジェクト追加
                TreeNode newProjectNode = addProjectNode(treeViewGif.Nodes, strProjectName, size);

                // フレーム生成
                int intFrameCount = newProject.getFrameCount();
                if (0 < intFrameCount)
                {
                    TreeViewControl.addFrameNode(newProjectNode, intFrameCount);
                }
            }
            newProject.Dispose();
        }

        /// <summary>
        /// プロジェクト新規生成ボタン実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonNewProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNewProject.PerformClick();
        }

        /// <summary>
        /// GIF構造にプロジェクト追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private TreeNode addProjectNode(TreeNodeCollection treeNodeCollection, string strProjectName, Size size)
        {
            // プロジェクトアイコン取得
            Bitmap canvas = new Bitmap(size.Width, size.Height);
            toolStripLabelImageSize.Text = "縦:" + size.Width.ToString() + "(pixel) 横:" + size.Height + "(pixel)";

            if (!imageListTree.Images.ContainsKey(strProjectName))
            {
                // プロジェクトアイコン(サムネイル)取得
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = TanugifCommon.createThumbnail(canvas, width, height);

                // プロジェクトアイコン(サムネイル)設定
                imageListTree.ImageSize = new Size(width, height);
                imageListTree.Images.Add(strProjectName, thumbnail);
            }

            // GIF構造プロジェクト追加
            return TreeViewControl.addProjectNode(treeNodeCollection, strProjectName, canvas);
        }

        /// <summary>
        /// GIF構造に追加ボタン実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonAddTreeNode_Click(object sender, EventArgs e)
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
        /// GIF構成へドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            TreeNode targetNode = null;

            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                // GIF構造にプロジェクト追加
                targetNode = addProjectNode(treeViewGif.Nodes, "DefaultProject", new Size(500, 500));
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
        }

        /// <summary>
        /// 画像追加
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
        /// 画像をフレームに追加
        /// </summary>
        /// <param name="frameNode"></param>
        /// <param name="srcItem"></param>
        private void addImageNodeToFrame(TreeNode frameNode, string name, string strFilePath, int intFrameNumber, Point point = new Point())
        {
            if (!File.Exists(strFilePath))
            {
                return;
            }

            string strFileName = Path.GetFileName(strFilePath);
            string strImageKey = strFilePath;
            string strImageName = strFileName;

            Image image = Image.FromFile(strFilePath);
            FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
            int intFrameCount = image.GetFrameCount(frameDimension);
            if (intFrameCount > 1)
            {
                image.SelectActiveFrame(frameDimension, intFrameNumber - 1);
                strImageName = strFileName + ":" + intFrameNumber.ToString();
                strImageKey = strFilePath + ":" + intFrameNumber.ToString();
            }

            if (!imageListTree.Images.ContainsKey(strImageKey))
            {
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = TanugifCommon.createThumbnail(image, width, height);
                imageListTree.Images.Add(strImageKey, thumbnail);
            }

            TreeViewControl.addImageNode(frameNode, name, strImageKey, strImageName, image, strFilePath, intFrameNumber, point);

            image.Dispose();
        }

        /// <summary>
        /// プロジェクトファイル名
        /// </summary>
        private string strProjectFileName = "";

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemOepnProject_Click(object sender, EventArgs e)
        {
            if (openFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                treeViewGif.SuspendLayout();

                if (treeViewGif.GetNodeCount(false) > 0)
                {
                    // GIF構造初期化
                    TreeViewControl.removeNode(treeViewGif.Nodes[0]);

                    // プレビュー初期化
                    if (pictureBoxPreview.Image != null)
                    {
                        pictureBoxPreview.Image.Dispose();
                        pictureBoxPreview.Image = null;
                    }
                }
                imageListTree.Images.Clear();

                string strFileName = openFileDialogProject.FileName;

                ProjectFile.LoadFromXmlFile(strFileName);

                ProjectFile projectFile = ProjectFile.Instance;

                foreach (Project project in projectFile.ProjectList)
                {
                    string strProjectName = project.Name;
                    Size size = new Size(project.Width, project.Height);
                    TreeNode nodeProject = addProjectNode(treeViewGif.Nodes, strProjectName, size);
                    if (nodeProject == null)
                    {
                        break;
                    }

                    foreach (Frame frame in project.FrameList)
                    {
                        TreeNode nodeFrame = TreeViewControl.addFrameNode(nodeProject, frame.Name);
                        if (nodeFrame == null)
                        {
                            break;
                        }

                        foreach (FrameImage frameImage in frame.FrameImageList)
                        {
                            addImageNodeToFrame(nodeFrame, frameImage.Name, frameImage.FilePath, frameImage.FrameNumber, frameImage.Point);
                        }
                    }
                    nodeProject.Expand();
                }

                treeViewGif.ResumeLayout(false);
                treeViewGif.PerformLayout();

                strProjectFileName = strFileName;

                projectFile.Clear();
            }
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
        /// プロジェクト保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemSaveProject_Click(object sender, EventArgs e)
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            if (strProjectFileName == "")
            {
                toolStripButtonSaveAsProject.PerformClick();
            }
            else
            {
                string strFileName = strProjectFileName;

                ProjectFile projectFile = ProjectFile.Instance;
                projectFile.Clear();

                foreach (TreeNode nodeProject in treeViewGif.Nodes)
                {
                    TreeNodeTag projectNodeTag = (TreeNodeTag)nodeProject.Tag;
                    Image image = projectNodeTag.Bitmap;

                    Project project = new Project();
                    project.Name = nodeProject.Text;
                    project.Height = image.Height;
                    project.Width = image.Width;

                    foreach (TreeNode nodeFrame in nodeProject.Nodes)
                    {
                        Frame frame = new Frame();
                        frame.Name = nodeFrame.Text;

                        foreach (TreeNode nodeImage in nodeFrame.Nodes)
                        {
                            TreeNodeTag treeNodeTag = (TreeNodeTag)nodeImage.Tag;
                            FrameImage frameImage = new FrameImage();
                            frameImage.Name = nodeImage.Text;
                            frameImage.FilePath = treeNodeTag.FilePath;
                            frameImage.FrameNumber = treeNodeTag.FrameNumber;
                            frameImage.Point = treeNodeTag.Point;
                            frame.Add(frameImage);
                        }

                        project.Add(frame);
                    }

                    projectFile.Add(project);
                }

                ProjectFile.SaveToXmlFile(strFileName);

                projectFile.Clear();
            }
        }

        /// <summary>
        /// プロジェクト保存
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
        private void toolStripMenuItemSaveAsProject_Click(object sender, EventArgs e)
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            if (saveFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                strProjectFileName = saveFileDialogProject.FileName;
                toolStripMenuItemSaveProject.PerformClick();
            }
        }

        /// <summary>
        /// プロジェクト名前を付けて保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSaveAsProject_Click(object sender, EventArgs e)
        {
            toolStripMenuItemSaveAsProject.PerformClick();
        }

        /// <summary>
        /// GIF構成へドロップ可否判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewLib_DragEnter(object sender, DragEventArgs e)
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
        /// GIF構成要素選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            TreeNode selectedNode = e.Node;
            TreeNodeTag treeNodeTag = selectedNode.Tag as TreeNodeTag;

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowserWrap(selectedNode);

            // プレビュー更新
            pictureBox_Redraw(selectedNode);
        }

        /// <summary>
        /// データブラウザ更新
        /// </summary>
        /// <param name="selectedNode"></param>
        private void updateDataBrowserWrap(TreeNode selectedNode)
        {
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
                if(number == 0)
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

        /// <summary>
        /// コントロールパネル更新
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

                foreach (TreeNode node in selectedNode.Nodes)
                {
                    updateControlPanel(node);
                    break;
                }
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
        /// GIF構成チェック切替
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
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

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowserWrap(selectedNode);

            // プレビュー更新
            pictureBox_Redraw(selectedNode);
         }

        /// <summary>
        /// GIF構成上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemTreeUp_Click(object sender, EventArgs e)
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
        /// GIF構成上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonUp_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNodeUp.PerformClick();
        }

        /// <summary>
        /// GIF構成下へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemTreeDown_Click(object sender, EventArgs e)
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
        /// GIF構成下へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonDown_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNodeDown.PerformClick();
        }

        /// <summary>
        /// GIF構成フレーム追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNodeAdd_Click(object sender, EventArgs e)
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
        /// GIF構成フレーム追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNodeAdd.PerformClick();
        }

        /// <summary>
        /// GIF構成フレーム挿入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNodeInsert_Click(object sender, EventArgs e)
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
        /// GIF構成フレーム挿入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonInsert_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNodeInsert.PerformClick();
        }

        /// <summary>
        /// GIF構成削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemNodeDel_Click(object sender, EventArgs e)
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
            GC.Collect();
        }

        /// <summary>
        /// GIF構成削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonRemove_Click(object sender, EventArgs e)
        {
            toolStripMenuItemNodeDel.PerformClick();
        }

        /// <summary>
        /// GIF構成キー操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                toolStripButtonRemove.PerformClick();
            }
            else if (e.Control && e.KeyCode == Keys.Up)
            {
                toolStripButtonUp.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Down)
            {
                toolStripButtonDown.PerformClick();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 再読込
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonReloadImage_Click(object sender, EventArgs e)
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
            updateDataBrowserWrap(selectedNode);

            // プレビュー更新
            pictureBox_Redraw(selectedNode);
        }

        //------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------

        /// <summary>
        /// プレビュー再描画
        /// </summary>
        /// <param name="selectedNode"></param>
        private void pictureBox_Redraw(TreeNode selectedNode)
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
                updateDataBrowserWrap(selectedNode);

                // プレビュー更新
                // フレーム選択時の動作とする。
                pictureBox_Redraw(selectedNode.Parent);
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
        /// GIF再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPreviewPlay_Click(object sender, EventArgs e)
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            m_intFrameCount = 0;
            treeViewGif.Enabled = false;
            timerGifPlay.Interval = (int)numericUpDownDelay.Value;
            timerGifPlay.Start();
        }

        /// <summary>
        /// GIF再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            m_intFrameCount = 0;
            treeViewGif.Enabled = false;
            timerGifPlay.Interval = (int)numericUpDownDelay.Value;
            timerGifPlay.Start();
        }

        /// <summary>
        /// GIF停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPreviewStop_Click(object sender, EventArgs e)
        {
            timerGifPlay.Stop();
            treeViewGif.Enabled = true;
        }

        /// <summary>
        /// GIF停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            timerGifPlay.Stop();
            treeViewGif.Enabled = true;

            TreeNode selectedNode = treeViewGif.SelectedNode;
            if(selectedNode == null)
            {
                return;
            }

            // コントロールパネル更新
            updateControlPanel(selectedNode);

            // 属性パネル更新
            updateDataBrowserWrap(selectedNode);

            // プレビュー更新
            pictureBox_Redraw(selectedNode);
        }

        /// <summary>
        /// 遅延時間変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownDelay_ValueChanged(object sender, EventArgs e)
        {
            timerGifPlay.Interval = (int)numericUpDownDelay.Value;
        }

        /// <summary>
        /// GIF再生中
        /// </summary>
        int m_intFrameCount;
        private void timerGifPlay_Tick(object sender, EventArgs e)
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

        /// <summary>
        /// プレビュー上移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewUp_Click(object sender, EventArgs e)
        {
            moveImage(1);
        }

        /// <summary>
        /// プレビュー下移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewDown_Click(object sender, EventArgs e)
        {
            moveImage(2);
        }

        /// <summary>
        /// プレビュー左移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewLeft_Click(object sender, EventArgs e)
        {
            moveImage(3);
        }

        /// <summary>
        /// プレビュー右移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemPreviewRight_Click(object sender, EventArgs e)
        {
            moveImage(4);
        }

        /// <summary>
        /// プレビュー座標値変更(メニュー)
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
            updateDataBrowserWrap(selectedNode);

            // プレビュー再描画
            pictureBox_Redraw(selectedNode);
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

        // プレビュー座標値記録用
        Point m_mousePoint;

        /// <summary>
        /// プレビュー座標値変更モード開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseDown(object sender, MouseEventArgs e)
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

                //numericUpDownPosX.Enabled = false;
                //numericUpDownPosY.Enabled = false;
            }
        }

        /// <summary>
        /// プレビュー座標値変更処理中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
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

                // コントロールパネル更新
                //updateControlPanel(selectedNode);

                // 属性パネル更新
                //updateDataBrowserWrap(selectedNode);

                // プレビュー再描画
                pictureBox_Redraw(selectedNode);

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

        bool bolUpdateControlPanel = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxPreview_MouseUp(object sender, MouseEventArgs e)
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

            //numericUpDownPosX.Enabled = true;
            //numericUpDownPosY.Enabled = true;

            // コントロールパネル更新
            bolUpdateControlPanel = false;
            updateControlPanel(selectedNode);
            bolUpdateControlPanel = true;

            // 属性パネル更新
            updateDataBrowserWrap(selectedNode);
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

        /// <summary>
        /// プロパティ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonProperty_Click(object sender, EventArgs e)
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
                    updateDataBrowserWrap(selectedNode);

                    // プレビュー再描画
                    pictureBox_Redraw(selectedNode);

                    toolStripLabelImageSize.Text = "縦:" + height.ToString() + "(pixel) 横:" + width + "(pixel)";
                }
            }
        }

        /// <summary>
        /// 出力サイズ取得
        /// </summary>
        /// <returns></returns>
        private Size getOutputSize()
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return new Size(0, 0);
            }

            TreeNode projectNode = treeViewGif.Nodes[0];
            TreeNodeTag treeNodeTag = projectNode.Tag as TreeNodeTag;
            return treeNodeTag.Bitmap.Size;
        }

        /// <summary>
        /// 画像位置X座標/Y座標変更
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
            pictureBox_Redraw(selectedNode);
        }

        /// <summary>
        /// 画像位置X座標変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_ValueChanged(object sender, EventArgs e)
        {
            changeImagePos(treeViewGif.SelectedNode);
        }

        /// <summary>
        /// 画像位置Y座標変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_ValueChanged(object sender, EventArgs e)
        {
            changeImagePos(treeViewGif.SelectedNode);
        }

        private Point numericUpDownPosXY;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosXY_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY = new Point((int)numericUpDownPosX.Value, (int)numericUpDownPosY.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY_Enter(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_Enter(object sender, EventArgs e)
        {
            numericUpDownPosXY_Enter(sender, e);
        }

        /// <summary>
        /// 
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
            updateDataBrowserWrap(selectedNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosX_Leave(object sender, EventArgs e)
        {
            numericUpDownPosXY_Leave(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownPosY_Leave(object sender, EventArgs e)
        {
            numericUpDownPosXY_Leave(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownMovePixel_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownPosX.Increment = numericUpDownMovePixel.Value;
            numericUpDownPosY.Increment = numericUpDownMovePixel.Value;
        }
    }
}
