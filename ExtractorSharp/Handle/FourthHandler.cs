﻿using ExtractorSharp.Data;
using ExtractorSharp.Lib;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ExtractorSharp.Handle {
    /// <summary>
    /// Ver4处理器
    /// </summary>
    class FourthHandler : SecondHandler {

        public FourthHandler(Album Album) : base(Album) {}

        public override Bitmap ConvertToBitmap(ImageEntity entity) {
            var data = entity.Data;
            var size = entity.Width * entity.Height;
            if (entity.Compress != Compress.ZLIB) {
                return base.ConvertToBitmap(entity);
            }
            data = FreeImage.Uncompress(data, size);
            var table = Album.CurrentTable;
            if (table.Count > 0) {
                using (var os = new MemoryStream()) {
                    for (var i = 0; i < data.Length; i++) {
                        var j = data[i] % table.Count;
                        os.WriteColor(table[j], ColorBits.ARGB_8888);
                    }
                    data = os.ToArray();
                }
            }
            return Tools.FromArray(data, entity.Size);
        }

        public override byte[] ConvertToByte(ImageEntity entity) {
            if (entity.Compress == Compress.NONE)
                return base.ConvertToByte(entity);
            using (var ms = new MemoryStream()) {
                var data = entity.Picture.ToArray();
                var table = Album.CurrentTable;
                for (var i = 0; i < data.Length; i += 4) {
                    var color = Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
                    if (!table.Contains(color))
                        table.Add(color);
                    ms.WriteByte((byte)table.IndexOf(color));
                }
                data = ms.ToArray();
                if (data.Length < 2) {
                    data = new byte[2];
                }
                return data;
            }
        }

        public override void NewImage(int count, ColorBits type, int index) {
            if (count < 1) {
                return;
            }
            var array = new ImageEntity[count];
            array[0] = new ImageEntity(Album);
            array[0].Index = index;
            array[0].Data = new byte[4];
            if (type != ColorBits.LINK) {
                array[0].Type = type;
            }
            for (var i = 1; i < count; i++) {
                array[i] = new ImageEntity(Album);
                array[i].Type = type;
                if (type == ColorBits.LINK) {
                    array[i].Target = array[0];
                }
                array[i].Index = array[0].Index + i;
            }
            Album.List.InsertAt(index, array);
        }

        public override byte[] AdjustIndex() {
            var table = Album.CurrentTable;
            var ms = new MemoryStream();
            ms.WriteInt(table.Count);
            ms.WriteColorChart(table.ToArray());
            ms.Write(base.AdjustIndex());
            ms.Close();
            return ms.ToArray();
        }

        public override void ConvertToVersion(Img_Version Version) {
            if (Version == Img_Version.Ver2) {
                Album.List.ForEach(item => item.Type = item.Type == ColorBits.ARGB_1555 && item.Compress == Compress.ZLIB ? ColorBits.ARGB_8888 : item.Type);
            }
        }

        public override void CreateFromStream(Stream stream) {
            var size = stream.ReadInt();
            var Table = new List<Color>(stream.ReadColorChart(size));
            Album.Tables = new List<List<Color>>();
            Album.Tables.Add(Table);
            base.CreateFromStream(stream);
        }

    }
}
