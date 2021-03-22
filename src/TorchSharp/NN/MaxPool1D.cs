// Copyright (c) Microsoft Corporation and contributors.  All Rights Reserved.  See License.txt in the project root for license information.
using System;
using System.Runtime.InteropServices;
using TorchSharp.Tensor;

namespace TorchSharp.NN
{
    /// <summary>
    /// This class is used to represent a MaxPool1D module.
    /// </summary>
    public class MaxPool1D : Module
    {
        internal MaxPool1D (IntPtr handle, IntPtr boxedHandle) : base (handle, boxedHandle)
        {
        }

        [DllImport ("LibTorchSharp")]
        private static extern IntPtr THSNN_MaxPool1d_forward (Module.HType module, IntPtr tensor);

        public TorchTensor forward (TorchTensor tensor)
        {
            var res = THSNN_MaxPool1d_forward (handle, tensor.Handle);
            if (res == IntPtr.Zero) { Torch.CheckForErrors(); }
            return new TorchTensor (res);
        }
    }
    public static partial class Modules
    {
        [DllImport ("LibTorchSharp")]
        extern static IntPtr THSNN_MaxPool1d_ctor (IntPtr pkernelSize, IntPtr pstrides, out IntPtr pBoxedModule);

        static public MaxPool1D MaxPool1D (long kernelSize, long? stride = null)
        {
            return stride.HasValue ?
                MaxPool1D(new long[] { kernelSize }, new long[] { stride.Value }) :
                MaxPool1D(new long[] { kernelSize }, null);
        }

        static private MaxPool1D MaxPool1D(long[] kernelSize, long[] strides = null)
        {
            unsafe {
                fixed (long* pkernelSize = kernelSize, pstrides = strides) {
                    var handle = THSNN_MaxPool1d_ctor((IntPtr)pkernelSize, (IntPtr)pstrides, out var boxedHandle);
                    if (handle == IntPtr.Zero) { Torch.CheckForErrors(); }
                    return new MaxPool1D(handle, boxedHandle);
                }
            }
        }

    }

    public static partial class Functions
    {
        static public TorchTensor MaxPool1D (TorchTensor x, long kernelSize, long? stride = null)
        {
            using (var d = Modules.MaxPool1D (kernelSize, stride)) {
                return d.forward (x);
            }
        }
    }
}
