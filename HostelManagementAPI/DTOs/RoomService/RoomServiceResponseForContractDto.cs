using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.RoomService
{
    public class RoomServiceResponseForContractDto
    {
        public int RoomId { get; set; }
        public int RoomServiceId { get; set; }
        public string TypeServiceName { get; set; }
        public string ServiceName { get; set; }
        public double ServicePrice { get; set; }
        public bool IsSelected { get; set; }
    }
}
