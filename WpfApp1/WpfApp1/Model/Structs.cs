using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Model
{
    public enum Mjesto { Slobodno, Zauzeto, Vod };

    public class MjestoMatrica
    {
        private Mjesto polje;
        private long pathId;

        public Mjesto Polje { get => polje; set => polje = value; }
        public long PathId { get => pathId; set => pathId = value; }
    }

    public struct ObjekatEES
    {
        public int x, y;
        public long id;
    };

    public struct Par
    {
        public int x2, y2, x1, y1;
        public long id2,id1,idVod;
        public string nameVod;
    };

    public interface IQItem {
        bool Valid { get; set; }
    };

    public struct QItem: IQItem
    {
        public int x, y, dist, xR, yR;
        public IQItem Parent;
        public bool VodPresek;
        public bool Valid { get; set; }
    };

    public struct Linija
    {
        public long firstEnd, secondEnd;
        public string name;
    }

}
