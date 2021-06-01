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
        public int age;
        public string gender;
        public string bloodType;
        public List<string> medicines;
        public List<string> care;
    }

    [Serializable]
    public class PatientStatus
    {
        public string status;
        public string info;
        public MedicalData patient;
    }

    [Serializable]
    public class ImageObject
    {
        public string base64;
    }
}
