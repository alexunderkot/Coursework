using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    internal class GanttForm
    {
        static int scale = 3;
        static int labelWidth = 100;
        static string[] colors = {
    // Reds & Pinks
    "#FF0000", // red
    "#FF4500", // orange-red
    "#FF1493", // deep pink
    "#DC143C", // crimson
    "#FFB6C1", // light pink
    "#FF69B4", // hot pink
    "#C71585", // medium violet red
    
    // Oranges & Yellows
    "#FFA500", // orange
    "#FF8C00", // dark orange
    "#FFD700", // gold
    "#FFDAB9", // peach
    "#F0E68C", // khaki
    "#FFEFD5", // papaya whip
    "#FFE4B5", // moccasin
    
    // Greens
    "#008000", // green
    "#00FF00", // lime
    "#228B22", // forest green
    "#2E8B57", // sea green
    "#00CED1", // dark turquoise
    "#008080", // teal
    "#7CFC00", // lawngreen
    "#32CD32", // limegreen
    
    // Blues
    "#0000FF", // blue
    "#1E90FF", // dodger blue
    "#4169E1", // royal blue
    "#00BFFF", // deep sky blue
    "#4682B4", // steel blue
    "#87CEEB", // sky blue
    "#191970", // midnight blue
    "#000080", // navy
    
    // Purples & Violets
    "#800080", // purple
    "#8B008B", // dark magenta
    "#9400D3", // dark violet
    "#8A2BE2", // blue violet
    "#9370DB", // medium purple
    "#DA70D6", // orchid
    "#BA55D3", // medium orchid
    
    // Browns & Earth tones
    "#8B4513", // saddle brown
    "#A0522D", // sienna
    "#D2691E", // chocolate
    "#CD853F", // peru
    "#DEB887", // burlywood
    
    // Grays & Others
    "#708090", // slate gray
    "#B0C4DE", // light steel blue
    "#F5F5DC", // beige
    "#FFE4E1", // misty rose
    "#E6E6FA"  // lavender
};

        public static void GenerateGantt(Individual individual)
        {
            var sb = new StringBuilder();

            sb.Append("<!DOCTYPE html>\r\n<html>\r\n<head>\r\n<style>\r\n" +
                    "body { font-family: monospace; padding: 20px; }\r\n" +
                    ".legend { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 20px; }\r\n" +
                    ".legend-box { width: 16px; height: 16px;}\r\n" +
                    "</style>\r\n</head>\r\n<body>\r\n");

            sb.Append("<div class='legend'>\r\n");
            for (int j = 0; j < Data.NumJobs; j++)
            {
                sb.Append($"  <div>" +
                    $"<div class='legend-box' style='background:{colors[j]};'></div>" +
                    $"Work {individual.Order[j]}</div>\r\n");
            }
            sb.Append("</div>\r\n");

            for (int i = 0; i < Data.NumMachines; i++)
            {
                sb.Append($"    <!--Это станок {i}-->\r\n    ");
                sb.Append($"<div style=\"position: relative; height: 30px; margin-bottom: 4px;\">\r\n        " +
                    $"<span style=\"position: absolute; left: 0px; top: 10px;\">" +
                    $"Machine {i}</span>\r\n\r\n        ");
                for (int j = 0; j < Data.NumJobs; j++)
                {
                    sb.Append($"<!--Это работа {j}-->\r\n        ");
                    sb.Append($"<div style=\"position: absolute; left: {labelWidth + individual.StartTime[j][i] * scale}px; " +
                        $"top: 10px; width: {(individual.EndTime[j][i] - individual.StartTime[j][i]) * scale}px; height: 20px; " +
                        $"background-color: {colors[j]}; border: 1px solid; box-sizing: border-box;\">\r\n            ");
                    sb.Append($"</div>");
                }
                sb.Append("</div>");
            }
            sb.Append("</body>\r\n\r\n</html>");

            File.WriteAllText("gantt.html", sb.ToString());
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("gantt.html") { UseShellExecute = true });
        }
    }
}
