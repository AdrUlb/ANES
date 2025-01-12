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
		mainMenuOpen = new ToolStripMenuItem();
		toolStripMenuItem1 = new ToolStripSeparator();
		mainMenuExit = new ToolStripMenuItem();
		viewToolStripMenuItem = new ToolStripMenuItem();
		mainMenuViewScale = new ToolStripMenuItem();
		helpToolStripMenuItem = new ToolStripMenuItem();
		mainMenuAbout = new ToolStripMenuItem();
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
		mainMenu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, helpToolStripMenuItem });
		mainMenu.Location = new Point(0, 0);
		mainMenu.Name = "mainMenu";
		mainMenu.Padding = new Padding(0);
		mainMenu.Size = new Size(320, 24);
		mainMenu.TabIndex = 1;
		mainMenu.Text = "menuStrip1";
		// 
		// fileToolStripMenuItem
		// 
		fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuOpen, toolStripMenuItem1, mainMenuExit });
		fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		fileToolStripMenuItem.Size = new Size(37, 24);
		fileToolStripMenuItem.Text = "&File";
		// 
		// mainMenuOpen
		// 
		mainMenuOpen.Name = "mainMenuOpen";
		mainMenuOpen.ShortcutKeys = Keys.Control | Keys.O;
		mainMenuOpen.Size = new Size(146, 22);
		mainMenuOpen.Text = "&Open";
		mainMenuOpen.Click += mainMenuOpen_Click;
		// 
		// toolStripMenuItem1
		// 
		toolStripMenuItem1.Name = "toolStripMenuItem1";
		toolStripMenuItem1.Size = new Size(143, 6);
		// 
		// mainMenuExit
		// 
		mainMenuExit.Name = "mainMenuExit";
		mainMenuExit.ShortcutKeys = Keys.Control | Keys.Q;
		mainMenuExit.Size = new Size(146, 22);
		mainMenuExit.Text = "E&xit";
		mainMenuExit.Click += mainMenuExit_Click;
		// 
		// viewToolStripMenuItem
		// 
		viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuViewScale });
		viewToolStripMenuItem.Name = "viewToolStripMenuItem";
		viewToolStripMenuItem.Size = new Size(44, 24);
		viewToolStripMenuItem.Text = "View";
		// 
		// mainMenuScale
		// 
		mainMenuViewScale.Name = "mainMenuScale";
		mainMenuViewScale.Size = new Size(180, 22);
		mainMenuViewScale.Text = "Scale";
		// 
		// helpToolStripMenuItem
		// 
		helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mainMenuAbout });
		helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		helpToolStripMenuItem.Size = new Size(44, 24);
		helpToolStripMenuItem.Text = "&Help";
		// 
		// mainMenuAbout
		// 
		mainMenuAbout.Name = "mainMenuAbout";
		mainMenuAbout.Size = new Size(107, 22);
		mainMenuAbout.Text = "&About";
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
	private ToolStripMenuItem mainMenuOpen;
	private ToolStripSeparator toolStripMenuItem1;
	private ToolStripMenuItem mainMenuExit;
	private ToolStripMenuItem helpToolStripMenuItem;
	private ToolStripMenuItem mainMenuAbout;
	private OpenFileDialog romOpenFIleDialog;
	private ToolStripMenuItem viewToolStripMenuItem;
	private ToolStripMenuItem mainMenuViewScale;
}
