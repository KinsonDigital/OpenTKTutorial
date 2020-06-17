using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenTKTutorial
{
    public class AtlasEntity : Entity
    {
        public AtlasEntity(int atlasTextureID, AtlasSubRect atlasSubRect) : base(atlasTextureID)
        {
            AtlasSubRect = atlasSubRect;
        }

        public AtlasSubRect AtlasSubRect { get; private set; }
    }
}
