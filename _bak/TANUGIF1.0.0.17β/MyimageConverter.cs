using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TANUGIF
{
    class MyimageConverter
    {
        /// <summary>
        /// 黒のみ抽出
        /// </summary>
        /// <param name="bmpSource"></param>
        /// <returns></returns>
        public static void ImageConvert32bitToLine(Bitmap bmpSource, double dblBrightness, int intRed, int intGreen, int intBlue)
        {
            Color colorTransparent = Color.FromArgb(255, 255-intRed, 255-intGreen, 255-intBlue);
            Color colorLine = Color.FromArgb(0, intRed, intGreen, intBlue);
            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);

                    if (color1.A == 255 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                    else if (color1.GetBrightness() < dblBrightness)
                    {
                        bmpSource.SetPixel(col, row, colorLine);
                    }
                    else
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                }
            }
        }

        /// <summary>
        /// 指定した画像からグレースケール画像を作成する
        /// </summary>
        /// <param name="img">基の画像</param>
        /// <returns>作成されたグレースケール画像</returns>
        public static Bitmap CreateGrayscaleImage(Bitmap img)
        {
            //グレースケールの描画先となるImageオブジェクトを作成
            Bitmap newImg = new Bitmap(img.Width, img.Height);
            //newImgのGraphicsオブジェクトを取得
            Graphics g = Graphics.FromImage(newImg);

            //ColorMatrixオブジェクトの作成
            //グレースケールに変換するための行列を指定する
            System.Drawing.Imaging.ColorMatrix cm =
                new System.Drawing.Imaging.ColorMatrix(
                    new float[][]{
                    new float[]{0.3086f, 0.3086f, 0.3086f, 0 ,0},
                    new float[]{0.6094f, 0.6094f, 0.6094f, 0, 0},
                    new float[]{0.0820f, 0.0820f, 0.0820f, 0, 0},
                    new float[]{0, 0, 0, 1, 0},
                    new float[]{0, 0, 0, 0, 1}
                });
            //ImageAttributesオブジェクトの作成
            System.Drawing.Imaging.ImageAttributes ia =
                new System.Drawing.Imaging.ImageAttributes();
            //ColorMatrixを設定する
            ia.SetColorMatrix(cm);

            //ImageAttributesを使用してグレースケールを描画
            g.DrawImage(img,
                new Rectangle(0, 0, img.Width, img.Height),
                0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);

            //リソースを解放する
            g.Dispose();

            return newImg;
        }

        /// <summary>
        /// 画像ドット変換
        /// </summary>
        /// <param name="bmpSource"></param>
        /// <returns></returns>
        public static void ImageConvert32bitToDot(Bitmap bmpSource, int intRowBlock, int intColBlock)
        {
            Color colorTransparent = Color.FromArgb(255, 0, 0, 0);
            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);
                    if (color1.A == 0 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                    {
                        bmpSource.SetPixel(col, row, colorTransparent);
                    }
                }
            }

            for (int row = 0; row < bmpSource.Height; ++row)
            {
                for (int col = 0; col < bmpSource.Width; ++col)
                {
                    Color color1 = bmpSource.GetPixel(col, row);
                    bool bolIsColorTransparent = true;
                    for (int row2 = row; row2 < row + intRowBlock && row2 < bmpSource.Height; ++row2)
                    {
                        for (int col2 = col; col2 < col + intColBlock && col2 < bmpSource.Width; ++col2)
                        {
                            Color color5 = bmpSource.GetPixel(col2, row2);
                            if (color5.A == colorTransparent.A
                                && color5.R == colorTransparent.R
                                && color5.G == colorTransparent.G
                                && color5.B == colorTransparent.B)
                            {
                                ;
                            }
                            else
                            {
                                if (bolIsColorTransparent)
                                {
                                    bolIsColorTransparent = false;
                                    color1 = color5;
                                    break;
                                }
                            }
                        }
                        if (bolIsColorTransparent == false)
                        {
                            break;
                        }
                    }

                    if (bolIsColorTransparent)
                    {

                    }
                    else
                    {
                        for (int row2 = row; row2 < row + intRowBlock && row2 < bmpSource.Height; ++row2)
                        {
                            for (int col2 = col; col2 < col + intColBlock && col2 < bmpSource.Width; ++col2)
                            {
                                bmpSource.SetPixel(col2, row2, color1);
                            }
                        }
                    }
                    col += intColBlock - 1;
                }
                row += intRowBlock - 1;
            }
        }

        /// <summary>
        /// マージ
        /// </summary>
        /// <param name="bmpSource1"></param>
        /// <returns></returns>
        public static Bitmap ImageMerge(Bitmap bmpSource1, Bitmap bmpSource2)
        {
            Bitmap bmp8bit = new Bitmap(bmpSource1.Width, bmpSource1.Height, PixelFormat.Format8bppIndexed);

            try
            {
                var palette1 = bmpSource1.Palette;
                var palette2 = bmpSource2.Palette;

                BitmapData bmpData8bit = bmp8bit.LockBits(
                    new Rectangle(0, 0, bmp8bit.Width, bmp8bit.Height),
                    ImageLockMode.WriteOnly,
                    bmp8bit.PixelFormat
                );

                BitmapData bmpDataSource1 = bmpSource1.LockBits(
                    new Rectangle(0, 0, bmpSource1.Width, bmpSource1.Height),
                    ImageLockMode.ReadOnly,
                    bmpSource1.PixelFormat
                );

                BitmapData bmpDataSource2 = bmpSource2.LockBits(
                    new Rectangle(0, 0, bmpSource2.Width, bmpSource2.Height),
                    ImageLockMode.ReadOnly,
                    bmpSource2.PixelFormat
                );

                byte[] imgBuf8bit = new byte[bmpData8bit.Stride * bmp8bit.Height];
                byte[] imgBufSource1 = new byte[bmpDataSource1.Stride * bmpSource1.Height];
                byte[] imgBufSource2 = new byte[bmpDataSource2.Stride * bmpSource2.Height];

                Marshal.Copy(bmpDataSource1.Scan0, imgBufSource1, 0, imgBufSource1.Length);
                Marshal.Copy(bmpDataSource2.Scan0, imgBufSource2, 0, imgBufSource2.Length);

                var colors = new Dictionary<Color, byte>();

                byte colorIndex = 0;
                byte colorIndex1 = 0;
                byte colorIndex2 = 0;

                for (int y = 0; y < bmpSource1.Height; y++)
                {
                    for (int x = 0; x < bmpSource1.Width; x++)
                    {
                        int indexSource1 = y * bmpDataSource1.Stride + x;
                        colorIndex1 = imgBufSource1[indexSource1];

                        Color color1 = palette1.Entries[colorIndex1];
                        if (colors.TryGetValue(color1, out byte ii))
                        {
                            imgBuf8bit[indexSource1] = ii;
                        }
                        else
                        {
                            imgBuf8bit[indexSource1] = colorIndex;
                            colors.Add(color1, colorIndex);
                            colorIndex++;
                        }
                    }
                }

                for (int y = 0; y < bmpSource2.Height; y++)
                {
                    for (int x = 0; x < bmpSource2.Width; x++)
                    {
                        int indexSource2 = y * bmpDataSource2.Stride + x;
                        colorIndex2 = imgBufSource2[indexSource2];
                        if (colorIndex2 != 0)
                        {
                            Color color2 = palette2.Entries[colorIndex2];
                            if (colors.TryGetValue(color2, out byte ii))
                            {
                                imgBuf8bit[indexSource2] = ii;
                            }
                            else
                            {
                                imgBuf8bit[indexSource2] = colorIndex;
                                colors.Add(color2, colorIndex);
                                if (colorIndex < 255)
                                {
                                    colorIndex++;
                                }
                            }
                        }
                    }
                }

                Marshal.Copy(imgBuf8bit, 0, bmpData8bit.Scan0, imgBuf8bit.Length);
                bmp8bit.UnlockBits(bmpData8bit);
                bmpSource2.UnlockBits(bmpDataSource2);
                bmpSource1.UnlockBits(bmpDataSource1);

                var pal = bmp8bit.Palette;
                foreach (var item in colors)
                    pal.Entries[item.Value] = item.Key;

                for (int ii = colorIndex; ii <= 255; ++ii)
                {
                    pal.Entries[ii] = Color.FromArgb(255, 0, 0, 0);
                }

                bmp8bit.Palette = pal;

            }
            catch (Exception)
            {
            }
            return bmp8bit;
        }

        /// <summary>
        /// マージ
        /// </summary>
        /// <param name="bmpSource1"></param>
        /// <returns></returns>
        public static void ImageMerge32bit(Bitmap bmpSource1, Bitmap bmpSource2)
        {
            for (int y = 0; y < bmpSource1.Height && y < bmpSource2.Height; y++)
            {
                for (int x = 0; x < bmpSource1.Width && x < bmpSource2.Width; x++)
                {
                    Color color = bmpSource2.GetPixel(x, y);
                    if (color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
                    {
                        ;
                    }
                    else
                    {
                        bmpSource1.SetPixel(x, y, color);
                    }
                }
            }
        }

        /// <summary>
        /// 減色
        /// </summary>
        /// <param name="bmpSource1"></param>
        /// <returns></returns>
        public static void reducedColours32bitTo8bit(Bitmap[] bmpSources)
        {
            // 使用中の色抽出
            var colors = new Dictionary<Color, int>();
            foreach(Bitmap bmpSource in bmpSources)
            {
                for (int y = 0; y < bmpSource.Height && y < bmpSource.Height; y++)
                {
                    for (int x = 0; x < bmpSource.Width && x < bmpSource.Width; x++)
                    {
                        Color color = bmpSource.GetPixel(x, y);
                        if (colors.TryGetValue(color, out int ii))
                        {
                            colors[color] = ii + 1;
                        }
                        else
                        {
                            colors.Add(color, 1);
                        }
                    }
                }
            }

            // 透明色初期処理
            Color colorTransparent;
            foreach (Bitmap bmpSource in bmpSources)
            {
                for (int ii = 0; ii < 255; ++ii)
                {
                    colorTransparent = Color.FromArgb(255, ii, ii, ii);
                    if (!colors.ContainsKey(colorTransparent))
                    {
                        for (int row = 0; row < bmpSource.Height; ++row)
                        {
                            for (int col = 0; col < bmpSource.Width; ++col)
                            {
                                Color color1 = bmpSource.GetPixel(col, row);
                                if (color1.A == 0 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                                {
                                    bmpSource.SetPixel(col, row, colorTransparent);
                                }
                            }
                        }
                        break;
                    }
                }
            }

            // 減色処理要否判定
            if (colors.Count > 255)
            {
                var colors2 = new Dictionary<string, Color>();
                var colors3 = new Dictionary<string, Color>();

                // 減色候補抽出1 色使用件数チェック
                List<string> colorList = new List<string>();
                foreach (var item in colors)
                {
                    if (item.Key.Name == "0")
                    {
                        continue;
                    }
                    string count = string.Format("{0:D10}", item.Value);
                    string key = count + "," + item.Key.Name;
                    colorList.Add(key);
                    colors2.Add(key, item.Key);
                }
                colorList.Sort();

                // 減色候補抽出2 減色対象色名抽出
                List<string> colorList2 = new List<string>();
                for (int ii = 0; ii < colorList.Count; ++ii)
                {
                    string[] strArray = colorList[ii].Split(',');
                    if (ii < colorList.Count - 254)
                    {
                        colorList2.Add(strArray[1]);
                    }
                    else
                    {
                        if (colors2.TryGetValue(colorList[ii], out Color color))
                        {
                            colors3.Add(strArray[1], color);
                        }
                    }
                }

                // 減色処理
                foreach (Bitmap bmpSource in bmpSources)
                {
                    for (int y = 0; y < bmpSource.Height && y < bmpSource.Height; y++)
                    {
                        for (int x = 0; x < bmpSource.Width && x < bmpSource.Width; x++)
                        {
                            Color color = bmpSource.GetPixel(x, y);
                            string colorName = color.Name;
                            if (colorList2.Contains(colorName))
                            {
                                // 減色対象の場合
                                bool bolRaplaced = false;

                                for (int rgb = 1; rgb < 254; ++rgb)
                                {
                                    foreach (var item in colors3)
                                    {
                                        int r1 = item.Value.R;
                                        int g1 = item.Value.G;
                                        int b1 = item.Value.B;

                                        if (Math.Abs(color.R - r1) < rgb && Math.Abs(color.G - g1) < rgb && Math.Abs(color.B - b1) < rgb)
                                        {
                                            bmpSource.SetPixel(x, y, item.Value);
                                            bolRaplaced = true;
                                            break;
                                        }
                                    }
                                    if (bolRaplaced)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 減色
        /// </summary>
        /// <param name="bmpSource1"></param>
        /// <returns></returns>
        public static void reducedColours32bitTo8bit(Bitmap bmpSource)
        {
            var colors = new Dictionary<Color, int>();
            for (int y = 0; y < bmpSource.Height && y < bmpSource.Height; y++)
            {
                for (int x = 0; x < bmpSource.Width && x < bmpSource.Width; x++)
                {
                    Color color = bmpSource.GetPixel(x, y);
                    if (colors.TryGetValue(color, out int ii))
                    {
                        colors[color] = ii + 1;
                    }
                    else
                    {
                        colors.Add(color, 1);
                    }
                }
            }

            Color colorTransparent;
            for (int ii = 0; ii < 255; ++ii)
            {
                colorTransparent = Color.FromArgb(255, ii, ii, ii);
                if (!colors.ContainsKey(colorTransparent))
                {
                    for (int row = 0; row < bmpSource.Height; ++row)
                    {
                        for (int col = 0; col < bmpSource.Width; ++col)
                        {
                            Color color1 = bmpSource.GetPixel(col, row);
                            if (color1.A == 0 && color1.R == 0 && color1.G == 0 && color1.B == 0)
                            {
                                bmpSource.SetPixel(col, row, colorTransparent);
                            }
                        }
                    }
                    break;
                }
            }

            if (colors.Count > 255)
            {
                var colors2 = new Dictionary<string, Color>();
                var colors3 = new Dictionary<string, Color>();
                List<string> colorList = new List<string>();
                foreach (var item in colors)
                {
                    if (item.Key.Name == "0")
                    {
                        continue;
                    }
                    string count = string.Format("{0:D10}", item.Value);
                    string key = count + "," + item.Key.Name;
                    colorList.Add(key);
                    colors2.Add(key, item.Key);
                }
                colorList.Sort();

                List<string> colorList2 = new List<string>();
                for (int ii = 0; ii < colorList.Count; ++ii)
                {
                    string[] strArray = colorList[ii].Split(',');
                    if (ii < colorList.Count - 254)
                    {
                        colorList2.Add(strArray[1]);
                    }
                    else
                    {
                        if ( colors2.TryGetValue(colorList[ii], out Color color))
                        {
                            colors3.Add(strArray[1], color);
                        }
                    }
                }

                for (int y = 0; y < bmpSource.Height && y < bmpSource.Height; y++)
                {
                    for (int x = 0; x < bmpSource.Width && x < bmpSource.Width; x++)
                    {
                        Color color = bmpSource.GetPixel(x, y);
                        string colorName = color.Name;
                        if (colorList2.Contains(colorName))
                        {
                            bool bolRaplaced = false;

                            for (int rgb = 1; rgb < 254; ++rgb)
                            {
                                foreach (var item in colors3)
                                {
                                    int r1 = item.Value.R;
                                    int g1 = item.Value.G;
                                    int b1 = item.Value.B;

                                    if (Math.Abs(color.R - r1) < rgb && Math.Abs(color.G - g1) < rgb && Math.Abs(color.B - b1) < rgb)
                                    {
                                        bmpSource.SetPixel(x, y, item.Value);
                                        bolRaplaced = true;
                                        break;
                                    }
                                }
                                if (bolRaplaced)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 32bpp→8bpp変換
        /// </summary>
        /// <param name="bmpSource">32bpp画像</param>
        /// <returns>8bpp画像</returns>
        public static void ImageConvert32bitTo8bit(Bitmap[] bmpSources)
        {
            // カラーパレット登録候補抽出
            var colors = new Dictionary<Color, byte>();
            for (int idx = 0; idx < bmpSources.Length; ++idx)
            {
                Bitmap bmpSource = bmpSources[idx];

                BitmapData bmpData32bit = bmpSource.LockBits(
                    new Rectangle(0, 0, bmpSource.Width, bmpSource.Height),
                    ImageLockMode.ReadOnly,
                    bmpSource.PixelFormat
                );

                byte[] imgBuf32bit = new byte[bmpData32bit.Stride * bmpSource.Height];
                Marshal.Copy(bmpData32bit.Scan0, imgBuf32bit, 0, imgBuf32bit.Length);

                byte colorIndex = 0;
                for (int y = 0; y < bmpSource.Height; y++)
                {
                    for (int x = 0; x < bmpSource.Width; x++)
                    {
                        int index32bit = y * bmpData32bit.Stride + x * 4;
                        byte b = imgBuf32bit[index32bit];
                        byte g = imgBuf32bit[index32bit + 1];
                        byte r = imgBuf32bit[index32bit + 2];

                        var color = Color.FromArgb(r, g, b);
                        if (colors.TryGetValue(color, out byte i))
                        {
                        }
                        else
                        {
                            colors.Add(color, colorIndex);
                            colorIndex++;
                        }
                    }
                }
                bmpSource.UnlockBits(bmpData32bit);
            }

            // 32bit→8bit変換
            for (int idx = 0; idx < bmpSources.Length; ++idx)
            {
                Bitmap bmpSource = bmpSources[idx];

                Bitmap bmp8bit = new Bitmap(bmpSource.Width, bmpSource.Height, PixelFormat.Format8bppIndexed);

                BitmapData bmpData8bit = bmp8bit.LockBits(
                    new Rectangle(0, 0, bmp8bit.Width, bmp8bit.Height),
                    ImageLockMode.WriteOnly,
                    bmp8bit.PixelFormat
                );

                BitmapData bmpData32bit = bmpSource.LockBits(
                    new Rectangle(0, 0, bmpSource.Width, bmpSource.Height),
                    ImageLockMode.ReadOnly,
                    bmpSource.PixelFormat
                );

                byte[] imgBuf8bit = new byte[bmpData8bit.Stride * bmp8bit.Height];
                byte[] imgBuf32bit = new byte[bmpData32bit.Stride * bmpSource.Height];

                Marshal.Copy(bmpData32bit.Scan0, imgBuf32bit, 0, imgBuf32bit.Length);

                byte colorIndex = 0;
                for (int y = 0; y < bmpSource.Height; y++)
                {
                    for (int x = 0; x < bmpSource.Width; x++)
                    {
                        int index32bit = y * bmpData32bit.Stride + x * 4;
                        int index8bit = y * bmpData8bit.Stride + x;
                        byte b = imgBuf32bit[index32bit];
                        byte g = imgBuf32bit[index32bit + 1];
                        byte r = imgBuf32bit[index32bit + 2];

                        // 32bitから8bitへ
                        var color = Color.FromArgb(r, g, b);
                        if (colors.TryGetValue(color, out byte i))
                        {
                            imgBuf8bit[index8bit] = i;
                        }
                        else
                        {
                            imgBuf8bit[index8bit] = colorIndex;
                            colors.Add(color, colorIndex);
                            colorIndex++;
                        }
                    }
                }
                Marshal.Copy(imgBuf8bit, 0, bmpData8bit.Scan0, imgBuf8bit.Length);
                bmpSource.UnlockBits(bmpData32bit);
                bmp8bit.UnlockBits(bmpData8bit);

                var pal = bmp8bit.Palette;
                foreach (var item in colors)
                {
                    pal.Entries[item.Value] = item.Key;
                }

                bmp8bit.Palette = pal;

                bmpSources[idx].Dispose();
                bmpSources[idx] = bmp8bit;
            }
        }

        /// <summary>
        /// 32bpp→8bpp変換
        /// </summary>
        /// <param name="bmpSource">32bpp画像</param>
        /// <returns>8bpp画像</returns>
        public static Bitmap ImageConvert32bitTo8bit(Bitmap bmpSource)
        {
            Bitmap bmp8bit = new Bitmap(bmpSource.Width, bmpSource.Height, PixelFormat.Format8bppIndexed);

            BitmapData bmpData8bit = bmp8bit.LockBits(
                new Rectangle(0, 0, bmp8bit.Width, bmp8bit.Height),
                ImageLockMode.WriteOnly,
                bmp8bit.PixelFormat
            );

            BitmapData bmpData32bit = bmpSource.LockBits(
                new Rectangle(0, 0, bmpSource.Width, bmpSource.Height),
                ImageLockMode.ReadOnly,
                bmpSource.PixelFormat
            );

            byte[] imgBuf8bit = new byte[bmpData8bit.Stride * bmp8bit.Height];
            byte[] imgBuf32bit = new byte[bmpData32bit.Stride * bmpSource.Height];

            Marshal.Copy(bmpData32bit.Scan0, imgBuf32bit, 0, imgBuf32bit.Length);

            var colors = new Dictionary<Color, byte>();
            byte colorIndex = 0;

            for (int y = 0; y < bmpSource.Height; y++)
            {
                for (int x = 0; x < bmpSource.Width; x++)
                {
                    int index32bit = y * bmpData32bit.Stride + x * 4;
                    int index8bit = y * bmpData8bit.Stride + x;
                    byte b = imgBuf32bit[index32bit];
                    byte g = imgBuf32bit[index32bit + 1];
                    byte r = imgBuf32bit[index32bit + 2];

                    // 32bitから8bitへ
                    var color = Color.FromArgb(r, g, b);
                    if (colors.TryGetValue(color, out byte i))
                    {
                        imgBuf8bit[index8bit] = i;
                    }
                    else
                    {
                        imgBuf8bit[index8bit] = colorIndex;
                        colors.Add(color, colorIndex);
                        colorIndex++;
                    }
                }
            }
            Marshal.Copy(imgBuf8bit, 0, bmpData8bit.Scan0, imgBuf8bit.Length);
            bmpSource.UnlockBits(bmpData32bit);
            bmp8bit.UnlockBits(bmpData8bit);

            var pal = bmp8bit.Palette;
            foreach (var item in colors)
                pal.Entries[item.Value] = item.Key;

            for(int ii = colorIndex; ii <= 255; ++ii)
            {
                pal.Entries[ii] = Color.FromArgb(255, 0, 0, 0);
            }

            bmp8bit.Palette = pal;

            return bmp8bit;
        }

        /// <summary>
        /// 指定された画像から1bppのイメージを作成する
        /// </summary>
        /// <param name="img">基になる画像</param>
        /// <returns>1bppに変換されたイメージ</returns>
        public static Bitmap Create1bppImage(Bitmap img, double dblBrightness)
        {
            //1bppイメージを作成する
            Bitmap newImg = new Bitmap(img.Width, img.Height,
                PixelFormat.Format1bppIndexed);

            //Bitmapをロックする
            BitmapData bitmapData = newImg.LockBits(
                new Rectangle(0, 0, newImg.Width, newImg.Height),
                ImageLockMode.WriteOnly, newImg.PixelFormat);

            //新しい画像のピクセルデータを作成する
            byte[] pixels = new byte[bitmapData.Stride * bitmapData.Height];
            for (int y = 0; y < bitmapData.Height; y++)
            {
                for (int x = 0; x < bitmapData.Width; x++)
                {
                    //明るさが指定以上の時は白くする
                    if (dblBrightness <= img.GetPixel(x, y).GetBrightness())
                    {
                        //ピクセルデータの位置
                        int pos = (x >> 3) + bitmapData.Stride * y;
                        //白くする
                        pixels[pos] |= (byte)(0x80 >> (x & 0x7));
                    }
                }
            }
            //作成したピクセルデータをコピーする
            IntPtr ptr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, ptr, pixels.Length);

            //ロックを解除する
            newImg.UnlockBits(bitmapData);

            return newImg;
        }

    }
}
