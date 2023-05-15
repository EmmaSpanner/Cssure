using System.Runtime.Serialization;

namespace Cssure.Constants
{
    public static class Topics
    {
        //private const string pre = "";
        private const string pre = "Dev/";
        public const string


            Topic_Status_CSSUREPY = pre + "ECG/Status/CSSURE-Py",
            Topic_Status_CSSURE = pre + "ECG/Status/CSSURE",
            Topic_Version_CSSURE = pre + "ECG/Version/CSSURE",

            Topic_GetVersion = pre + "ECG/GetVersion",


            Topic_Status_Python = pre + "ECG/Status/Python",
            Topic_Version_Python = pre + "ECG/Version/Python",

            Topic_Series_FromBSSURE = pre + "ECG/Series/BSSURE2CSSURE",
            Topic_Series_Raw = pre + "ECG/Series/CSSURE2PYTHON",
            Topic_Series_Filtred = pre + "ECG/Series/PYTHON2CSSURE",

            Topic_User = pre + "ECG/Userdata/#";
    }
}
