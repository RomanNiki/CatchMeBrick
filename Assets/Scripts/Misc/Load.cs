using System;

namespace Misc
{
    public class Load : IDisposable 
    {
        public Load(string text) {
            CanvasUtilities.Instance.Toggle(true, text);
        }

        public void Dispose() {
            CanvasUtilities.Instance.Toggle(false);
        }
    }
}