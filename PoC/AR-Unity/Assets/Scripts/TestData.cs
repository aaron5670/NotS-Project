using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    [Serializable]
    public class TestData
    {
        public int id;
        public string first_name;
        public string phone;
        public string image;
    }

    [Serializable]
    public class TestUser
    {
        public TestData User;
        public TestData userData;
    }
}
