using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Extensions {
    static class controlExtensions {
        public static void DoubleBuffer(this Control control) {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession) {
                return;
            }
            System.Reflection.PropertyInfo dbProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
                                                                                                      System.Reflection.
                                                                                                          BindingFlags.
                                                                                                          NonPublic |
                                                                                                      System.Reflection.
                                                                                                          BindingFlags.
                                                                                                          Instance);
            dbProp.SetValue(control, true, null);
        }
    }
}
