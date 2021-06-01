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
        public Medicines[] medicines;
        public Care[] care;
    }

    [Serializable]
    public class PatientStatus
    {
        public string status;
        public string info;
        public MedicalData patient;
    }

    [Serializable]
    public class Medicines
    {
        public int id;
        public string name;
    }

    [Serializable]
    public class Care
    {
        public int id;
        public string name;
    }

    [Serializable]
    public class ImageObject
    {
        public string base64;
    }
}
