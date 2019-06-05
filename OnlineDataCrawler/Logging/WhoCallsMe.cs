using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OnlineDataCrawler.Logging
{
    public static class WhoCalledMe
    {
        /// <summary>
        /// Gets the method name of the caller
        /// </summary>
        /// <param name="frame">The number of stack frames to retrace from the caller's position</param>
        /// <returns>The method name of the containing scope 'frame' stack frames above the caller</returns>
        [MethodImpl(MethodImplOptions.NoInlining)] // inlining messes this up pretty badly
        public static string GetMethodName(int frame = 1)
        {
            // we need to increment the frame to account for this method
            var methodBase = new StackFrame(frame + 1).GetMethod();
            var declaringType = methodBase.DeclaringType;
            return declaringType != null ? declaringType.Name + "." + methodBase.Name : methodBase.Name;
        }
    }
}
