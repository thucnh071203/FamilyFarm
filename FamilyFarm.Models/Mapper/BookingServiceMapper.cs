using FamilyFarm.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Models.Mapper
{
    public class BookingServiceMapper
    {
        public FriendMapper Account { get; set; }//account cua farmer or expert
        public Service Service { get; set; }
        public BookingService Booking { get; set; }
    }
}
