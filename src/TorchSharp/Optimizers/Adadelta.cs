// Copyright (c) .NET Foundation and Contributors.  All Rights Reserved.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static TorchSharp.torch;

namespace TorchSharp
{
    using Modules;

    public static partial class torch
    {
        public static partial class optim
        {

            /// <summary>
            /// Implements the Adadelta algorithm.
            ///
            /// It has been proposed in ADADELTA: An Adaptive Learning Rate Method.
            /// https://arxiv.org/abs/1212.5701
            /// </summary>
            /// <param name="parameters">Parameters to optimize. This optimizer requires the <b>named</b> parameters collection.</param>
            /// <param name="lr ">Learning rate</param>
            /// <param name="rho">Coefficient used for computing a running average of squared gradients (default: 0.9)</param>
            /// <param name="eps">Term added to the denominator to improve numerical stability, i.e. avoid division-by-zero (default: 1e-6)</param>
            /// <param name="weight_decay">Weight decay (L2 penalty) (default: 0)</param>
            /// <returns></returns>
            public static Adadelta Adadelta(IEnumerable<Parameter> parameters, double lr = 1.0, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
            {
                return new Adadelta(parameters, lr, rho, eps, weight_decay);
            }

            /// <summary>
            /// Implements the Adadelta algorithm.
            ///
            /// It has been proposed in ADADELTA: An Adaptive Learning Rate Method.
            /// https://arxiv.org/abs/1212.5701
            /// </summary>
            /// <param name="parameters">Parameters to optimize. This optimizer requires the <b>named</b> parameters collection.</param>
            /// <param name="lr ">Learning rate</param>
            /// <param name="rho">Coefficient used for computing a running average of squared gradients (default: 0.9)</param>
            /// <param name="eps">Term added to the denominator to improve numerical stability, i.e. avoid division-by-zero (default: 1e-6)</param>
            /// <param name="weight_decay">Weight decay (L2 penalty) (default: 0)</param>
            /// <returns></returns>
            public static Adadelta Adadelta(IEnumerable<(string name, Parameter parameter)> parameters, double lr = 1.0, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
            {
                return new Adadelta(parameters.Select(np => np.parameter), lr, rho, eps, weight_decay);
            }

            /// <summary>
            /// Implements the Adadelta algorithm.
            ///
            /// It has been proposed in ADADELTA: An Adaptive Learning Rate Method.
            /// https://arxiv.org/abs/1212.5701
            /// </summary>
            /// <param name="parameters">Parameters to optimize. This optimizer requires the <b>named</b> parameters collection.</param>
            /// <param name="lr ">Learning rate</param>
            /// <param name="rho">Coefficient used for computing a running average of squared gradients (default: 0.9)</param>
            /// <param name="eps">Term added to the denominator to improve numerical stability, i.e. avoid division-by-zero (default: 1e-6)</param>
            /// <param name="weight_decay">Weight decay (L2 penalty) (default: 0)</param>
            /// <returns></returns>
            public static Adadelta Adadelta(IEnumerable<Adadelta.ParamGroup> parameters, double lr = 1.0, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
            {
                return new Adadelta(parameters, lr, rho, eps, weight_decay);
            }
        }
    }

    namespace Modules
    {
        using static torch.optim;

        public class Adadelta : OptimizerHelper
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parameters">Parameters to optimize.</param>
            /// <param name="lr ">Learning rate</param>
            /// <param name="rho">Coefficient used for computing a running average of squared gradients (default: 0.9)</param>
            /// <param name="eps">Term added to the denominator to improve numerical stability, i.e. avoid division-by-zero (default: 1e-6)</param>
            /// <param name="weight_decay">Weight decay (L2 penalty) (default: 0)</param>
            public Adadelta(IEnumerable<Parameter> parameters, double lr, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
                : this(new ParamGroup[] { new ParamGroup { Parameters = parameters } }, lr, rho, eps, weight_decay)
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="parameters">Parameters to optimize.</param>
            /// <param name="lr ">Learning rate</param>
            /// <param name="rho">Coefficient used for computing a running average of squared gradients (default: 0.9)</param>
            /// <param name="eps">Term added to the denominator to improve numerical stability, i.e. avoid division-by-zero (default: 1e-6)</param>
            /// <param name="weight_decay">Weight decay (L2 penalty) (default: 0)</param>
            public Adadelta(IEnumerable<ParamGroup> parameters, double lr = 1.0, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
            {
                if (lr < 0.0) throw new ArgumentException($"Invalid learning rate: {lr}");
                if (rho < 0.0 || rho > 1.0) throw new ArgumentException($"Invalid rho value: {rho}");
                if (eps < 0.0) throw new ArgumentException($"Invalid eps value: {eps}");
                if (weight_decay < 0.0) throw new ArgumentException($"Invalid weight_decay value: {weight_decay}");

                var options = new Options {
                    LearningRate = lr,
                    InitialLearningRate = lr,
                    rho = rho,
                    eps = eps,
                    weight_decay = weight_decay
                };

                _defaults = options;
                _parameter_groups = new List<Modules.ParamGroup>();

                foreach (var g in parameters) {
                    add_param_group(g);
                }
            }

            /// <summary>
                /// Performs a single optimization step (parameter update).
                /// </summary>
                /// <param name="closure">A closure that reevaluates the model and returns the loss. Optional for most optimizers.</param>
                /// <returns></returns>
                public override Tensor step(Func<Tensor> closure = null)
            {
                return _step<ParamGroup>(group => {

                    var options = group.Options as Options;
                    var rho = options.rho.Value;
                    var eps = options.eps.Value;
                    var weight_decay = options.weight_decay.Value;
                    var lr = options.LearningRate.Value;

                    foreach (var param in group.Parameters) {

                        var grad = param.grad();

                        if (grad is null) continue;

                        if (grad.is_sparse) throw new ArgumentException("Adadelta does not support sparse gradients");

                        var state = _state[param.handle];

                        var square_avg = state.square_avg;
                        var acc_delta = state.acc_delta;

                        grad = (weight_decay != 0)
                            ? grad.add(param, alpha: weight_decay)
                            : grad.alias();

                        square_avg.mul_(rho).addcmul_(grad, grad, 1 - rho);

                        var std = square_avg.add(eps).sqrt_();
                        var delta = acc_delta.add(eps).sqrt_().div_(std).mul_(grad);

                        param.add_(delta, alpha: -lr);
                        acc_delta.mul_(rho).addcmul_(delta, delta, 1 - rho);
                    }
                }, closure);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                foreach (var kvp in _state) {
                    kvp.Value.Dispose();
                }
            }

            private class State : IDisposable
            {
                public long step;
                public Tensor square_avg;
                public Tensor acc_delta;

                public void Dispose()
                {
                    square_avg.Dispose();
                    acc_delta.Dispose();
                }
            }

            /// <summary>
            /// Add a param group to the Optimizer s param_groups.
            /// </summary>
            /// <param name="param_group"></param>
            /// <remarks>This can be useful when fine tuning a pre-trained network as frozen layers can be made trainable and added to the Optimizer as training progresses.</remarks>
            public override void add_param_group(Modules.ParamGroup param_group)
            {
                var def = _defaults as Options;
                if (param_group.Options is null) {
                    param_group.Options = new Options();
                }

                var opt = param_group.Options as Options;

                // Make sure all the options are set.
                if (!opt.LearningRate.HasValue) opt.LearningRate = def.LearningRate;
                if (!opt.rho.HasValue) opt.rho = def.rho;
                if (!opt.eps.HasValue) opt.eps = def.eps;
                if (!opt.weight_decay.HasValue) opt.weight_decay = def.weight_decay;

                opt.InitialLearningRate = opt.LearningRate.Value;

                _parameter_groups.Add(param_group);

                foreach (var p in param_group.Parameters) {
                    var state = new State();
                    _state[p.Handle] = state;
                    state.step = 0;
                    state.square_avg = torch.zeros_like(p);
                    state.acc_delta = torch.zeros_like(p);
                }
            }

            public class Options : OptimizerOptions
            {
                public double? rho;
                public double? eps;
                public double? weight_decay;
            }

            public class ParamGroup : ParamGroup<Options>
            {
                public ParamGroup() { }

                public ParamGroup(IEnumerable<Parameter> parameters, Options options) : base(parameters, options) { }

                public ParamGroup(IEnumerable<Parameter> parameters, double lr = 1.0, double rho = 0.9, double eps = 1e-6, double weight_decay = 0)
                    : base(parameters, new Adadelta.Options { LearningRate = lr, rho = rho, eps = eps, weight_decay = weight_decay })
                {
                }
            }

            private Dictionary<IntPtr, State> _state = new Dictionary<IntPtr, State>();
        }
    }
}
