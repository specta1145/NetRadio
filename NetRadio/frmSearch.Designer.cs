namespace NetRadio
{
    partial class frmSearch
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbSearch = new System.Windows.Forms.TextBox();
            this.lblSeach = new System.Windows.Forms.Label();
            this.linkLblRadioBrowserInfo = new System.Windows.Forms.LinkLabel();
            this.lblSearchButton = new System.Windows.Forms.Label();
            this.btnAcceptUnvisible = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbSearch
            // 
            this.tbSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbSearch.Location = new System.Drawing.Point(15, 32);
            this.tbSearch.Name = "tbSearch";
            this.tbSearch.Size = new System.Drawing.Size(149, 23);
            this.tbSearch.TabIndex = 0;
            // 
            // lblSeach
            // 
            this.lblSeach.AutoSize = true;
            this.lblSeach.Location = new System.Drawing.Point(12, 9);
            this.lblSeach.Name = "lblSeach";
            this.lblSeach.Size = new System.Drawing.Size(147, 13);
            this.lblSeach.TabIndex = 1;
            this.lblSeach.Text = "Search radio station by name:";
            // 
            // linkLblRadioBrowserInfo
            // 
            this.linkLblRadioBrowserInfo.Location = new System.Drawing.Point(40, 59);
            this.linkLblRadioBrowserInfo.Name = "linkLblRadioBrowserInfo";
            this.linkLblRadioBrowserInfo.Size = new System.Drawing.Size(120, 23);
            this.linkLblRadioBrowserInfo.TabIndex = 3;
            this.linkLblRadioBrowserInfo.TabStop = true;
            this.linkLblRadioBrowserInfo.Text = "www.radio-browser.info";
            this.linkLblRadioBrowserInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.linkLblRadioBrowserInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLblRadioBrowserInfo_LinkClicked);
            // 
            // lblSearchButton
            // 
            this.lblSearchButton.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSearchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblSearchButton.Image = global::NetRadio.Properties.Resources.epul;
            this.lblSearchButton.Location = new System.Drawing.Point(163, 32);
            this.lblSearchButton.Name = "lblSearchButton";
            this.lblSearchButton.Size = new System.Drawing.Size(30, 23);
            this.lblSearchButton.TabIndex = 4;
            this.lblSearchButton.Click += new System.EventHandler(this.lblSearchButton_Click);
            this.lblSearchButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblSearchButton_MouseDown);
            this.lblSearchButton.MouseEnter += new System.EventHandler(this.lblSearchButton_MouseEnter);
            this.lblSearchButton.MouseLeave += new System.EventHandler(this.lblSearchButton_MouseLeave);
            // 
            // btnAcceptUnvisible
            // 
            this.btnAcceptUnvisible.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAcceptUnvisible.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAcceptUnvisible.ForeColor = System.Drawing.SystemColors.Control;
            this.btnAcceptUnvisible.Location = new System.Drawing.Point(0, 0);
            this.btnAcceptUnvisible.Name = "btnAcceptUnvisible";
            this.btnAcceptUnvisible.Size = new System.Drawing.Size(1, 1);
            this.btnAcceptUnvisible.TabIndex = 5;
            this.btnAcceptUnvisible.UseVisualStyleBackColor = true;
            // 
            // frmSearch
            // 
            this.AcceptButton = this.btnAcceptUnvisible;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(207, 89);
            this.Controls.Add(this.btnAcceptUnvisible);
            this.Controls.Add(this.lblSearchButton);
            this.Controls.Add(this.linkLblRadioBrowserInfo);
            this.Controls.Add(this.lblSeach);
            this.Controls.Add(this.tbSearch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSearch";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NetRadio";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmSearch_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbSearch;
        private System.Windows.Forms.Label lblSeach;
        private System.Windows.Forms.LinkLabel linkLblRadioBrowserInfo;
        private System.Windows.Forms.Label lblSearchButton;
        private System.Windows.Forms.Button btnAcceptUnvisible;
    }
}