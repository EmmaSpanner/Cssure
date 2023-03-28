using System.ComponentModel.DataAnnotations;

namespace Cssure.Models
{
    public class RawData
    {
        public RawData(byte[] rawData)
        {
            this.rawData = rawData;
        }

        [Required]
        public byte[] rawData { get; set; }
    }
}
