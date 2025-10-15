using System.Windows.Forms;
using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

public partial class ReportForm : Form
{
    private Chart chart;

    public ReportForm()
    {
        this.Text = "Reports";
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;

        Label title = new Label()
        {
            Text = "📊 Reports Dashboard",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };
        this.Controls.Add(title);

        chart = new Chart()
        {
            Location = new Point(20, 70),
            Size = new Size(900, 400),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        ChartArea chartArea = new ChartArea("MainArea");
        chart.ChartAreas.Add(chartArea);

        Series serviceSeries = new Series("Services")
        {
            ChartType = SeriesChartType.Column,
            Color = Color.SteelBlue
        };

        serviceSeries.Points.AddXY("Jan", 25);
        serviceSeries.Points.AddXY("Feb", 18);
        serviceSeries.Points.AddXY("Mar", 32);
        serviceSeries.Points.AddXY("Apr", 45);
        serviceSeries.Points.AddXY("May", 50);
        serviceSeries.Points.AddXY("Jun", 38);

        chart.Series.Add(serviceSeries);
        chart.Titles.Add("Monthly Service Volume");

        this.Controls.Add(chart);
    }
}