using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Zyl.ExSpans {
#nullable enable

    /// <summary>
    /// Environment output info.
    /// </summary>
    internal partial class EnvironmentOutput {

        /// <summary>
        /// Is release make.
        /// </summary>
        public static readonly bool IsRelease =
#if DEBUG
            false
#else
            true
#endif
        ;

        /// <summary>
        /// .NET Framework Version.
        /// </summary>
        public static readonly string DotNetFrameworkVersion =
#if NET45
            "4.5"
#elif NET461
            "4.6.1"
#else
            ""
#endif
        ;

        /// <summary>
        /// Output Environment.
        /// </summary>
        /// <param name="writer">Output <see cref="TextWriter"/>.</param>
        /// <param name="indent">The indent.</param>
        public static void OutputEnvironment(TextWriter writer, string? indent = null) {
            if (null == writer) return;
            if (null == indent) indent = "";
            //string indentNext = indent + "\t";

            writer.WriteLine(indent + string.Format("IsRelease:\t{0}", IsRelease));
            //writer.WriteLine(indent + string.Format("Environment.ProcessorCount:\t{0}", Environment.ProcessorCount));
            writer.WriteLine(indent + string.Format("IntPtr.Size:\t{0}", IntPtr.Size));
            //writer.WriteLine(indent + string.Format("Environment.Is64BitOperatingSystem:\t{0}", Environment.Is64BitOperatingSystem));
            writer.WriteLine(indent + string.Format("Environment.Is64BitProcess:\t{0}", Environment.Is64BitProcess));
            writer.WriteLine(indent + string.Format("Environment.OSVersion:\t{0}", Environment.OSVersion)); // Same RuntimeInformation.OSDescription. .Net framework does not recognize Windows 10 .
            writer.WriteLine(indent + string.Format("Environment.Version:\t{0}", Environment.Version));
            //writer.WriteLine(indent + string.Format("Stopwatch.Frequency:\t{0}", Stopwatch.Frequency));
#if NETCOREAPP3_0_OR_GREATER
            writer.WriteLine(indent + string.Format("RuntimeFeature.IsDynamicCodeCompiled:\t{0}", RuntimeFeature.IsDynamicCodeCompiled));
            writer.WriteLine(indent + string.Format("RuntimeFeature.IsDynamicCodeSupported:\t{0}", RuntimeFeature.IsDynamicCodeSupported));
#endif // NETCOREAPP3_0_OR_GREATER
            //writer.WriteLine(indent + string.Format("RuntimeEnvironment.GetSystemVersion:\t{0}", System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion())); // Same Environment.Version
            writer.WriteLine(indent + string.Format("RuntimeEnvironment.GetRuntimeDirectory:\t{0}", System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()));
#if (NET47 || NET462 || NET461 || NET46 || NET452 || NET451 || NET45 || NET40 || NET35 || NET20) || (NETSTANDARD1_0)
#else
            writer.WriteLine(indent + string.Format("RuntimeInformation.FrameworkDescription:\t{0}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription));
            writer.WriteLine(indent + string.Format("RuntimeInformation.OSArchitecture:\t{0}", System.Runtime.InteropServices.RuntimeInformation.OSArchitecture));
            writer.WriteLine(indent + string.Format("RuntimeInformation.OSDescription:\t{0}", System.Runtime.InteropServices.RuntimeInformation.OSDescription)); // Same Environment.OSVersion. It's more accurate.
            //writer.WriteLine(indent + string.Format("RuntimeInformation.ProcessArchitecture:\t{0}", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture)); Same OSArchitecture.
#endif
#if NET5_0_OR_GREATER
            writer.WriteLine(indent + string.Format("RuntimeInformation.RuntimeIdentifier:\t{0}", System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier)); // e.g. win10-x64
#endif // NET5_0_OR_GREATER
#if NETCOREAPP1_0_OR_GREATER || NETSTANDARD1_6_OR_GREATER || NET471_OR_GREATER
            // writer.WriteLine(indent + string.Format("RUNTIME_IDENTIFIER:\t{0}", AppContext.GetData("RUNTIME_IDENTIFIER"))); // Need NET5_0+
#endif // NETCOREAPP1_0_OR_GREATER || NETSTANDARD1_6_OR_GREATER || NET471_OR_GREATER
            writer.WriteLine(indent + string.Format("BitConverter.IsLittleEndian:\t{0}", BitConverter.IsLittleEndian));
            if (DotNetFrameworkVersion.Length>0) {
                writer.WriteLine(indent + string.Format(".NET Framework Version:\t{0}", DotNetFrameworkVersion));
            }

            //writer.WriteLine();
        }

        /// <summary>
        /// Get CodeBase.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Returns CodeBase.</returns>
#if NET5_0_OR_GREATER
        [UnconditionalSuppressMessage("SingleFile", "IL3002:Avoid calling members marked with 'RequiresAssemblyFilesAttribute' when publishing as a single-file", Justification = "Try catch")]
#endif // NET5_0_OR_GREATER
        private static string? GetCodeBase(Assembly assembly) {
#pragma warning disable SYSLIB0012 // Type or member is obsolete
            return assembly.CodeBase;
#pragma warning restore SYSLIB0012 // Type or member is obsolete
        }

    }

#nullable restore
}
