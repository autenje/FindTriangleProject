using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FindTriangles.Models
{
    public class Triangle
    {
        public String Name { get; set; }
        public int PtAx { get; set; }
        public int PtBx { get; set; }
        public int PtCx { get; set; }
        public int PtAy { get; set; }
        public int PtBy { get; set; }
        public int PtCy { get; set; }
        public Double Area { get; set; }
        public String Row { get; set; }
        public int Col { get; set; }
    }
}