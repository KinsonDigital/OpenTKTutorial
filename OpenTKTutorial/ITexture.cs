using System;

namespace OpenTKTutorial
{
    public interface ITexture : IDisposable
    {
        #region Props
        int ID { get; }

        string Name { get;  }

        int Width { get; }

        int Height { get; }
        #endregion


        #region Methods
        void Bind();


        void Unbind();
        #endregion
    }
}
