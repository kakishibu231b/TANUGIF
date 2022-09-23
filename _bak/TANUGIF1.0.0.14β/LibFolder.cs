using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TANUGIF
{
    public partial class LibFolder : Form
    {
        public LibFolder(List<string> vs)
        {
            InitializeComponent();

            foreach (string strPath in vs)
            {
                listBoxLibFolder.Items.Add(strPath);
            }
        }

        public List<string> getLibFolderList()
        {
            List<string> vs = new List<string>();
            foreach(string strLibPath in listBoxLibFolder.Items)
            {
                vs.Add(strLibPath);
            }
            return vs;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string strPath = folderBrowserDialog1.SelectedPath;

                if (checkBoxFolder.Checked)
                {
                    listBoxLibFolder_Items_Add(strPath);
                }
                else
                {
                    if (!listBoxLibFolder.Items.Contains(strPath))
                    {
                        listBoxLibFolder.Items.Add(strPath);
                    }
                }
            }
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            int intSelectedIndex = listBoxLibFolder.SelectedIndex;
            if (intSelectedIndex < 0)
            {
                return;
            }

            listBoxLibFolder.Items.RemoveAt(intSelectedIndex);

            if (intSelectedIndex == listBoxLibFolder.Items.Count)
            {
                intSelectedIndex--;
            }

            listBoxLibFolder.SelectedIndex = intSelectedIndex;

        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            int intSelectedIndex = listBoxLibFolder.SelectedIndex;
            string strLibFolder = (string)listBoxLibFolder.SelectedItem;
            listBoxLibFolder.Items.RemoveAt(intSelectedIndex);
            listBoxLibFolder.Items.Insert(intSelectedIndex-1, strLibFolder);
            listBoxLibFolder.SelectedIndex = intSelectedIndex - 1;
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            int intSelectedIndex = listBoxLibFolder.SelectedIndex;
            string strLibFolder = (string)listBoxLibFolder.SelectedItem;
            listBoxLibFolder.Items.RemoveAt(intSelectedIndex);
            listBoxLibFolder.Items.Insert(intSelectedIndex + 1, strLibFolder);
            listBoxLibFolder.SelectedIndex = intSelectedIndex + 1;
        }

        private void LibFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void LibFolder_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            string[] strFileNames;
            strFileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach(string strPath in strFileNames)
            {
                if(strPath == null)
                {
                    continue;
                }

                FileAttributes fileAttributes = System.IO.File.GetAttributes(strPath);

                bool isDirectory = fileAttributes.HasFlag(FileAttributes.Directory);

                string strLibDirName = strPath;
                if (!isDirectory)
                {
                    strLibDirName = System.IO.Path.GetDirectoryName(strPath);
                    if(strLibDirName == null)
                    {
                        continue;
                    }
                }

                if (checkBoxFolder.Checked)
                {
                    listBoxLibFolder_Items_Add(strLibDirName);
                }
                else
                {
                    if (!listBoxLibFolder.Items.Contains(strLibDirName))
                    {
                        listBoxLibFolder.Items.Add(strLibDirName);
                    }
                }
            }
        }

        private void listBoxLibFolder_Items_Add(string strLibFolderPath)
        {
            if (!listBoxLibFolder.Items.Contains(strLibFolderPath))
            {
                listBoxLibFolder.Items.Add(strLibFolderPath);
            }

            string[] strDirectories = System.IO.Directory.GetDirectories(
                strLibFolderPath, "*", System.IO.SearchOption.AllDirectories);

            foreach (string strPath in strDirectories)
            {
                FileAttributes fileAttributes = System.IO.File.GetAttributes(strPath);
                bool isDirectory = fileAttributes.HasFlag(FileAttributes.Directory);

                string strLibDirName = strPath;
                if (isDirectory)
                {
                    listBoxLibFolder_Items_Add(strLibDirName);
                }
            }
        }

        private void LibFolder_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void listBoxLibFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        e.Handled = true;
                        buttonUp.Focus();
                        buttonUp.PerformClick();
                        listBoxLibFolder.Focus();
                        break;
                    case Keys.Down:
                        e.Handled = true;
                        buttonDown.Focus();
                        buttonDown.PerformClick();
                        listBoxLibFolder.Focus();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Delete:
                        e.Handled = true;
                        buttonDel.Focus();
                        buttonDel.PerformClick();
                        listBoxLibFolder.Focus();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
