﻿using ExtractorSharp.Core;
using ExtractorSharp.Data;
using System.Drawing;
using System.IO;
using System.Windows;

namespace ExtractorSharp.Command.ImageCommand {
    public class PasteSingleImage : ICommand {

        private ImageEntity Entity;

        private Bitmap Picture;

        private ColorBits Bits;

        private System.Drawing.Point OldLocation;

        private System.Drawing.Point Location;
           
        public string Name => "PasteSingleImage";

        public bool CanUndo => true;

        public bool IsChanged => true;

        public bool IsFlush => false;


        public void Do(params object[] args) {
            Entity = args[0] as ImageEntity;
            Location = (System.Drawing.Point)args[1];
            var clipboard = Clipboarder.Default;
            Bitmap image = null;
            if (clipboard != null) {
                var source = clipboard.Album;
                var indexes = clipboard.Indexes;
                if (indexes.Length > 0 && indexes[0] < source.List.Count) {
                    image = source[indexes[0]].Picture;
                }
            } else if (Clipboard.ContainsFileDropList()) {
                var collection = Clipboard.GetFileDropList();
                if (collection.Count > 0 && File.Exists(collection[0])) {
                    image = Image.FromFile(collection[0]) as Bitmap;
                }
            }
            OldLocation = Entity.Location;
            Picture = Entity.Picture;
            Bits = Entity.Type;
            Entity.ReplaceImage(Bits, false, image);
            Entity.Location = Location;
        }

        public void Redo() {
            Do(Entity,Location);
        }

        public void Undo() {
            Entity.ReplaceImage(Bits, false, Picture);
            Entity.Location = OldLocation;
        }
        
    }
}
