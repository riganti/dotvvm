namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{
	partial class DotvvmPropertyPageView
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
            this.chkPrecompileViews = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkPrecompileViews
            // 
            this.chkPrecompileViews.AutoSize = true;
            this.chkPrecompileViews.Checked = true;
            this.chkPrecompileViews.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrecompileViews.Location = new System.Drawing.Point(21, 24);
            this.chkPrecompileViews.Name = "chkPrecompileViews";
            this.chkPrecompileViews.Size = new System.Drawing.Size(152, 17);
            this.chkPrecompileViews.TabIndex = 2;
            this.chkPrecompileViews.Text = "Precompile DotVVM Views";
            // 
            // DotvvmPropertyPageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkPrecompileViews);
            this.Name = "DotvvmPropertyPageView";
            this.Size = new System.Drawing.Size(445, 232);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox chkPrecompileViews;
	}
}