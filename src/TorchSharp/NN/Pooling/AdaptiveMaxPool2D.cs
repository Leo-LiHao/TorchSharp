// Copyright (c) .NET Foundation and Contributors.  All Rights Reserved.  See LICENSE in the project root for license information.
using System;
using System.Runtime.InteropServices;
using static TorchSharp.torch;

namespace TorchSharp
{
    using Modules;

    namespace Modules
    {
        /// <summary>
        /// This class is used to represent a AdaptiveMaxPool2D module.
        /// </summary>
        public class AdaptiveMaxPool2d : torch.nn.Module
        {
            internal AdaptiveMaxPool2d(IntPtr handle, IntPtr boxedHandle) : base(handle, boxedHandle)
            {
            }

            [DllImport("LibTorchSharp")]
            private static extern IntPtr THSNN_AdaptiveMaxPool2d_forward(IntPtr module, IntPtr tensor);

            public override Tensor forward(Tensor tensor)
            {
                var res = THSNN_AdaptiveMaxPool2d_forward(handle.DangerousGetHandle(), tensor.Handle);
                if (res == IntPtr.Zero) { torch.CheckForErrors(); }
                return new Tensor(res);
            }
        }
    }

    public static partial class torch
    {
        public static partial class nn
        {
            [DllImport("LibTorchSharp")]
            extern static IntPtr THSNN_AdaptiveMaxPool2d_ctor(IntPtr psizes, int length, out IntPtr pBoxedModule);

            /// <summary>
            /// Applies a 2D adaptive max pooling over an input signal composed of several input planes.
            /// The output is of size H x W, for any input size.The number of output features is equal to the number of input planes.
            /// </summary>
            /// <param name="outputSize">Applies a 2D adaptive max pooling over an input signal composed of several input planes.
            /// The output is of size H x W, for any input size.The number of output features is equal to the number of input planes.</param>
            /// <returns></returns>
            static public AdaptiveMaxPool2d AdaptiveMaxPool2d(long[] outputSize)
            {
                unsafe {
                    fixed (long* pkernelSize = outputSize) {
                        var handle = THSNN_AdaptiveMaxPool2d_ctor((IntPtr)pkernelSize, outputSize.Length, out var boxedHandle);
                        if (handle == IntPtr.Zero) { torch.CheckForErrors(); }
                        return new AdaptiveMaxPool2d(handle, boxedHandle);
                    }
                }
            }

            public static partial class functional
            {
                /// <summary>
                /// Applies a 2D adaptive max pooling over an input signal composed of several input planes.
                /// The output is of size H x W, for any input size.The number of output features is equal to the number of input planes.
                /// </summary>
                /// <param name="x"></param>
                /// <param name="outputSize">Applies a 2D adaptive max pooling over an input signal composed of several input planes.
                /// The output is of size H x W, for any input size.The number of output features is equal to the number of input planes.</param>
                /// <returns></returns>
                static public Tensor adaptive_max_pool2d(Tensor x, long[] outputSize)
                {
                    using (var d = nn.AdaptiveMaxPool2d(outputSize)) {
                        return d.forward(x);
                    }
                }
            }
        }
    }
}
