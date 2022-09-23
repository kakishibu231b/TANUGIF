using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TANUGIF
{
    partial class MainForm
    {
        /// <summary>
        /// ライブラリフォルダ一覧
        /// </summary>
        private void showDialogLibFolder()
        {
            Settings settings = Settings.Instance;
            LibFolder libFolder = new LibFolder(settings.LibPath);
            if (libFolder.ShowDialog() == DialogResult.OK)
            {
                settings.LibPath.Clear();
                settings.LibPath = libFolder.getLibFolderList();
                Settings.SaveToXmlFile();

                List<string> libPath = settings.LibPath;

                toolStripComboBoxLibFolder.Items.Clear();
                toolStripComboBoxLibFolder.Items.Add("ALL");
                foreach (string path in libPath)
                {
                    string strFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    toolStripComboBoxLibFolder.Items.Add(strFileNameWithoutExtension);
                }

                // ライブラリ初期化
                initListViewLib();
            }
        }
    }
}
