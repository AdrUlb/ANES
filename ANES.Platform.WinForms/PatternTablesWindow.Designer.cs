namespace ANES.Platform.WinForms;

partial class PatternTablesWindow
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

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
		patternSdl0 = new ANES.Platform.WinForms.Controls.SdlControl();
		myMenuStrip1 = new ANES.Platform.WinForms.Controls.MyMenuStrip();
		fileToolStripMenuItem = new ToolStripMenuItem();
		mainMenuFileExit = new ToolStripMenuItem();
		viewToolStripMenuItem = new ToolStripMenuItem();
		mainMenuViewScale = new ToolStripMenuItem();
		patternGroup0 = new GroupBox();
		patternGroup1 = new GroupBox();
		patternSdl1 = new ANES.Platform.WinForms.Controls.SdlControl();
		myMenuStrip1.SuspendLayout();
		patternGroup0.SuspendLayout();
		patternGroup1.SuspendLayout();
		SuspendLayout();
		// 
		// patternSdl0
		// 
		patternSdl0.BackColor = Color.Black;
		patternSdl0.Dock = DockStyle.Fill;
		patternSdl0.Location = new Point(12, 28);
		patternSdl0.Name = "patternSdl0";
		patternSdl0.Padding = new Padding(12);
		patternSdl0.Size = new Size(131, 125);
		patternSdl0.TabIndex = 0;
		// 
		// myMenuStrip1
		// 
		myMenuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem });
		myMenuStrip1.Location = new Point(0, 0);
		myMenuStrip1.Name = "myMenuStrip1";
		myMenuStrip1.Padding = new Padding(0);
		myMenuStrip1.Size = new Size(382, 24);
		myMenuStrip1.TabIndex = 1;
		myMenuStrip1.Text = "myMenuStrip1";
		// 
		// fileToolStripMenuItem
		// 
		fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuFileExit });
		fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		fileToolStripMenuItem.Size = new Size(37, 24);
		fileToolStripMenuItem.Text = "&File";
		// 
		// mainMenuFileExit
		// 
		mainMenuFileExit.Name = "mainMenuFileExit";
		mainMenuFileExit.Size = new Size(93, 22);
		mainMenuFileExit.Text = "E&xit";
		mainMenuFileExit.Click += mainMenuFileExit_Click;
		// 
		// viewToolStripMenuItem
		// 
		viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuViewScale });
		viewToolStripMenuItem.Name = "viewToolStripMenuItem";
		viewToolStripMenuItem.Size = new Size(44, 24);
		viewToolStripMenuItem.Text = "&View";
		// 
		// mainMenuViewScale
		// 
		mainMenuViewScale.Name = "mainMenuViewScale";
		mainMenuViewScale.Size = new Size(101, 22);
		mainMenuViewScale.Text = "&Scale";
		// 
		// patternGroup0
		// 
		patternGroup0.Controls.Add(patternSdl0);
		patternGroup0.Location = new Point(35, 47);
		patternGroup0.Name = "patternGroup0";
		patternGroup0.Padding = new Padding(12);
		patternGroup0.Size = new Size(155, 165);
		patternGroup0.TabIndex = 2;
		patternGroup0.TabStop = false;
		patternGroup0.Text = "Pattern Table 0x0000";
		// 
		// patternGroup1
		// 
		patternGroup1.Controls.Add(patternSdl1);
		patternGroup1.Location = new Point(209, 46);
		patternGroup1.Name = "patternGroup1";
		patternGroup1.Padding = new Padding(12);
		patternGroup1.Size = new Size(161, 166);
		patternGroup1.TabIndex = 3;
		patternGroup1.TabStop = false;
		patternGroup1.Text = "Pattern Table 0x1000";
		// 
		// patternSdl1
		// 
		patternSdl1.BackColor = Color.Black;
		patternSdl1.Dock = DockStyle.Fill;
		patternSdl1.Location = new Point(12, 28);
		patternSdl1.Name = "patternSdl1";
		patternSdl1.Size = new Size(137, 126);
		patternSdl1.TabIndex = 0;
		// 
		// PatternTablesWindow
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(382, 271);
		Controls.Add(patternGroup1);
		Controls.Add(patternGroup0);
		Controls.Add(myMenuStrip1);
		MainMenuStrip = myMenuStrip1;
		MaximizeBox = false;
		MdiChildrenMinimizedAnchorBottom = false;
		MinimizeBox = false;
		Name = "PatternTablesWindow";
		ShowInTaskbar = false;
		Text = "Pattern Tables";
		myMenuStrip1.ResumeLayout(false);
		myMenuStrip1.PerformLayout();
		patternGroup0.ResumeLayout(false);
		patternGroup1.ResumeLayout(false);
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private Controls.SdlControl patternSdl0;
	private Controls.MyMenuStrip myMenuStrip1;
	private ToolStripMenuItem fileToolStripMenuItem;
	private ToolStripMenuItem mainMenuFileExit;
	private GroupBox patternGroup0;
	private GroupBox patternGroup1;
	private Controls.SdlControl patternSdl1;
	private ToolStripMenuItem viewToolStripMenuItem;
	private ToolStripMenuItem mainMenuViewScale;
}