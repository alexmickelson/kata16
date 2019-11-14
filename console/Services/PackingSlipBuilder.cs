using console.Enums;
using console.Models;

namespace console.Services 
{

    public class PackingSlipBuilder //: IPackingSlipBuilder
    {
        public PackingSlipBuilder()
        {

        }

        public PackingSlip Generate()
        {
            return new PackingSlip();
        }

        public void SendPackingSlipTo(Departments destinationDepartment)
        {

        }
    }

}