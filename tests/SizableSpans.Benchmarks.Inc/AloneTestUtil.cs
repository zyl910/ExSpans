using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.SizableSpans.Benchmarks {
    /// <summary>
    /// Alone test util .
    /// </summary>
    internal static class AloneTestUtil {

        /// <summary>
        /// Alone test by commnad.
        /// </summary>
        /// <param name="args">Command line args.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AloneTestByCommand(TextWriter writer, string[] args) {
            ParseCommand(args);
            //Debugger.Break();
            AloneTest(writer);
        }

        /// <summary>
        /// Parse command line args.
        /// </summary>
        /// <param name="args">Command line args.</param>
        public static void ParseCommand(string[] args) {
        }

        /// <summary>
        /// Alone test.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AloneTest(TextWriter writer) {
        }

    }
}
