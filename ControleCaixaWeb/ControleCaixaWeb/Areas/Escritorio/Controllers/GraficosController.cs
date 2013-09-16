using DotNet.Highcharts;
using DotNet.Highcharts.Options;
using System.Web.Mvc;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using Point = DotNet.Highcharts.Options.Point;
using System.Drawing;

namespace ControleCaixaWeb.Areas.Escritorio.Controllers
{
    public class GraficosController : Controller
    {
        //
        // GET: /Escritorio/Graficos/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ChartPizzaEstabelecimento()
        {
            Highcharts chart = new Highcharts("chart")
                 .InitChart(new Chart { PlotShadow = false })
                 .SetTitle(new Title { Text = "Relatório de Vendas Por Usuário." })
                 .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage +' %'; }" })
                 .SetPlotOptions(new PlotOptions
                 {
                     Pie = new PlotOptionsPie
                     {
                         AllowPointSelect = true,
                         Cursor = Cursors.Pointer,
                         DataLabels = new PlotOptionsPieDataLabels
                         {
                             Color = ColorTranslator.FromHtml("#000000"),
                             ConnectorColor = ColorTranslator.FromHtml("#000000"),
                             Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage +' %'; }"
                         }
                     }
                 })
                 .SetSeries(new Series
                 {
                     Type = ChartTypes.Pie,
                     Name = "Gráfico do Ademi",
                     Data = new Data(new object[]
                                               {
                                                   new object[] { "Firefox", 45.0 },
                                                   new object[] { "IE", 26.8 },
                                                   new Point
                                                   {
                                                       Name = "Chrome",
                                                       Y = 12.8,
                                                       Sliced = true,
                                                       Selected = true
                                                   },
                                                   new object[] { "Safari", 8.5 },
                                                   new object[] { "Opera", 6.2 },
                                                   new object[] { "Others", 0.7 }
                                               })
                 });

            return View(chart);
        }


    }
}
