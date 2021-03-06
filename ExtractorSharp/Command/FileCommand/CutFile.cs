﻿using ExtractorSharp.Core;
using ExtractorSharp.Data;
using ExtractorSharp.Loose;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace ExtractorSharp.Command.ImgCommand {
    class CutFile : ICommand {

        public bool CanUndo => true;

        public bool IsChanged => false;

        public bool IsFlush => false;

        private Clipboarder Clipboarder;

        private Album[] Array;

        private ClipMode Mode;


        public string Name => "CutFile";


        public void Do(params object[] args) {
            Array = args[0] as Album[];
            Mode = (ClipMode)args[1];
            Clipboarder = Clipboarder.Default;
            Clipboarder.Default = Clipboarder.CreateClipboarder(Array, null, Mode);
            var builder = new LSBuilder();
            var dir = $"{Program.Config["RootPath"]}/temp/clipbord_img";
            if (Directory.Exists(dir)) {
                //删除文件夹内容
                Directory.Delete(dir,true);
            }
            Directory.CreateDirectory(dir);
            var path_arr = new string[Array.Length];
            for (var i = 0; i < Array.Length; i++) {
                path_arr[i] = $"{dir}/{Array[i].Name}";
                Tools.SaveFile(path_arr[i], Array[i]);
                var json_path = path_arr[i].RemoveSuffix(".ogg");
                json_path = json_path.RemoveSuffix(".img");
                json_path = $"{json_path}.json";
                var root = new LSObject {
                    { "path", Array[i].Path }
                };
                builder.Write(root, json_path);
            }
            var collection = new StringCollection();
            collection.AddRange(path_arr);
            Clipboard.SetFileDropList(collection);
        }

        public void Redo() {
           Do(Array, Mode);
        }

        public void Undo() {
            Clipboarder.Default = Clipboarder;
        }
    }
}
