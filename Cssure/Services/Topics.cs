using System.Runtime.Serialization;

namespace Cssure.Services
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
            Topic_Series_Raw = "ECG/Series/Raw",
            Topic_Series_Filtred = "ECG/Series/Filtred";
    }
}
