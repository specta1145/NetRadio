﻿using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NetRadio
{
    internal class ProgressBarEx : ProgressBar
    {
        public ProgressBarEx()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //LinearGradientBrush brush = null;
            SolidBrush brush = null;
            Rectangle rec = new Rectangle(0, 0, this.Width, this.Height);

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);

            rec.Width = (int)(rec.Width * ((double)base.Value / Maximum)) - 4;
            rec.Height -= 4;
            //            brush = new LinearGradientBrush(rec, this.ForeColor, this.BackColor, LinearGradientMode.Vertical);
            //            using (brush = new SolidBrush(this.ForeColor))
                        using (brush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
            }
        }
    }
}