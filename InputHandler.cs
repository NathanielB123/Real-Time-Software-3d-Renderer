using _Application_Namespace;

namespace _3D_Renderer
{
    static class InputHandler
    {
#nullable enable
        //Due to protection levels, the GUI cannot directly store a reference to the app itself, so all inputs are sent to the GUI by sending
        //them to this input handler class first.
        public static App? MasterApp { get; set; }
        public static void KeyDown(string KeyValue)
        {
            if (MasterApp != null)
            {
                MasterApp.KeyDown(KeyValue);
            }
        }

        public static void KeyUp(int KeyValue)
        {
            if (MasterApp != null)
            {
                MasterApp.KeyUp(KeyValue);
            }
        }
    }
}
