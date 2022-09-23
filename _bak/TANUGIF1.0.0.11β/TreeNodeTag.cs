using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANUGIF
{
    internal class TreeNodeTag
    {
        private Bitmap _bitmap;

        private string _filePath;

        private int _frameNumber;

        private Point _point;

        private TreeNodeTag()
        {
            _bitmap = null;
            _filePath = "";
            _frameNumber = 1;
            _point = new Point(0, 0);
        }

        public TreeNodeTag(Bitmap bitmap, string filePath, int frameNumber)
        {
            _bitmap = bitmap;
            _filePath = filePath;
            _frameNumber = frameNumber;
            _point = new Point(0, 0);
        }

        public TreeNodeTag(Bitmap bitmap, string filePath, int frameNumber, Point point)
        {
            _bitmap = bitmap;
            _filePath = filePath;
            _frameNumber = frameNumber;
            _point = point;
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        public int FrameNumber
        {
            get { return _frameNumber; }
            set { _frameNumber = value; }    
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set { _bitmap = value; }
        }

        public Point Point
        {
            get { return _point; }
            set { _point = value; }
        }
    }
}
