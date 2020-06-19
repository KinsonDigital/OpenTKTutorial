// <copyright file="AtlasEntity.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    public class AtlasEntity : Entity
    {
        public AtlasEntity(int atlasTextureID, AtlasSubRect atlasSubRect)
            : base(atlasTextureID)
            => AtlasSubRect = atlasSubRect;

        public AtlasSubRect AtlasSubRect { get; private set; }
    }
}
