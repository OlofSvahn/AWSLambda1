using System;
using System.Collections.Generic;
using System.Text;

namespace AWSModels
{
    public class ScannedImage
    {
        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private string quality;
        public string Quality 
        {
            get { return quality; }
            set { quality = value; } 
        }

        private Dictionary<string, float> faceDetails = new Dictionary<string, float>();
        public Dictionary<string, float> FaceDetails
        {
            get { return faceDetails; }
            set { faceDetails = value; } 
        }

        private Dictionary<string, float> labels = new Dictionary<string, float>();
        public Dictionary<string,float> Labels
        {
            get { return labels; }
            set { labels = value; }
        }

        private Dictionary<string, float> moderationlabels = new Dictionary<string, float>();
        public Dictionary<string, float> ModerationLabels
        {
            get { return moderationlabels; }
            set { moderationlabels = value; }
        }

        private string notApprovedReason;
        public string NotApprovedReason
        {
            get { return notApprovedReason; }
            set { notApprovedReason = value; }
        }

        private bool isApproved;
        public bool IsApproved
        {
            get { return isApproved; }
            set { isApproved = value; }
        }
    }
}
