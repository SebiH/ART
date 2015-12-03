namespace Assets.Code.Console
{
    static class UConsoleFactory
    {

        // TODO: multiple consoles
        private static UConsole console;

        public static UConsole CreateConsole()
        {
            if (console == null)
            {
                console = new UConsole();
            }
            return console;
        }
    }
}
