﻿using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Text;

namespace de4dot.code.deobfuscators.PhoneixProtector
{
    class StringDecrypter
    {
        ModuleDefMD module;
        //MethodDefAndDeclaringTypeDict<StringDecrypterInfo> stringDecrypterMethods = new MethodDefAndDeclaringTypeDict<StringDecrypterInfo>();
        TypeDef stringDecrypterType;
        MethodDef stringDecrtpterMethod;

        public TypeDef Type 
        {
            get { return stringDecrypterType; }
        }

            public MethodDef Method
        {
            get { return stringDecrtpterMethod; }
        }

        public bool Detected
        {
            get { return stringDecrtpterMethod != null; }
        }




        public void Find(ISimpleDeobfuscator simpleDeobfuscator)
        {
            foreach (var type in module.GetTypes())
            {
                foreach (var method in DotNetUtils.FindMethods(type.Methods, "System.String", new string[] { "System.String" }))
                {
                    if (!method.HasBody)
                        continue;
                    simpleDeobfuscator.Deobfuscate(method);

                    var instrs = method.Body.Instructions;
                    for (int i = 0; i < instrs.Count - 3; i++)
                    {
                        if (!instrs[i].IsLdarg() || instrs[i].GetParameterIndex() != 0)
                            continue;
                        if (instrs[i + 1].OpCode.Code != Code.Callvirt)
                            continue;
                        if (!instrs[i + 2].IsStloc())
                            continue;
                        if (!instrs[i + 3].IsLdloc())
                            continue;
                        if (instrs[i + 4].OpCode.Code != Code.Newarr)
                            continue;
                        if (!instrs[i + 5].IsStloc())
                            continue;
                        if (!instrs[i + 6].IsLdcI4())
                            continue;
                        if (!instrs[i + 7].IsStloc())
                            continue;
                        if (instrs[i + 8].OpCode.Code != Code.Br_S)
                            continue;
                        if (!instrs[i + 9].IsLdarg())
                            continue;
                        if (!instrs[i + 10].IsLdloc())
                            continue;
                        if (instrs[i + 11].OpCode.Code != Code.Callvirt)
                            continue;

                        stringDecrtpterMethod = method;
                        stringDecrypterType = method.DeclaringType;
                        return;
                    }
                }
            }
        }

        public StringDecrypter(ModuleDefMD module)
        {
            this.module = module;
        }

        public string Decrypt(string str)
        {
            var chrArr = new char[str.Length];
            var i = 0;
            foreach (char c in str)
                chrArr[i] = char.ConvertFromUtf32((((byte)((c >> 8) ^ i) << 8) | (byte)(c ^ (chrArr.Length - i++))))[0];
            return string.Intern(new string(chrArr));
        }
    }
}
