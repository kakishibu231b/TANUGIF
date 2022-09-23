using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TANUGIF
{
    public static class MyGifEncorder
    {
        public class BitmapAndDelayTime
        {
            public Bitmap bitmap;
            public ushort delayTime;
            public BitmapAndDelayTime(Bitmap _bitmap, ushort _delayTime)
            {
                bitmap = _bitmap;
                delayTime = _delayTime;
            }
        }

        public static void SaveAnimatedGif(string fileName, List<BitmapAndDelayTime> baseImages, ushort loopCount)
        {
            if (baseImages.Count == 0)
            {
                return;
            }

            //書き込み先のファイルを開く
            var writerFs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            //BinaryWriterで書き込む
            var writer = new BinaryWriter(writerFs);
            var ms = new MemoryStream();

            try
            {
                bool hasGlobalColorTable = false;
                int colorTableSize = 0;
                int imagesCount = baseImages.Count;

                for (int i = 0; i < imagesCount; i++)
                {
                    //画像をGIFに変換して、MemoryStreamに入れる
                    Bitmap bmp = baseImages[i].bitmap;
                    bmp.Save(ms, ImageFormat.Gif);
                    ms.Position = 0;

                    if (i == 0)
                    {
                        writer.Write(ReadBytes(ms, 6));   //ヘッダを書き込む // "GIF89a" のはず

                        //Logical Screen Descriptor
                        byte[] screenDescriptor = ReadBytes(ms, 7);
                        //Global Color Tableがあるか確認
                        if ((screenDescriptor[4] & 0x80) != 0)
                        {
                            //Color Tableのサイズを取得
                            colorTableSize = screenDescriptor[4] & 0x07;
                            hasGlobalColorTable = true;
                        }
                        else
                        {
                            hasGlobalColorTable = false;
                        }
                        //Global Color Tableを使わない
                        //広域配色表フラグと広域配色表の寸法を消す
                        screenDescriptor[4] &= 0x78;
                        writer.Write(screenDescriptor);

                        //Application Extension
                        writer.Write(GetApplicationExtension(loopCount));
                    }
                    else
                    {
                        //HeaderとLogical Screen Descriptorをスキップ
                        ms.Position += 6 + 7;
                    }

                    byte[] colorTable = null;
                    if (hasGlobalColorTable)
                    {
                        //Color Tableを取得
                        colorTable = ReadBytes(ms, (1 << (colorTableSize + 1)) * 3);
                    }

                    //Graphics Control Extension
                    writer.Write(GetGraphicControlExtension(baseImages[i].delayTime));

                    {
                        byte[] tmp = PeekBytes(ms, 2);
                        if (tmp[0] == 0x21 && tmp[1] == 0xF9)
                        {
                            //基のGraphics Control Extensionをスキップ
                            ms.Position += 8;
                        }
                    }

                    //Image Descriptor
                    byte[] imageDescriptor = ReadBytes(ms, 10);
                    if (imageDescriptor[0] != 0x2C)
                    { // Image Separator
                        throw new Exception("Unexpected format.");
                    }
                    if (!hasGlobalColorTable)
                    {
                        //Local Color Tableを持っているか確認
                        if ((imageDescriptor[9] & 0x80) == 0)
                        {
                            throw new Exception("Not found local color table."); // not support
                        }
                        colorTableSize = imageDescriptor[9] & 0x07;//Color Tableのサイズを取得
                                                                   //Color Tableを取得
                        colorTable = ReadBytes(ms, (1 << (colorTableSize + 1)) * 3);
                    }
                    //狭域配色表フラグ (Local Color Table Flag) と狭域配色表の寸法を追加
                    imageDescriptor[9] |= (byte)(0x80 | colorTableSize);
                    writer.Write(imageDescriptor);
                    if(colorTable != null)
                    {
                        writer.Write(colorTable);                   //Local Color Tableを書き込む
                    }

                    //Image Dataを書き込む (終了部は書き込まない)
                    writer.Write(ReadBytes(ms, (int)(ms.Length - ms.Position - 1)));

                    if (i == imagesCount - 1)
                    {
                        writer.Write((byte)0x3B);               //終了部 (Trailer)
                    }

                    ms.SetLength(0);                            //MemoryStreamをリセット
                }
            }
            finally
            {
                ms.Close();
                writer.Close();
                writerFs.Close();
            }
        }

        private static byte[] ReadBytes(MemoryStream ms, int count)
        {
            byte[] bs = new byte[count];
            int n = ms.Read(bs, 0, count);
            if (n < count)
            {
                throw new Exception("ReadBytes failed.");
            }
            return bs;
        }

        private static byte[] PeekBytes(MemoryStream ms, int count)
        {
            byte[] bs = new byte[count];
            long pos = ms.Position;
            int n = ms.Read(bs, 0, count);
            ms.Position = pos; // positionを戻す
            if (n < count)
            {
                throw new Exception("PeekBytes failed.");
            }
            return bs;
        }

        // loopCount: 繰り返し回数(0 = 無限)
        private static byte[] GetApplicationExtension(ushort loopCount)
        {
            byte[] bs = new byte[19] {
                    0x21,               // [0] 拡張導入符 (Extension Introducer)
                    0xFF,               // [1] アプリケーション拡張ラベル (Application Extension Label)
                    0x0B,               // [2] ブロック寸法 (Block Size)
                                        // [3..10] "NETSCAPE"  アプリケーション識別名 (Application Identifier)
                    0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45,
                    0x32, 0x2E, 0x30,   // [11..13] "2.0"  アプリケーション確証符号 (Application Authentication Code)
                    0x03,               // [14] データ副ブロック寸法 (Data Sub-block Size)
                    0x01,               // [15] 詰め込み欄 [ネットスケープ拡張コード (Netscape Extension Code)]
                    0x00, 0x00,         // [16..17] ※以降の処理で代入  繰り返し回数 (Loop Count)
                    0x00                // [18] ブロック終了符 (Block Terminator)
                };
            bs[16] = (byte)(loopCount & 0xFF);
            bs[17] = (byte)((loopCount >> 8) & 0xFF);
            return bs;
        }

        private enum PackedField
        {
            UseTransparency = 0x01,
            UseUserInput = 0x02,
            NotSpecified = 0x00,
            DoNotDispose = 0x04,
            RestoreToBGColor = 0x08,
            RestoreToPrevious = 0x0C,
        }

        // delayTime: 遅延時間 (単位:10ms)
        private static byte[] GetGraphicControlExtension(ushort delayTime)
        {
            byte[] bs = new byte[8]{
                    0x21,       // [0] 拡張導入符 (Extension Introducer)
                    0xF9,       // [1] グラフィック制御ラベル (Graphic Control Label)
                    0x04,       // [2] ブロック寸法 (Block Size, Byte Size)

                    0x09,       // [3] 詰め込み欄 (Packed Field)
                                //     bit 0:            1=透過色指標を使う
                                //     bit 3-2: 消去方法: 1=そのまま残す  2=背景色でつぶす  3=直前の画像に戻す

                    0x00, 0x00, // [4..5] ※以降の処理で代入   遅延時間 (Delay Time)
                    0x00,       // [6] 透過色指標 (Transparency Index, Transparent Color Index)
                    0x00        // [7] ブロック終了符 (Block Terminator)
                };
            bs[4] = (byte)(delayTime & 0xFF);
            bs[5] = (byte)((delayTime >> 8) & 0xFF);
            return bs;
        }
    }
}
