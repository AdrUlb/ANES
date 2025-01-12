using ANES.Platform.WinForms.Controls;

namespace ANES.Platform.WinForms;

partial class MainWindow
{
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
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
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		sdlControl = new SdlControl();
		mainMenu = new MyMenuStrip();
		fileToolStripMenuItem = new ToolStripMenuItem();
		mainMenuFileOpen = new ToolStripMenuItem();
		toolStripMenuItem1 = new ToolStripSeparator();
		mainMenuFileExit = new ToolStripMenuItem();
		viewToolStripMenuItem = new ToolStripMenuItem();
		mainMenuViewScale = new ToolStripMenuItem();
		debugToolStripMenuItem = new ToolStripMenuItem();
		mainMenuDebugPatternTableViewer = new ToolStripMenuItem();
		helpToolStripMenuItem = new ToolStripMenuItem();
		mainMenuHelpAbout = new ToolStripMenuItem();
		romOpenFIleDialog = new OpenFileDialog();
		mainMenu.SuspendLayout();
		SuspendLayout();
		// 
		// sdlControl
		// 
		sdlControl.BackColor = Color.Black;
		sdlControl.Dock = DockStyle.Fill;
		sdlControl.Location = new Point(0, 24);
		sdlControl.Name = "sdlControl";
		sdlControl.Size = new Size(320, 216);
		sdlControl.TabIndex = 0;
		// 
		// mainMenu
		// 
		mainMenu.BackColor = SystemColors.MenuBar;
		mainMenu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, debugToolStripMenuItem, helpToolStripMenuItem });
		mainMenu.Location = new Point(0, 0);
		mainMenu.Name = "mainMenu";
		mainMenu.Padding = new Padding(0);
		mainMenu.Size = new Size(320, 24);
		mainMenu.TabIndex = 1;
		mainMenu.Text = "menuStrip1";
		// 
		// fileToolStripMenuItem
		// 
		fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuFileOpen, toolStripMenuItem1, mainMenuFileExit });
		fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		fileToolStripMenuItem.Size = new Size(37, 24);
		fileToolStripMenuItem.Text = "&File";
		// 
		// mainMenuFileOpen
		// 
		mainMenuFileOpen.Name = "mainMenuFileOpen";
		mainMenuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
		mainMenuFileOpen.Size = new Size(146, 22);
		mainMenuFileOpen.Text = "&Open";
		mainMenuFileOpen.Click += mainMenuFileOpen_Click;
		// 
		// toolStripMenuItem1
		// 
		toolStripMenuItem1.Name = "toolStripMenuItem1";
		toolStripMenuItem1.Size = new Size(143, 6);
		// 
		// mainMenuFileExit
		// 
		mainMenuFileExit.Name = "mainMenuFileExit";
		mainMenuFileExit.ShortcutKeys = Keys.Control | Keys.Q;
		mainMenuFileExit.Size = new Size(146, 22);
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
		// debugToolStripMenuItem
		// 
		debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuDebugPatternTableViewer });
		debugToolStripMenuItem.Name = "debugToolStripMenuItem";
		debugToolStripMenuItem.Size = new Size(54, 24);
		debugToolStripMenuItem.Text = "&Debug";
		// 
		// mainMenuDebugPatternTableViewer
		// 
		mainMenuDebugPatternTableViewer.Name = "mainMenuDebugPatternTableViewer";
		mainMenuDebugPatternTableViewer.Size = new Size(147, 22);
		mainMenuDebugPatternTableViewer.Text = "&Pattern Tables";
		mainMenuDebugPatternTableViewer.Click += mainMenuDebugPatternTableViewer_Click;
		// 
		// helpToolStripMenuItem
		// 
		helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuHelpAbout });
		helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		helpToolStripMenuItem.Size = new Size(44, 24);
		helpToolStripMenuItem.Text = "&Help";
		// 
		// mainMenuHelpAbout
		// 
		mainMenuHelpAbout.Name = "mainMenuHelpAbout";
		mainMenuHelpAbout.Size = new Size(107, 22);
		mainMenuHelpAbout.Text = "&About";
		// 
		// romOpenFIleDialog
		// 
		romOpenFIleDialog.DefaultExt = "nes";
		romOpenFIleDialog.Filter = "NES files|*.nes";
		// 
		// MainWindow
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(320, 240);
		Controls.Add(sdlControl);
		Controls.Add(mainMenu);
		Name = "MainWindow";
		Text = "ANES";
		mainMenu.ResumeLayout(false);
		mainMenu.PerformLayout();
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private SdlControl sdlControl;
	private MyMenuStrip mainMenu;
	private ToolStripMenuItem fileToolStripMenuItem;
	private ToolStripMenuItem mainMenuFileOpen;
	private ToolStripSeparator toolStripMenuItem1;
	private ToolStripMenuItem mainMenuFileExit;
	private ToolStripMenuItem helpToolStripMenuItem;
	private ToolStripMenuItem mainMenuHelpAbout;
	private OpenFileDialog romOpenFIleDialog;
	private ToolStripMenuItem viewToolStripMenuItem;
	private ToolStripMenuItem mainMenuViewScale;
	private ToolStripMenuItem debugToolStripMenuItem;
	private ToolStripMenuItem mainMenuDebugPatternTableViewer;
}
