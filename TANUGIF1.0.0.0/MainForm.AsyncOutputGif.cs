using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// GIF生成
        /// </summary>
        private void outputGif()
        {
            // 処理中
            if (backgroundWorkerOutputGif.IsBusy)
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
                backgroundWorkerOutputGif.RunWorkerAsync();
            }

            // GIF出力メニューボタン更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.OutputGif);
        }

        /// <summary>
        /// GIF生成
        /// </summary>
        /// <param name="bw"></param>
        private void outputGifAsync(BackgroundWorker bw)
        {
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

                //// 減色処理(パレット統一版)
                //Bitmap[] bitmaps = new Bitmap[bmps.Count];
                //for (int ii = 0; ii < bmps.Count; ++ii)
                //{
                //    bitmaps[ii] = bmps[ii].bitmap;
                //}
                //MyimageConverter.reducedColours32bitTo8bit(bitmaps);
                //MyimageConverter.ImageConvert32bitTo8bit(bitmaps);
                //for (int ii = 0; ii < bmps.Count; ++ii)
                //{
                //    bmps[ii].bitmap = bitmaps[ii];
                //}

                for (int ii = 0; ii < bmps.Count; ++ii)
                {
                    if (bw.CancellationPending)
                    {
                        break;
                    }

                    // 減色処理(パレット非統一版)
                    MyimageConverter.reducedColours32bitTo8bit(bmps[ii].bitmap);
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
        /// <param name="e"></param>
        private void outputGifAsyncProgressChanged(ProgressChangedEventArgs e)
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
        private void outputGifAsyncCompleted()
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

            // GIF出力メニューボタン更新
            updateMenuButtonEnabled(UpdateMenuButtonEnabledMode.OutputGifEnd);
        }

        /// <summary>
        /// GIF生成中止
        /// </summary>
        private void outputGifCancelAsync()
        {
            if (backgroundWorkerOutputGif.IsBusy)
            {
                backgroundWorkerOutputGif.CancelAsync();
            }
        }
    }
}
