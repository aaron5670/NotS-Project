using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    [Serializable]
    public class MedicalData
    {
        public int id;
        public string name;
        public int leeftijd;
        public string geslacht;
        public string bloedgroep;
        public List<string> medicijnen;
        public List<string> allergiën;
    }
}
