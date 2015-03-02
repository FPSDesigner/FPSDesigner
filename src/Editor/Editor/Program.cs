using System;

namespace Engine
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MainGameEngine game = new MainGameEngine())
            {
                game.Run();
            }
        }
    }
#endif
}

