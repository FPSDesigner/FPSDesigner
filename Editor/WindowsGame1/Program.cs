using System;

namespace ModelViewer
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ModelViewer game = new ModelViewer())
            {
                game.Run();
            }
        }
    }
#endif
}

