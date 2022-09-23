using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using static TANUGIF.ProjectFile;

namespace TANUGIF
{
    [Serializable()]
    public class ProjectFile
    {
        private List<Project> m_projects = new List<Project>();

        public List<Project> ProjectList
        {
            get { return m_projects; }
            set { m_projects = value; }
        }

        public void Add(Project project)
        {
            m_projects.Add(project);
        }

        public void Clear()
        {
            m_projects.Clear();
        }

        /// <summary>
        /// プロジェクト
        /// </summary>
        public class Project
        {
            private string m_name = "";
            private int m_height = 0;
            private int m_width = 0;
            private List<Frame> m_frames = new List<Frame>();

            public string Name {
                get { return m_name; }
                set { m_name = value; }
            }
            public int Height
            {
                get { return m_height; }
                set { m_height = value; }
            }
            public int Width
            {
                get { return m_width; }
                set { m_width = value; }
            }

            public List<Frame> FrameList
            {
                get { return m_frames; }
                set { m_frames = value; }
            }


            public void Add(Frame frame)
            {
                m_frames.Add(frame);
            }
        }

        /// <summary>
        /// フレーム
        /// </summary>
        public class Frame
        {
            private string m_name = "";
            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }

            private List<FrameImage> m_frameImages = new List<FrameImage>();

            public List<FrameImage> FrameImageList
            {
                get { return m_frameImages; }
                set { m_frameImages = value; }
            }

            public void Add(FrameImage frameImage)
            {
                m_frameImages.Add(frameImage);
            }
        }

        /// <summary>
        /// フレームイメージ
        /// </summary>
        public class FrameImage
        {
            private string m_name = "";
            private string strFilePath = "";
            private int intFrameNumber = 1;

            private Point point;

            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }
            public string FilePath
            {
                get { return strFilePath; }
                set { strFilePath = value; }
            }
            public int FrameNumber
            {
                get { return intFrameNumber; }
                set { intFrameNumber = value; }
            }
            public Point Point
            {
                get { return point; }
                set { point = value; }
            }
        }

        [NonSerialized()]
        private static ProjectFile _instance = new ProjectFile();

        [System.Xml.Serialization.XmlIgnore]
        public static ProjectFile Instance
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
        public static void LoadFromXmlFile(string strFileName)
        {
            StreamReader sr = new StreamReader(strFileName, new UTF8Encoding(false));
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(typeof(ProjectFile));
            //読み込んで逆シリアル化する
            object obj = xs.Deserialize(sr);
            if (obj != null)
            {
                _instance = (ProjectFile)obj;
            }
            sr.Close();
        }

        /// <summary>
        /// 現在の設定をXMLファイルに保存する
        /// </summary>
        public static void SaveToXmlFile(string strFileName)
        {
            string tempFileName = Path.GetTempFileName();
            StreamWriter sw = new StreamWriter(tempFileName, false, new UTF8Encoding(false));
            System.Xml.Serialization.XmlSerializer xs =
                new System.Xml.Serialization.XmlSerializer(typeof(ProjectFile));
            //シリアル化して書き込む
            xs.Serialize(sw, Instance);
            sw.Close();

            if (File.Exists(strFileName))
            {
                File.Delete(strFileName);
            }
            File.Move(tempFileName, strFileName);
        }
    }
}
