﻿using System;
using System.Windows.Forms;
using ExtractorSharp.Command;
using ExtractorSharp.UI;
using ExtractorSharp.Properties;
using ExtractorSharp.Config;
using ExtractorSharp.Core;
using ExtractorSharp.Data;

namespace ExtractorSharp.View {
    public partial class SaveImageDialog : EaseDialog {
        private int[] Indexes;
        private Album Album;
        private Controller Controller { get; }
        public SaveImageDialog(){
            Controller = Program.Controller;
            InitializeComponent();
            pathBox.Click += LoadPath;
            loadButton.Click += LoadPath;
            yesButton.Click += Replace;
            CancelButton = cancelButton;
        }

        public override DialogResult Show(params object[] args) {
            pathBox.Text = Config["SaveImagePath"].Value;
            allPathCheck.Checked = Config["SaveImageAllPath"].Boolean;
            Album = args[0] as Album;
            Indexes = args[1] as int[];
            if (Config["SaveImageTip"].Boolean) 
                return ShowDialog();
            Controller.Do("saveImage", Album, 1, Indexes, pathBox.Text);
            return DialogResult.None;
        }

        public void Replace(object sender,EventArgs e) {
            Config["SaveImagePath"]= new ConfigValue( pathBox.Text);
            Config["SaveImageTip"] = new ConfigValue(!tipsCheck.Checked);
            Config["SaveImageAllPath"] = new ConfigValue(allPathCheck.Checked);
            Config.Save();
            Controller.Do("saveImage", Album, 1, Indexes, pathBox.Text);
            Visible = false;
        }

        public void LoadPath(object sender, EventArgs e) {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK) 
                pathBox.Text = dialog.SelectedPath;          
        }


    }
}