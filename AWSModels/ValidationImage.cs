using System;
using System.Collections.Generic;
using System.Text;

namespace AWSModels
{
    public class ValidationImage
    {
        private string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private byte[] imageBytes;

        public byte[] ImageBytes
        {
            get { return imageBytes; }
            set { imageBytes = value; }
        }
    }

}
