using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace UACHelperFunctions {
    static class UACHelper {
        public static bool IsAdmin() {
            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            WindowsPrincipal wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void LaunchWithAdminRights() {
            try {
                Process.Start(new ProcessStartInfo() {
                    Verb = "runas",
                    FileName = Application.ExecutablePath
                });
            }
            catch (Exception) {
                return;
            }
        }

        public static void RunApplicationAsAdmin(string path, string arguments, bool waitForExit = false) {
            try {
                Process process = Process.Start(new ProcessStartInfo() {
                                                         Verb = "runas",
                                                         FileName = path,
                                                         Arguments = arguments
                                                     });
                if (waitForExit) {
                    process.WaitForExit();
                }
            }
            catch (Exception) {
                return;
            }
        }
    }
}
