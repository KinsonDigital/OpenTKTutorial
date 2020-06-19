// <copyright file="Entity.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace OpenTKTutorial
{
    using System.Drawing;
    using OpenToolkit.Mathematics;

    /// <summary>
    /// A basic entity.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        /// <param name="textureID">The OpenGL ID of the texture.</param>
        public Entity(int textureID) => TextureID = textureID;

        /// <summary>
        /// Gets or sets The OpenGL ID of the texture.
        /// </summary>
        public int TextureID { get; set; }

        /// <summary>
        /// Gets or sets the position of the entity in the window.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the angle of the entity.
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        /// Gets or sets the size of the entity.
        /// </summary>
        public float Size { get; set; } = 1;

        /// <summary>
        /// Gets or sets the tint color of the entity.
        /// </summary>
        public Color TintColor { get; set; } = Color.White;
    }
}
