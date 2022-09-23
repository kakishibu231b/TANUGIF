using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using static TANUGIF.ProjectFile;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// プロジェクト新規生成
        /// </summary>
        private void createNewProject()
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

                //メニューボタン表示状態更新
                updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
            }
            newProject.Dispose();
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
        /// プロジェクトファイル名
        /// </summary>
        private string strProjectFileName = "";

        /// <summary>
        /// プロジェクトを開く
        /// </summary>
        private void oepnProject()
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

                //メニューボタン表示状態更新
                updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);
            }
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
        /// プロジェクトを保存
        /// </summary>
        private void saveProject()
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
        /// 名前を付けてプロジェクトを保存
        /// </summary>
        private void saveAsProject()
        {
            // プロジェクトなし
            if (treeViewGif.GetNodeCount(false) == 0)
            {
                return;
            }

            saveFileDialogProject.FileName = "";
            if (saveFileDialogProject.ShowDialog() == DialogResult.OK)
            {
                strProjectFileName = saveFileDialogProject.FileName;

                //メニューボタン表示状態更新
                updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.Normal);

                toolStripMenuItemSaveProject.PerformClick();
            }
        }

        // 終了時処理フラグ
        bool m_bolFormClosing = false;

        /// <summary>
        /// 終了時処理
        /// </summary>
        private void formClosing()
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
            if (backgroundWorkerOutputGif.IsBusy)
            {
                backgroundWorkerOutputGif.CancelAsync();
            }

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
