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
        /// This class is used to represent a ReLU6 module.
        /// </summary>
        public class ReLU6 : torch.nn.Module
        {
            internal ReLU6(IntPtr handle, IntPtr boxedHandle) : base(handle, boxedHandle) { }

            [DllImport("LibTorchSharp")]
            private static extern IntPtr THSNN_ReLU6_forward(torch.nn.Module.HType module, IntPtr tensor);

            public override Tensor forward(Tensor tensor)
            {
                var res = THSNN_ReLU6_forward(handle, tensor.Handle);
                if (res == IntPtr.Zero) { torch.CheckForErrors(); }
                return new Tensor(res);
            }

            public override string GetName()
            {
                return typeof(ReLU6).Name;
            }
        }
    }

    public static partial class torch
    {
        public static partial class nn
        {
            [DllImport("LibTorchSharp")]
            extern static IntPtr THSNN_ReLU6_ctor(bool inplace, out IntPtr pBoxedModule);

            /// <summary>
            /// Rectified Linear Unit
            ///
            /// This ReLU version caps positive values at 6.
            /// </summary>
            /// <param name="inPlace">Do the operation in-place. Default: False</param>
            /// <returns></returns>
            static public ReLU6 ReLU6(bool inPlace = false)
            {
                var handle = THSNN_ReLU6_ctor(inPlace, out var boxedHandle);
                if (handle == IntPtr.Zero) { torch.CheckForErrors(); }
                return new ReLU6(handle, boxedHandle);
            }

            public static partial class functional
            {
                /// <summary>
                /// Rectified Linear Unit
                ///
                /// This ReLU version caps positive values at 6.
                /// </summary>
                /// <param name="x">The input tensor</param>
                /// <param name="inPlace">Do the operation in-place. Default: False</param>
                /// <returns></returns>
                static public Tensor relu6(Tensor x, bool inPlace = false)
                {
                    using (var m = nn.ReLU6(inPlace)) {
                        return m.forward(x);
                    }
                }
            }
        }
    }
}
