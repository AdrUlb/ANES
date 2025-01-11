namespace ANES.Platform.Windows;

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
		menuStrip1 = new MenuStrip();
		fileToolStripMenuItem = new ToolStripMenuItem();
		openToolStripMenuItem = new ToolStripMenuItem();
		toolStripMenuItem1 = new ToolStripSeparator();
		exitToolStripMenuItem = new ToolStripMenuItem();
		helpToolStripMenuItem = new ToolStripMenuItem();
		aboutToolStripMenuItem = new ToolStripMenuItem();
		menuStrip1.SuspendLayout();
		SuspendLayout();
		// 
		// sdlControl
		// 
		sdlControl.Dock = DockStyle.Fill;
		sdlControl.Location = new Point(0, 24);
		sdlControl.Name = "sdlControl";
		sdlControl.Size = new Size(320, 216);
		sdlControl.TabIndex = 0;
		// 
		// menuStrip1
		// 
		menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, helpToolStripMenuItem });
		menuStrip1.Location = new Point(0, 0);
		menuStrip1.Name = "menuStrip1";
		menuStrip1.RenderMode = ToolStripRenderMode.System;
		menuStrip1.Size = new Size(320, 24);
		menuStrip1.TabIndex = 1;
		menuStrip1.Text = "menuStrip1";
		// 
		// fileToolStripMenuItem
		// 
		fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, toolStripMenuItem1, exitToolStripMenuItem });
		fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		fileToolStripMenuItem.Size = new Size(37, 20);
		fileToolStripMenuItem.Text = "File";
		// 
		// openToolStripMenuItem
		// 
		openToolStripMenuItem.Name = "openToolStripMenuItem";
		openToolStripMenuItem.Size = new Size(103, 22);
		openToolStripMenuItem.Text = "Open";
		// 
		// toolStripMenuItem1
		// 
		toolStripMenuItem1.Name = "toolStripMenuItem1";
		toolStripMenuItem1.Size = new Size(100, 6);
		// 
		// exitToolStripMenuItem
		// 
		exitToolStripMenuItem.Name = "exitToolStripMenuItem";
		exitToolStripMenuItem.Size = new Size(103, 22);
		exitToolStripMenuItem.Text = "Exit";
		// 
		// helpToolStripMenuItem
		// 
		helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
		helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		helpToolStripMenuItem.Size = new Size(44, 20);
		helpToolStripMenuItem.Text = "Help";
		// 
		// aboutToolStripMenuItem
		// 
		aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
		aboutToolStripMenuItem.Size = new Size(107, 22);
		aboutToolStripMenuItem.Text = "About";
		// 
		// MainWindow
		// 
		AutoScaleDimensions = new SizeF(7F, 15F);
		AutoScaleMode = AutoScaleMode.Font;
		ClientSize = new Size(320, 240);
		Controls.Add(sdlControl);
		Controls.Add(menuStrip1);
		Name = "MainWindow";
		Text = "ANES";
		menuStrip1.ResumeLayout(false);
		menuStrip1.PerformLayout();
		ResumeLayout(false);
		PerformLayout();
	}

	#endregion

	private SdlControl sdlControl;
	private MenuStrip menuStrip1;
	private ToolStripMenuItem fileToolStripMenuItem;
	private ToolStripMenuItem openToolStripMenuItem;
	private ToolStripSeparator toolStripMenuItem1;
	private ToolStripMenuItem exitToolStripMenuItem;
	private ToolStripMenuItem helpToolStripMenuItem;
	private ToolStripMenuItem aboutToolStripMenuItem;
}
