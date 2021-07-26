using System;
using VL.Lib.Collections;

namespace VL.OpenCV
{

    [Serializable]
    public class HAARCascadeFile : DynamicEnumBase<HAARCascadeFile, HAARCascadeFileDefinition>
    {
        public HAARCascadeFile(string value) : base(value)
        {
        }

        //this method needs to be imported in VL to set the default
        public static HAARCascadeFile CreateDefault()
        {
            return CreateDefaultBase("No HAAR cascade files found");
        }
    }
}