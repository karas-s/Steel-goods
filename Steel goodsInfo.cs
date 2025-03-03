using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Steel_goods
{
    public class Steel_goodsInfo : GH_AssemblyInfo
    {
        public override string Name => "Steel goods";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("f9ff04b6-5b03-4505-8064-77b62458d672");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}