using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using static TANUGIF.ProjectFile;

namespace TANUGIF
{
    public partial class MainForm : Form
    {
        LibFolder m_libFolder;

        public MainForm()
        {
            InitializeComponent();
        }

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
        /// ライブラリフォルダ一覧編集
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
            if(e.KeyCode == Keys.Enter)
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
                if( bw.CancellationPending)
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
                    Image thumbnailLarge = createThumbnail(original, widthLarge, heightLarge);
                    Image thumbnailSmall = createThumbnail(original, widthSmall, heightSmall);

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

            if(e.UserState != null)
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
        /// サムネイル生成
        /// </summary>
        /// <param name="image"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        Image createThumbnail(Image image, int w, int h)
        {
            Bitmap canvas = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(canvas);
            graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, w, h);

            float fw = (float)w / (float)image.Width;
            float fh = (float)h / (float)image.Height;

            float scale = Math.Min(fw, fh);
            fw = image.Width * scale;
            fh = image.Height * scale;

            graphics.DrawImage(image, (w - fw) / 2, (h - fh) / 2, fw, fh);
            graphics.Dispose();

            return canvas;
        }

        /// <summary>
        /// プロジェクト新規生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewProjectMenuItem_Click(object sender, EventArgs e)
        {
            //プロジェクト新規生成ダイアログ表示
            NewProject newProject = new NewProject();
            if (newProject.ShowDialog() == DialogResult.OK)
            {
                if(treeView.GetNodeCount(false) > 0) {
                    // GIF構造初期化
                    removeNode(treeView.Nodes[0]);
                }
                imageListTree.Images.Clear();

                // プロジェクト設定取得
                string strProjectName = newProject.getProjectName();
                Size size = newProject.getProjectImageSize();

                // GIF構造にプロジェクト追加
                createProject(strProjectName, size);

                // フレーム生成
                int intFrameCount = newProject.getFrameCount();
                if (0 < intFrameCount)
                {
                    foreach (TreeNode node in treeView.Nodes.Find(strProjectName, false))
                    {
                        createFrame(node, intFrameCount);
                    }
                }
            }
            newProject.Dispose();
        }

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
        /// GIF構造にプロジェクト追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private TreeNode createProject(string strProjectName, Size size)
        {
            TreeNode treeNode = null;

            // プロジェクトアイコン取得
            Bitmap canvas = new Bitmap(size.Width, size.Height);
            toolStripLabelImageSize.Text = "縦:" + size.Width.ToString() + "(pixel) 横:" + size.Height + "(pixel)";

            if (!imageListTree.Images.ContainsKey(strProjectName))
            {
                // プロジェクトアイコン(サムネイル)取得
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = createThumbnail(canvas, width, height);

                // プロジェクトアイコン(サムネイル)設定
                imageListTree.ImageSize = new Size(width, height);
                imageListTree.Images.Add(strProjectName, thumbnail);
            }

            // GIF構造プロジェクト追加
            treeView.Nodes.Add(strProjectName, strProjectName);
            foreach (TreeNode node in treeView.Nodes.Find(strProjectName, false))
            {
                TreeNodeTag treeNodeTag = new TreeNodeTag(canvas, "", 0);
                node.ImageKey = strProjectName;
                node.SelectedImageKey = strProjectName;
                node.StateImageKey = strProjectName;
                node.Checked = true;
                node.Tag = treeNodeTag;
                treeNode = node;
                break;
            }

            return treeNode;
        }

        /// <summary>
        /// GIF構造に追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonAddTreeNode_Click(object sender, EventArgs e)
        {
            if (treeView.GetNodeCount(false) == 0)
            {
                // GIF構造にプロジェクト追加
                createProject("DefaultProject", new Size(500, 500));
            }

            TreeView tv = treeView;
            TreeNode target = tv.SelectedNode;
            if (target == null)
            {
                target = tv.Nodes[0];
            }

            if (listViewLib.Visible)
            {
                foreach (ListViewItem srcItem in listViewLib.SelectedItems)
                {
                    if (target.Parent == null)
                    {
                        // フレーム生成
                        createFrame(target, srcItem);

                        // フレームにイメージ追加
                        addImageToProject(target, srcItem);
                    }
                    else if (target.Parent == tv.Nodes[0])
                    {
                        // フレームにイメージ追加
                        addImageToFrame(target, srcItem);
                    }
                }
            }
            else
            {
                foreach (ListViewItem srcItem in listViewLibExpand.SelectedItems)
                {
                    if (target.Parent == null)
                    {

                    }
                    else if (target.Parent == tv.Nodes[0])
                    {
                        // フレームにイメージ追加
                        addImageToFrame(target, srcItem);
                    }
                }
            }
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
                treeView.SuspendLayout();

                if (treeView.GetNodeCount(false) > 0)
                {
                    // GIF構造初期化
                    removeNode(treeView.Nodes[0]);

                    // プレビュー初期化
                    if ( pictureBox1.Image != null)
                    {
                        pictureBox1.Image.Dispose();
                        pictureBox1.Image = null;
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
                    TreeNode nodeProject = createProject(strProjectName, size);
                    if (nodeProject == null)
                    {
                        break;
                    }

                    foreach (Frame frame in project.FrameList)
                    {
                        TreeNode nodeFrame = createFrame(nodeProject, frame.Name);
                        if (nodeFrame == null)
                        {
                            break;
                        }

                        foreach (FrameImage frameImage in frame.FrameImageList)
                        {
                            addImageToFrame(nodeFrame, frameImage.FilePath, frameImage.FrameNumber, frameImage.Point);
                        }
                    }
                    nodeProject.Expand();
                }

                treeView.ResumeLayout(false);
                treeView.PerformLayout();

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
            if (treeView.GetNodeCount(false) == 0)
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

                foreach (TreeNode nodeProject in treeView.Nodes)
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
            if (treeView.GetNodeCount(false) == 0)
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
        /// リストビューダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewLib_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // GIF構造に追加を起動
            toolStripButtonAddTreeNode.PerformClick();
        }

        /// <summary>
        /// フレーム生成
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void createFrame(TreeNode target, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            if (!File.Exists(strFilePath))
            {
                return;
            }

            Image imgInputImage = Image.FromFile(strFilePath);
            FrameDimension fdimInputImage = new FrameDimension(imgInputImage.FrameDimensionsList[0]);
            int intFrameCount = imgInputImage.GetFrameCount(fdimInputImage);
            createFrame(target, intFrameCount);
            imgInputImage.Dispose();
        }

        /// <summary>
        /// フレーム生成
        /// </summary>
        /// <param name="nodeProject"></param>
        /// <param name="srcItem"></param>
        private TreeNode createFrame(TreeNode nodeProject, string strFrameKey)
        {
            TreeNode nodeFrame = null;

            if (!nodeProject.Nodes.ContainsKey(strFrameKey))
            {
                nodeProject.Nodes.Add(strFrameKey, strFrameKey);
            }

            foreach (TreeNode node in nodeProject.Nodes.Find(strFrameKey, false))
            {
                node.Checked = true;
                node.Expand();
                nodeFrame = node;
                break;
            }

            return nodeFrame;
        }

        /// <summary>
        /// フレーム生成
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void createFrame(TreeNode target, int intFrameCount)
        {
            int intCreateFrameCount = 0;
            for (int ii = 1; ii <= intFrameCount; ++ii)
            {
                string strFrameKey = "フレーム" + ii.ToString();
                if (!target.Nodes.ContainsKey(strFrameKey))
                {
                    target.Nodes.Add(strFrameKey, strFrameKey);
                    foreach (TreeNode node in target.Nodes.Find(strFrameKey, false))
                    {
                        node.Checked = true;
                        node.Expand();
                        intCreateFrameCount++;
                    }
                }
            }
            if (0 < intCreateFrameCount)
            {
                target.Expand();
            }
        }

        /// <summary>
        /// GIF構造に追加(プロジェクト)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToProject(TreeNode target, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            string strFileName = srcItem.Text;

            if (!File.Exists(strFilePath))
            {
                return;
            }

            Image image = Image.FromFile(strFilePath);
            FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
            int intFrameCount = image.GetFrameCount(frameDimension);
            if (intFrameCount == 1)
            {
                addImageToProject(target, image, strFilePath);
            }
            else
            {
                addImageToProject(target, image, frameDimension, strFilePath);
            }
            image.Dispose();
        }

        /// <summary>
        /// GIF構造に追加(プロジェクト)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToProject(TreeNode target, Image image, string strFilePath)
        {
            string strImageKey = strFilePath;
            string strFileName = Path.GetFileName(strFilePath);

            if (!imageListTree.Images.ContainsKey(strImageKey))
            {
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = createThumbnail(image, width, height);

                imageListTree.Images.Add(strImageKey, thumbnail);
            }

            foreach (TreeNode projectNode in treeView.Nodes)
            {
                foreach (TreeNode frameNode in projectNode.Nodes)
                {
                    frameNode.Nodes.Add(strImageKey, strFileName);

                    foreach (TreeNode node2 in frameNode.Nodes.Find(strImageKey, false))
                    {
                        TreeNodeTag treeNodeTag = new TreeNodeTag(new Bitmap(image), strFilePath, 1);
                        node2.ImageKey = strImageKey;
                        node2.SelectedImageKey = strImageKey;
                        node2.StateImageKey = strImageKey;
                        node2.Tag = treeNodeTag;
                        node2.Checked = true;
                    }
                }
            }
        }

        /// <summary>
        /// GIF構造に追加(プロジェクト)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToProject(TreeNode target, Image image, FrameDimension fdimInputImage, string strFilePath)
        {
            string strFileName = Path.GetFileName(strFilePath);

            int intFrameCount = image.GetFrameCount(fdimInputImage);
            for (int ii = 0; ii < intFrameCount; ++ii)
            {
                int intFrameNumber = ii + 1;
                image.SelectActiveFrame(fdimInputImage, ii);

                string strImageName;
                strImageName = strFileName + ":" + intFrameNumber.ToString();

                string strImageKey;
                strImageKey = strFilePath + ":" + intFrameNumber.ToString();

                if (!imageListTree.Images.ContainsKey(strImageKey))
                {
                    Settings settings = Settings.Instance;
                    int width = settings.TreeIconSize;
                    int height = settings.TreeIconSize;
                    Image thumbnail = createThumbnail(image, width, height);

                    imageListTree.Images.Add(strImageKey, thumbnail);
                }
                // フレーム検索
                string strFrameKey = "フレーム" + intFrameNumber.ToString();
                foreach (TreeNode nodeFrame in target.Nodes.Find(strFrameKey, false))
                {
                    nodeFrame.Nodes.Add(strImageKey, strImageName);
                    foreach (TreeNode nodeImage in nodeFrame.Nodes.Find(strImageKey, false))
                    {
                        TreeNodeTag treeNodeTag = new TreeNodeTag(new Bitmap(image), strFilePath, intFrameNumber);
                        nodeImage.ImageKey = strImageKey;
                        nodeImage.SelectedImageKey = strImageKey;
                        nodeImage.StateImageKey = strImageKey;
                        nodeImage.Tag = treeNodeTag;
                        nodeImage.Checked = true;
                    }
                    nodeFrame.Expand();
                }
            }
        }

        /// <summary>
        /// GIF構造に追加(フレーム)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToFrame(TreeNode target, string strFilePath, int intTargetFrameCount, Point point = new Point())
        {
            if (!File.Exists(strFilePath))
            {
                return;
            }

            string strFileName = strFilePath;
            Image image = Image.FromFile(strFilePath);
            FrameDimension frameDimension = new FrameDimension(image.FrameDimensionsList[0]);
            int intFrameCount = image.GetFrameCount(frameDimension);
            if (intFrameCount == 1)
            {
                addImageToFrame(target, image, strFilePath, point);
            }
            else
            {
                addImageToFrame(target, image, frameDimension, strFilePath, intTargetFrameCount, point);
            }
            image.Dispose();
        }

        /// <summary>
        /// GIF構造に追加(フレーム)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToFrame(TreeNode target, ListViewItem srcItem)
        {
            string strFilePath = (string)srcItem.Tag;
            string strFileName = srcItem.Text;
            if (!File.Exists(strFilePath))
            {
                return;
            }

            string[] strFileNameSplit = strFileName.Split(':');
            int intTargetFrameCount = 1;
            if (strFileNameSplit.Length > 1)
            {
                intTargetFrameCount = int.Parse(strFileNameSplit[1]);
            }
            addImageToFrame(target, strFilePath, intTargetFrameCount);
        }

        /// <summary>
        /// GIF構造に追加(フレーム)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToFrame(TreeNode nodeFrame, Image image, string strFilePath, Point point = new Point())
        {
            string strImageKey = strFilePath;
            string strFileName = Path.GetFileName(strFilePath);

            if (!imageListTree.Images.ContainsKey(strImageKey))
            {
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = createThumbnail(image, width, height);

                imageListTree.Images.Add(strImageKey, thumbnail);
            }

            Bitmap canvas = new Bitmap(image.Width, image.Height);
            Graphics graphics = Graphics.FromImage(canvas);
            graphics.DrawImage(image, point.X, point.Y, image.Width, image.Height);

            nodeFrame.Nodes.Add(strImageKey, strFileName);
            foreach (TreeNode node2 in nodeFrame.Nodes.Find(strImageKey, false))
            {
                TreeNodeTag treeNodeTag = new TreeNodeTag(canvas, strFilePath, 1);
                node2.ImageKey = strImageKey;
                node2.SelectedImageKey = strImageKey;
                node2.StateImageKey = strImageKey;
                node2.Checked = true;
                node2.Tag = treeNodeTag;
            }

            graphics.Dispose();
        }

        /// <summary>
        /// GIF構造に追加(フレーム)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcItem"></param>
        private void addImageToFrame(TreeNode nodeFrame, Image image, FrameDimension fdimInputImage, string strFilePath, int intFrameNumber, Point point = new Point())
        {
            string strFileName = Path.GetFileName(strFilePath);

            image.SelectActiveFrame(fdimInputImage, intFrameNumber - 1);

            string strImageName;
            strImageName = strFileName + ":" + intFrameNumber.ToString();

            string strImageKey;
            strImageKey = strFilePath + ":" + intFrameNumber.ToString();

            if (!imageListTree.Images.ContainsKey(strImageKey))
            {
                Settings settings = Settings.Instance;
                int width = settings.TreeIconSize;
                int height = settings.TreeIconSize;
                Image thumbnail = createThumbnail(image, width, height);
                imageListTree.Images.Add(strImageKey, thumbnail);
            }

            Bitmap canvas = new Bitmap(image.Width, image.Height);
            Graphics graphics = Graphics.FromImage(canvas);
            graphics.DrawImage(image, point.X, point.Y, image.Width, image.Height);

            nodeFrame.Nodes.Add(strImageKey, strImageName);
            foreach (TreeNode nodeImage in nodeFrame.Nodes.Find(strImageKey, false))
            {
                TreeNodeTag treeNodeTag = new TreeNodeTag(canvas, strFilePath, intFrameNumber);
                nodeImage.ImageKey = strImageKey;
                nodeImage.SelectedImageKey = strImageKey;
                nodeImage.StateImageKey = strImageKey;
                nodeImage.Tag = treeNodeTag;
                nodeImage.Checked = true;
            }
            nodeFrame.Expand();

            graphics.Dispose();
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
            if(e.Item == null)
            {
                return;
            }

            treeView.DoDragDrop((ListViewItem)e.Item, DragDropEffects.Copy);
        }

        /// <summary>
        /// GIF構成へドロップ可否判定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewLib_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data == null)
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
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            if(e.Data == null)
            {
                return;
            }

            if (treeView.GetNodeCount(false) == 0)
            {
                // GIF構造にプロジェクト追加
                createProject("DefaultProject", new Size(500, 500));
            }

            TreeView tv = (TreeView)sender;
            TreeNode target = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));
            if (target == null)
            {
                target = tv.Nodes[0];
            }

            ListViewItem srcItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

            // ドロップ先がプロジェクトの場合
            if (target.Parent == null)
            {
                // フレーム生成
                createFrame(target, srcItem);

                // フレームにイメージ追加
                addImageToProject(target, srcItem);
            }
            // ドロップ先がフレームの場合
            else if (target.Parent.Parent == null)
            {
                // フレームにイメージ追加
                addImageToFrame(target, srcItem);
            }

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
            listViewLib.View = View.LargeIcon;
            listViewLibExpand.View = View.LargeIcon;
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
        }

        /// <summary>
        /// GIF展開
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonGifExpand_Click(object sender, EventArgs e)
        {
            if(listViewLib.Visible)
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
                            Image thumbnailLarge = createThumbnail(image, widthLarge, heightLarge);
                            Image thumbnailSmall = createThumbnail(image, widthSmall, heightSmall);
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
        /// GIF構成要素選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if(e.Node == null)
            {
                return;
            }

            TreeNode selectedNode = e.Node;

            // フレームなしの場合処理なし
            if (treeView.Nodes[0] == selectedNode)
            {
                if (selectedNode.Nodes.Count == 0)
                {
                    return;
                }
            }

            pictureBox_Redraw(selectedNode);
        }

        /// <summary>
        /// GIF構成チェック切替
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // 未選択状態は処理なし
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null)
            {
                return;
            }

            // フレームなしの場合処理なし
            if (treeView.Nodes[0] == selectedNode)
            {
                if (selectedNode.GetNodeCount(false) == 0)
                {
                    return;
                }
            }

            pictureBox_Redraw(selectedNode);
        }

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
                // フレーム選択時の動作とする。
                pictureBox_Redraw(selectedNode.Parent);
                return;
            }

            TreeNodeTag projectNodeTag = (TreeNodeTag)treeView.Nodes[0].Tag;
            Bitmap canvas = new Bitmap(projectNodeTag.Bitmap);
            Graphics graphics = Graphics.FromImage(canvas);

            // プロジェクト選択時 or フレーム選択時
            if (selectedNode == treeView.Nodes[0] || selectedNode.Parent == treeView.Nodes[0])
            {
                // フレーム選択時(デフォルト)
                TreeNode nodeFrame = selectedNode;

                // プロジェクト選択時
                if (selectedNode == treeView.Nodes[0])
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

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            pictureBox1.Image = canvas;
        }

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
            if(treeView.GetNodeCount(false) == 0)
            {
                return;
            }

            // フレームなし
            if (treeView.GetNodeCount(true) == 1)
            {
                return;
            }

            int intStepCount = 0;
            foreach (TreeNode projectNode in treeView.Nodes)
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
            foreach (TreeNode projectNode in treeView.Nodes)
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

                    strMessage = string.Format("256色以内に減色しています({0}/{1})", ii+1, intFrameCount);
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

            if(e.UserState != null)
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

        /// <summary>
        /// GIF構成上へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemTreeUp_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;

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
            treeView.BeginUpdate();

            for (int ii = 1; ii < intNodeCount; ++ii)
            {
                if (parentNode.Nodes[ii] == selectedNode)
                {
                    parentNode.Nodes.Remove(selectedNode);
                    parentNode.Nodes.Insert(ii - 1, selectedNode);
                    treeView.SelectedNode = selectedNode;
                    break;
                }
            }

            // 要素移動終了
            treeView.EndUpdate();
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
            TreeNode selectedNode = treeView.SelectedNode;

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
            treeView.BeginUpdate();

            for (int ii = 0; ii < intNodeCount - 1; ++ii)
            {
                if (parentNode.Nodes[ii] == selectedNode)
                {
                    parentNode.Nodes.Remove(selectedNode);
                    parentNode.Nodes.Insert(ii + 1, selectedNode);
                    treeView.SelectedNode = selectedNode;
                    break;
                }
            }

            // 要素移動終了
            treeView.EndUpdate();
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
            TreeNode selectedNode = treeView.SelectedNode;

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
            treeView.BeginUpdate();

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
                    treeView.SelectedNode = createNode;
                }
            }

            // 要素追加終了
            treeView.EndUpdate();
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
            TreeNode selectedNode = treeView.SelectedNode;

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
            treeView.BeginUpdate();

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
                    treeView.SelectedNode = createNode;
                }
            }

            // 要素追加終了
            treeView.EndUpdate();
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
            TreeNode selectedNode = treeView.SelectedNode;

            // 削除なし
            if (selectedNode == null)
            {
                return;
            }

            TreeNode parentNode = selectedNode.Parent;

            // プロジェクト削除
            if (parentNode == null)
            {
                treeView.BeginUpdate();

                removeNode(selectedNode);

                treeView.EndUpdate();

                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }

                GC.Collect();
                return;
            }

            // フレーム、イメージ削除
            int intNodeCount = parentNode.GetNodeCount(false);
            for (int ii = 0; ii < intNodeCount; ii++)
            {
                if (parentNode.Nodes[ii] == selectedNode)
                {
                    treeView.BeginUpdate();

                    removeNode(selectedNode);

                    if (ii == 0)
                    {
                        if (intNodeCount == 1)
                        {
                            treeView.SelectedNode = parentNode;
                        }
                        else
                        {
                            treeView.SelectedNode = parentNode.Nodes[ii];
                        }
                    }
                    else
                    {
                        treeView.SelectedNode = parentNode.Nodes[ii - 1];
                    }

                    treeView.EndUpdate();
                    break;
                }
            }

            GC.Collect();
        }

        /// <summary>
        /// GIF構成削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeNode(TreeNode target)
        {
            if (target == null)
            {
                return;
            }

            if (target.GetNodeCount(false) == 0)
            {
                if (target.Tag != null)
                {
                    TreeNodeTag treeNodeTag = (TreeNodeTag)target.Tag;
                    Bitmap bitmap = treeNodeTag.Bitmap;
                    bitmap.Dispose();
                }
                target.Remove();
            }
            else
            {
                while(target.GetNodeCount(false) > 0)
                {
                    int intNodeCount = target.GetNodeCount(false);
                    removeNode(target.Nodes[intNodeCount-1]);
                }
                removeNode(target);
            }
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
            if(e.KeyCode == Keys.Delete)
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
        /// GIF再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItemPreviewPlay_Click(object sender, EventArgs e)
        {
            if (treeView.GetNodeCount(false) == 0)
            {
                return;
            }

            m_intFrameCount = 0;
            treeView.Enabled = false;
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
            if (treeView.GetNodeCount(false) == 0)
            {
                return;
            }

            m_intFrameCount = 0;
            treeView.Enabled = false;
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
            treeView.Enabled = true;
        }

        /// <summary>
        /// GIF停止
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            timerGifPlay.Stop();
            treeView.Enabled = true;
            pictureBox_Redraw(treeView.SelectedNode);
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
            TreeNode nodeProject = treeView.Nodes[0];
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
            TreeNodeTag projectNodeTag = (TreeNodeTag)treeView.Nodes[0].Tag;
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

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            pictureBox1.Image = canvas;
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
            TreeNode node = treeView.SelectedNode;
            if (node == null)
            {
                return;
            }

            if (node == treeView.Nodes[0])
            {
                return;
            }

            int intMovePixel = (int)numericUpDownMovePixel.Value;

            int intX = 0;
            int intY = 0;
            switch(mode)
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
            moveImage(node, intX, intY);

            // プレビュー再描画
            pictureBox_Redraw(node);
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
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                m_mousePoint = e.Location;
            }
        }

        /// <summary>
        /// プレビュー座標値変更処理中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                TreeNode node = treeView.SelectedNode;
 
                // GIF構成選択要素なし
                if (node == null)
                {
                    return;
                }
 
                // プロジェクト選択時
                if (node.Parent == null)
                {
                    return;
                }

                int intX = e.Location.X - m_mousePoint.X;
                int intY = e.Location.Y - m_mousePoint.Y;

                // 画像位置変更
                moveImage(node, intX, intY);

                // プレビュー再描画
                pictureBox_Redraw(node);

                // マウス座標値記録
                m_mousePoint = e.Location;
            }

            if (pictureBox1.Image != null)
            {
                // プレビュー領域サイズ
                Point point = e.Location;
                point.X = point.X - ((pictureBox1.Width - pictureBox1.Image.Width) / 2);
                point.Y = point.Y - ((pictureBox1.Height - pictureBox1.Image.Height) / 2);
                toolStripLabelMousePos.Text = point.ToString();
            }
        }

        /// <summary>
        /// プレビュー座標値変更モード終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            toolStripLabelMousePos.Text = "";
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
            m_bolFormClosing = true;

            // ライブラリ読込中の場合、読込を中止する。
            if ( backgroundWorkerLib.IsBusy)
            {
                backgroundWorkerLib.CancelAsync();
            }

            // GIF生成中の場合、GIF生成を中止する。
            if (backgroundWorkerOutput.IsBusy)
            {
                backgroundWorkerOutput.CancelAsync();
            }
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

        private void toolStripMenuItemVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("たぬぎふ " + ProductVersion.ToString(), "バージョン情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
