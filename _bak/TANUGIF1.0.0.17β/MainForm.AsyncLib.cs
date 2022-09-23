using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
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
        /// <param name="bw"></param>
        private void initListViewLibAsync(BackgroundWorker bw)
        {
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
        /// ライブラリ画像読込中
        /// </summary>
        /// <param name="e"></param>
        private void initListViewLibAsyncProgressChanged(ProgressChangedEventArgs e)
        {
            toolStripProgressBarLib.Value = e.ProgressPercentage;

            if (e.UserState != null)
            {
                toolStripStatusLabelLib.Text = (string)e.UserState;
            }
        }

        /// <summary>
        /// ライブラリ画像読込完了
        /// </summary>
        private void initListViewLibAsyncCompleted()
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
    }
}
