namespace DirectPlayS
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.BunnerLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TextEditor = new System.Windows.Forms.TextBox();
            this.SelectTextEditor = new System.Windows.Forms.Button();
            this.NowPlaying = new System.Windows.Forms.Label();
            this.SelectRomFIlename = new System.Windows.Forms.Button();
            this.RomFilename = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SelectMusicPlayer = new System.Windows.Forms.Button();
            this.MusicPlayer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // BunnerLabel
            // 
            this.BunnerLabel.AllowDrop = true;
            this.BunnerLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BunnerLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BunnerLabel.Location = new System.Drawing.Point(12, 13);
            this.BunnerLabel.Name = "BunnerLabel";
            this.BunnerLabel.Size = new System.Drawing.Size(564, 166);
            this.BunnerLabel.TabIndex = 0;
            this.BunnerLabel.Text = "Drop the s file you want to play here.";
            this.BunnerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.BunnerLabel.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 260);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(393, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "Text editor to use when s file is ordinary ASM code:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 195);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 18);
            this.label3.TabIndex = 2;
            this.label3.Text = "Select Play ROM";
            // 
            // TextEditor
            // 
            this.TextEditor.Location = new System.Drawing.Point(15, 284);
            this.TextEditor.Name = "TextEditor";
            this.TextEditor.Size = new System.Drawing.Size(482, 25);
            this.TextEditor.TabIndex = 5;
            this.TextEditor.TextChanged += new System.EventHandler(this.TextEditor_TextChanged);
            this.TextEditor.DoubleClick += new System.EventHandler(this.TextEditor_DoubleClick);
            // 
            // SelectTextEditor
            // 
            this.SelectTextEditor.Location = new System.Drawing.Point(513, 284);
            this.SelectTextEditor.Name = "SelectTextEditor";
            this.SelectTextEditor.Size = new System.Drawing.Size(36, 23);
            this.SelectTextEditor.TabIndex = 6;
            this.SelectTextEditor.Text = "...";
            this.SelectTextEditor.UseVisualStyleBackColor = true;
            this.SelectTextEditor.Click += new System.EventHandler(this.SelectTextEditor_Click);
            // 
            // NowPlaying
            // 
            this.NowPlaying.AutoSize = true;
            this.NowPlaying.Location = new System.Drawing.Point(30, 149);
            this.NowPlaying.Name = "NowPlaying";
            this.NowPlaying.Size = new System.Drawing.Size(66, 18);
            this.NowPlaying.TabIndex = 7;
            this.NowPlaying.Text = "Playing:";
            // 
            // SelectRomFIlename
            // 
            this.SelectRomFIlename.Location = new System.Drawing.Point(513, 216);
            this.SelectRomFIlename.Name = "SelectRomFIlename";
            this.SelectRomFIlename.Size = new System.Drawing.Size(36, 23);
            this.SelectRomFIlename.TabIndex = 9;
            this.SelectRomFIlename.Text = "...";
            this.SelectRomFIlename.UseVisualStyleBackColor = true;
            this.SelectRomFIlename.Click += new System.EventHandler(this.SelectRomFIlename_Click);
            // 
            // RomFilename
            // 
            this.RomFilename.Location = new System.Drawing.Point(15, 216);
            this.RomFilename.Name = "RomFilename";
            this.RomFilename.Size = new System.Drawing.Size(482, 25);
            this.RomFilename.TabIndex = 8;
            this.RomFilename.TextChanged += new System.EventHandler(this.RomFilename_TextChanged);
            this.RomFilename.DoubleClick += new System.EventHandler(this.RomFilename_DoubleClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 328);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 18);
            this.label4.TabIndex = 11;
            this.label4.Text = "Select Music Player:";
            // 
            // SelectMusicPlayer
            // 
            this.SelectMusicPlayer.Location = new System.Drawing.Point(513, 352);
            this.SelectMusicPlayer.Name = "SelectMusicPlayer";
            this.SelectMusicPlayer.Size = new System.Drawing.Size(36, 23);
            this.SelectMusicPlayer.TabIndex = 13;
            this.SelectMusicPlayer.Text = "...";
            this.SelectMusicPlayer.UseVisualStyleBackColor = true;
            this.SelectMusicPlayer.Click += new System.EventHandler(this.SelectMusicPlayer_Click);
            // 
            // MusicPlayer
            // 
            this.MusicPlayer.Location = new System.Drawing.Point(15, 352);
            this.MusicPlayer.Name = "MusicPlayer";
            this.MusicPlayer.Size = new System.Drawing.Size(482, 25);
            this.MusicPlayer.TabIndex = 12;
            this.MusicPlayer.TextChanged += new System.EventHandler(this.MusicPlayer_TextChanged);
            this.MusicPlayer.DoubleClick += new System.EventHandler(this.MusicPlayer_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 407);
            this.Controls.Add(this.SelectMusicPlayer);
            this.Controls.Add(this.MusicPlayer);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SelectRomFIlename);
            this.Controls.Add(this.RomFilename);
            this.Controls.Add(this.NowPlaying);
            this.Controls.Add(this.SelectTextEditor);
            this.Controls.Add(this.TextEditor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BunnerLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "DirectPlayS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label BunnerLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TextEditor;
        private System.Windows.Forms.Button SelectTextEditor;
        private System.Windows.Forms.Label NowPlaying;
        private System.Windows.Forms.Button SelectRomFIlename;
        private System.Windows.Forms.TextBox RomFilename;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SelectMusicPlayer;
        private System.Windows.Forms.TextBox MusicPlayer;
    }
}

