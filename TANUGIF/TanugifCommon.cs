using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANUGIF
{
    internal class TanugifCommon
    {
        /// <summary>
        /// サムネイル生成
        /// </summary>
        /// <param name="image"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Image createThumbnail(Image image, int w, int h)
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
    }
}
