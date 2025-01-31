using System.ComponentModel;

namespace ExplosiveCatsUi;

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
        flpHand = new System.Windows.Forms.FlowLayoutPanel();
        btnPlay = new System.Windows.Forms.Button();
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
        buttonReady.Location = new System.Drawing.Point(666, 348);
        buttonReady.Name = "buttonReady";
        buttonReady.Size = new System.Drawing.Size(108, 42);
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
        // flpHand
        // 
        flpHand.Dock = System.Windows.Forms.DockStyle.Bottom;
        flpHand.Location = new System.Drawing.Point(0, 400);
        flpHand.Name = "flpHand";
        flpHand.Size = new System.Drawing.Size(800, 200);
        flpHand.TabIndex = 0;
        // 
        // btnPlay
        // 
        btnPlay.Location = new System.Drawing.Point(12, 12);
        btnPlay.Name = "btnPlay";
        btnPlay.Size = new System.Drawing.Size(100, 40);
        btnPlay.TabIndex = 1;
        btnPlay.Text = "Сыграть карту";
        btnPlay.UseVisualStyleBackColor = true;
        // 
        // MainPageForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(782, 403);
        Controls.Add(labelMove);
        Controls.Add(buttonReady);
        Controls.Add(labelStatus);
        Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
        Text = "MainPageForm";
        Resize += MainPageForm_Resize;
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.Label labelMove;

    private System.Windows.Forms.Button buttonReady;

    private System.Windows.Forms.Label labelStatus;
    private FlowLayoutPanel flpHand;
    private Button btnPlay;

    #endregion
}