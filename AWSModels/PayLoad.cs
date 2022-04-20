using System;
using System.Collections.Generic;
using System.Text;

namespace AWSModels
{
    public class PayLoad
    {
        private List<ValidationImage> validationImages = new List<ValidationImage>();

        public List<ValidationImage> ValidationImages
        {
            get { return validationImages; }
            set { validationImages = value; }
        }
    }
}
