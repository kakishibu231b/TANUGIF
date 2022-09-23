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

                // フォルダ選択コンボボックス初期化
                initComboBoxLibFolder();

                // ライブラリ初期化
                initListViewLib();
            }
        }

        /// <summary>
        /// フォルダ選択コンボボックス要素
        /// </summary>
        private class ComboBoxLibFolderElement
        {
            private string _text;
            private string _path;

            private ComboBoxLibFolderElement()
            {
                ;
            }

            public ComboBoxLibFolderElement(string strText, string strPath)
            {
                _text = strText;
                _path = strPath;
            }

            public string Text
            {
                get { return _text; }
                set { _text = value; }
            }

            public string Path
            {
                get { return _path; }
                set { _path = value; }
            }

            public override string ToString()
            {
                return _text;
            }
        }

        /// <summary>
        /// フォルダ選択コンボボックス初期化
        /// </summary>
        public void initComboBoxLibFolder()
        {
            Settings settings = Settings.Instance;
            List<string> libPath = settings.LibPath;

            toolStripComboBoxLibFolder.Items.Clear();
            ComboBoxLibFolderElement comboBoxLibFolderElement = new ComboBoxLibFolderElement("ALL", "");
            toolStripComboBoxLibFolder.Items.Add(comboBoxLibFolderElement);
            foreach (string path in libPath)
            {
                string strFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                string strFullPath = Path.GetFullPath(path);
                comboBoxLibFolderElement = new ComboBoxLibFolderElement(strFileNameWithoutExtension, strFullPath);
                toolStripComboBoxLibFolder.Items.Add(comboBoxLibFolderElement);
            }
        }
    }
}
