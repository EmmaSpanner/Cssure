using System.Runtime.Serialization;

namespace Cssure.Constants
{
    public static class Topics
    {
        public const string
            Topic_Result = "ECG/Result/#",
            Topic_Result_CSI = "ECG/Result/CSI",
            Topic_Result_ModCSI = "ECG/Result/ModCSI",
            Topic_Result_RR = "ECG/Result/RR-Peak",

            Topic_Status_CSSURE = "ECG/Status/CSSURE",
            Topic_Status_Python = "ECG/Status/Python",

            Topic_Series_FromBSSURE = "ECG/Series/BSSURE2CSSURE",
            Topic_Series_Raw = "ECG/Series/CSSURE2PYTHON",
            Topic_Series_Filtred = "ECG/Series/PYTHON2CSSURE",

            Topic_Series_TempToBSSURE = "ECG/Temp/ToBSSURE";
    }
}
