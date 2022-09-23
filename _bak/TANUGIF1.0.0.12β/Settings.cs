using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TANUGIF
{
    [Serializable()]
    public class Settings
    {
        private List<string> m_libPath = new List<string>();
        private int m_libIconSizeLarge;
        private int m_libIconSizeSmall;
        private int m_treeIconSize;

        public List<string> LibPath
        {
            get
            {
                return m_libPath;
            }
            set { m_libPath = value; }
        }

        public int LibIconSizeLarge
        {
            get { return m_libIconSizeLarge; }
            set { m_libIconSizeLarge = value; }
        }
        public int LibIconSizeSmall
        {
            get { return m_libIconSizeSmall; }
            set { m_libIconSizeSmall = value; }
        }
        public int TreeIconSize
        {
            get { return m_treeIconSize; }
            set { m_treeIconSize = value; }
        }

        [NonSerialized()]
        private static string fileName = ".\\settings.config";
        private static Settings _instance = new Settings();
        [System.Xml.Serialization.XmlIgnore]
        public static Settings Instance
        {
            get
            {
                return _instance;
            }
            set { _instance = value; }
        }

        /// <summary>
        /// 設定をXMLファイルから読み込み復元する
        /// </summary>
        public static void LoadFromXmlFile()
        {
            if (!System.IO.File.Exists(fileName))
            {
                Settings settings = Settings.Instance;
                settings.LibPath.Add("lib");
                settings.LibIconSizeLarge = 80;
                settings.LibIconSizeSmall = 20;
                settings.TreeIconSize = 30;
                Settings.SaveToXmlFile();
            }

            StreamReader sr = new StreamReader(fileName, new UTF8Encoding(false));
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //読み込んで逆シリアル化する
            object obj = xs.Deserialize(sr);
            sr.Close();

            if(obj == null)
            {
                return;
            }

            Instance = (Settings)obj;

            if (Instance.LibPath.Count == 0)
            {
                Instance.LibPath.Add("lib");
            }
            if (Instance.LibIconSizeLarge == 0)
            {
                Instance.LibIconSizeLarge = 80;
            }
            if (Instance.LibIconSizeSmall == 0)
            {
                Instance.LibIconSizeSmall = 20;
            }
            if (Instance.TreeIconSize == 0)
            {
                Instance.TreeIconSize = 30;
            }
        }

        /// <summary>
        /// 現在の設定をXMLファイルに保存する
        /// </summary>
        public static void SaveToXmlFile()
        {
            StreamWriter sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //シリアル化して書き込む
            xs.Serialize(sw, Instance);
            sw.Close();
        }
    }
}
