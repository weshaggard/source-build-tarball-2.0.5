﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices.Implementation.Interop;
using Microsoft.VisualStudio.LanguageServices.Implementation.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.ExternalElements
{
    [ComVisible(true)]
    [ComDefaultInterface(typeof(EnvDTE.CodeFunction))]
    public sealed class ExternalCodeAccessorFunction : AbstractExternalCodeMember, EnvDTE.CodeFunction
    {
        internal static EnvDTE.CodeFunction Create(CodeModelState state, ProjectId projectId, IMethodSymbol symbol, AbstractExternalCodeMember parent)
        {
            var element = new ExternalCodeAccessorFunction(state, projectId, symbol, parent);
            return (EnvDTE.CodeFunction)ComAggregate.CreateAggregatedObject(element);
        }

        private readonly ParentHandle<AbstractExternalCodeMember> _parentHandle;

        private ExternalCodeAccessorFunction(CodeModelState state, ProjectId projectId, IMethodSymbol symbol, AbstractExternalCodeMember parent)
            : base(state, projectId, symbol)
        {
            Debug.Assert(symbol.MethodKind == MethodKind.EventAdd ||
                         symbol.MethodKind == MethodKind.EventRaise ||
                         symbol.MethodKind == MethodKind.EventRemove ||
                         symbol.MethodKind == MethodKind.PropertyGet ||
                         symbol.MethodKind == MethodKind.PropertySet);

            _parentHandle = new ParentHandle<AbstractExternalCodeMember>(parent);
        }

        private IMethodSymbol MethodSymbol
        {
            get { return (IMethodSymbol)LookupSymbol(); }
        }

        private bool IsPropertyAccessor()
        {
            var methodKind = MethodSymbol.MethodKind;
            return methodKind == MethodKind.PropertyGet
                || methodKind == MethodKind.PropertySet;
        }

        protected override EnvDTE.vsCMAccess GetAccess()
        {
            return _parentHandle.Value.Access;
        }

        protected override bool GetCanOverride()
        {
            return IsPropertyAccessor()
                ? ((ExternalCodeProperty)_parentHandle.Value).CanOverride
                : ((ExternalCodeEvent)_parentHandle.Value).CanOverride;
        }

        protected override string GetDocComment()
        {
            return string.Empty;
        }

        protected override string GetFullName()
        {
            return _parentHandle.Value.FullName;
        }

        protected override bool GetIsShared()
        {
            return _parentHandle.Value.IsShared;
        }

        protected override bool GetMustImplement()
        {
            return _parentHandle.Value.MustImplement;
        }

        protected override string GetName()
        {
            return _parentHandle.Value.Name;
        }

        protected override object GetParent()
        {
            return _parentHandle.Value;
        }

        public override EnvDTE.vsCMElement Kind
        {
            get { return EnvDTE.vsCMElement.vsCMElementFunction; }
        }

        public EnvDTE.vsCMFunction FunctionKind
        {
            get
            {
                switch (MethodSymbol.MethodKind)
                {
                    case MethodKind.PropertyGet:
                    case MethodKind.EventRemove:
                        return EnvDTE.vsCMFunction.vsCMFunctionPropertyGet;

                    case MethodKind.PropertySet:
                    case MethodKind.EventAdd:
                        return EnvDTE.vsCMFunction.vsCMFunctionPropertySet;

                    case MethodKind.EventRaise:
                        return EnvDTE.vsCMFunction.vsCMFunctionOther;

                    default:
                        throw Exceptions.ThrowEUnexpected();
                }
            }
        }

        public bool IsOverloaded
        {
            get { return false; }
        }

        public EnvDTE.CodeElements Overloads
        {
            get { throw Exceptions.ThrowEFail(); }
        }

        public EnvDTE.CodeTypeRef Type
        {
            get
            {
                return ((ExternalCodeProperty)_parentHandle.Value).Type;
            }

            set
            {
                throw Exceptions.ThrowEFail();
            }
        }
    }
}
