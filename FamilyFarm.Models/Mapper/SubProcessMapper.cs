using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;

namespace FamilyFarm.Models.Mapper
{
    public class SubProcessMapper
    {
        public FriendMapper Account { get; set; }//account cua farmer or expert
        public ExpertMapper Expert { get; set; }
        public Service Service { get; set; }
        public SubProcess SubProcess { get; set; }
        public PaymentTransaction Payment { get; set; }
    }
}
