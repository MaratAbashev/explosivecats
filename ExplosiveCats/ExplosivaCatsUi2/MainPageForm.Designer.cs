using System.ComponentModel;

namespace ExplosivaCatsUi2;

partial class MainPageForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainPageForm));
        labelStatus = new System.Windows.Forms.Label();
        buttonReady = new System.Windows.Forms.Button();
        labelMove = new System.Windows.Forms.Label();
        btnPlay = new System.Windows.Forms.Button();
        btnTake = new System.Windows.Forms.Button();
        cardPanel = new System.Windows.Forms.Panel();
        otherPlayerInfo = new System.Windows.Forms.Label();
        insertion = new System.Windows.Forms.TrackBar();
        buttonDefuse = new System.Windows.Forms.Button();
        indexLabel = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)insertion).BeginInit();
        SuspendLayout();
        // 
        // labelStatus
        // 
        labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom));
        labelStatus.AutoSize = true;
        labelStatus.Location = new System.Drawing.Point(358, 9);
        labelStatus.Name = "labelStatus";
        labelStatus.Size = new System.Drawing.Size(115, 20);
        labelStatus.TabIndex = 0;
        labelStatus.Text = "Подключение...";
        // 
        // buttonReady
        // 
        buttonReady.BackColor = System.Drawing.SystemColors.Control;
        buttonReady.Location = new System.Drawing.Point(666, 349);
        buttonReady.Name = "buttonReady";
        buttonReady.Size = new System.Drawing.Size(107, 42);
        buttonReady.TabIndex = 1;
        buttonReady.Text = "Готов";
        buttonReady.UseVisualStyleBackColor = false;
        buttonReady.Visible = false;
        buttonReady.Click += buttonReady_Click;
        buttonReady.MouseHover += buttonReady_MouseHover;
        // 
        // labelMove
        // 
        labelMove.AllowDrop = true;
        labelMove.AutoSize = true;
        labelMove.Location = new System.Drawing.Point(369, 174);
        labelMove.Name = "labelMove";
        labelMove.Size = new System.Drawing.Size(85, 20);
        labelMove.TabIndex = 2;
        labelMove.Text = "Ждем хода";
        labelMove.Visible = false;
        // 
        // btnPlay
        // 
        btnPlay.Location = new System.Drawing.Point(666, 300);
        btnPlay.Name = "btnPlay";
        btnPlay.Size = new System.Drawing.Size(107, 43);
        btnPlay.TabIndex = 3;
        btnPlay.Text = "Играть";
        btnPlay.UseVisualStyleBackColor = true;
        btnPlay.Visible = false;
        btnPlay.Click += buttonPlay_Click;
        // 
        // btnTake
        // 
        btnTake.Location = new System.Drawing.Point(666, 248);
        btnTake.Name = "btnTake";
        btnTake.Size = new System.Drawing.Size(107, 46);
        btnTake.TabIndex = 4;
        btnTake.Text = "Взять";
        btnTake.UseVisualStyleBackColor = true;
        btnTake.Visible = false;
        btnTake.Click += btnTake_Click;
        // 
        // cardPanel
        // 
        cardPanel.AutoScroll = true;
        cardPanel.Location = new System.Drawing.Point(41, 203);
        cardPanel.Name = "cardPanel";
        cardPanel.Size = new System.Drawing.Size(273, 188);
        cardPanel.TabIndex = 5;
        // 
        // otherPlayerInfo
        // 
        otherPlayerInfo.AutoSize = true;
        otherPlayerInfo.Location = new System.Drawing.Point(380, 69);
        otherPlayerInfo.Name = "otherPlayerInfo";
        otherPlayerInfo.Size = new System.Drawing.Size(50, 20);
        otherPlayerInfo.TabIndex = 6;
        otherPlayerInfo.Text = "label1";
        otherPlayerInfo.Visible = false;
        // 
        // insertion
        // 
        insertion.Location = new System.Drawing.Point(358, 312);
        insertion.Name = "insertion";
        insertion.Size = new System.Drawing.Size(145, 56);
        insertion.TabIndex = 7;
        insertion.Visible = false;
        insertion.ValueChanged += insertion_ValueChanged;
        // 
        // buttonDefuse
        // 
        buttonDefuse.Location = new System.Drawing.Point(524, 312);
        buttonDefuse.Name = "buttonDefuse";
        buttonDefuse.Size = new System.Drawing.Size(117, 36);
        buttonDefuse.TabIndex = 8;
        buttonDefuse.Text = "Обезвредь";
        buttonDefuse.UseVisualStyleBackColor = true;
        buttonDefuse.Visible = false;
        buttonDefuse.Click += buttonDefuse_Click;
        // 
        // indexLabel
        // 
        indexLabel.Location = new System.Drawing.Point(358, 272);
        indexLabel.Name = "indexLabel";
        indexLabel.Size = new System.Drawing.Size(145, 28);
        indexLabel.TabIndex = 9;
        indexLabel.Text = "0";
        indexLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        indexLabel.Visible = false;
        // 
        // MainPageForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(782, 403);
        Controls.Add(indexLabel);
        Controls.Add(buttonDefuse);
        Controls.Add(insertion);
        Controls.Add(otherPlayerInfo);
        Controls.Add(cardPanel);
        Controls.Add(btnTake);
        Controls.Add(btnPlay);
        Controls.Add(labelMove);
        Controls.Add(buttonReady);
        Controls.Add(labelStatus);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        Text = "MainPageForm";
        Resize += MainPageForm_Resize;
        ((System.ComponentModel.ISupportInitialize)insertion).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Label indexLabel;

    private System.Windows.Forms.Button buttonDefuse;

    private System.Windows.Forms.TrackBar insertion;

    private System.Windows.Forms.Label otherPlayerInfo;

    private System.Windows.Forms.Panel cardPanel;

    private System.Windows.Forms.Button btnTake;

    private System.Windows.Forms.Button btnPlay;

    private System.Windows.Forms.Label labelMove;

    private System.Windows.Forms.Button buttonReady;

    private System.Windows.Forms.Label labelStatus;

    #endregion
}