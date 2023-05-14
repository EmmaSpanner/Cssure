using System.Runtime.Serialization;

namespace Cssure.Constants
{
    public static class Topics
    {
        private const string pre = "Dev/";
        public const string
            Topic_Result = pre+"ECG/Result/#",
            Topic_Result_CSI = pre+"ECG/Result/CSI",
            Topic_Result_ModCSI = pre+"ECG/Result/ModCSI",
            Topic_Result_RR = pre + "ECG/Result/RR-Peak",

            Topic_Status_CSSURE = pre + "ECG/Status/CSSURE",
            Topic_Status_Python = pre + "ECG/Status/Python",

            Topic_Series_FromBSSURE = pre + "ECG/Series/BSSURE2CSSURE",
            Topic_Series_Raw = pre + "ECG/Series/CSSURE2PYTHON",
            Topic_Series_Filtred = pre + "ECG/Series/PYTHON2CSSURE",

            Topic_User= pre + "ECG/Userdata/#",

            Topic_Series_TempToBSSURE = pre + "ECG/Temp/ToBSSURE";
    }
}
