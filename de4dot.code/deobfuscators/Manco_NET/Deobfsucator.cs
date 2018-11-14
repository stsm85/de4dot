﻿/*
    Copyright (C) 2016 TheProxy

    This file is part of modified de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace de4dot.code.deobfuscators.Manco_NET
{
    public class DeobfuscatorInfo : DeobfuscatorInfoBase
    {
        public const string THE_NAME = "Manco.Net";
        public const string THE_TYPE = "mn";
        const string DEFAULT_REGEX = DeobfuscatorBase.DEFAULT_ASIAN_VALID_NAME_REGEX;

        public DeobfuscatorInfo()
            : base(DEFAULT_REGEX)
        {
        }

        public override string Name
        {
            get { return THE_NAME; }
        }

        public override string Type
        {
            get { return THE_TYPE; }
        }

        public override IDeobfuscator CreateDeobfuscator()
        {
            return new Deobfuscator(new Deobfuscator.Options
            {
                RenameResourcesInCode = false,
                ValidNameRegex = validNameRegex.Get(),
            });
        }

        class Deobfuscator : DeobfuscatorBase
        {
            AntiTamper antiTamper;
            StringDecryptor strDecryptor;
            Junk junkDetector;
            internal class Options : OptionsBase
            {
            }

            public override string Type
            {
                get { return DeobfuscatorInfo.THE_TYPE; }
            }

            public override string TypeLong
            {
                get { return DeobfuscatorInfo.THE_NAME; }
            }

            public override string Name
            {
                get { return TypeLong; }
            }

            public Deobfuscator(Options options)
                : base(options)
            {
            }

            protected override int DetectInternal()
            {
                int val = 0;
                val += val < 100 ? Convert.ToInt32(antiTamper.Detected) * 100 : Convert.ToInt32(antiTamper.Detected) * 10;
                val += val < 100 ? Convert.ToInt32(strDecryptor.Detected) * 100 : Convert.ToInt32(strDecryptor.Detected) * 10;
                if (junkDetector.Detected) val += 10;
                return val;
            }

            protected override void ScanForObfuscator()
            {
                antiTamper = new AntiTamper(module);
                antiTamper.Find();
                strDecryptor = new StringDecryptor(module,DeobfuscatedFile);
                strDecryptor.Find(DeobfuscatedFile);
                junkDetector = new Junk(module);
                junkDetector.Find();
            }

            public override void DeobfuscateBegin()
            {
                strDecryptor.Fix();

                if (antiTamper.Detected)
                    foreach (var method in antiTamper.Methods)
                        junkDetector.AdditionalMethods.Add(method);

                if (strDecryptor.Detected)
                    foreach (var field in strDecryptor.ProxyFields)
                        junkDetector.AdditionalFields.Add(field);

                base.DeobfuscateBegin();

            }

            public override void DeobfuscateEnd()
            {
                junkDetector.Fix();

                List<TypeDef> junkTypes = new List<TypeDef>();
                if (antiTamper.Detected)
                    junkTypes.Add(antiTamper.Type);
                if (strDecryptor.Detected)
                    foreach (TypeDef typ in strDecryptor.Types)
                        junkTypes.Add(typ);
                if (junkDetector.Detected)
                    junkTypes.Add(junkDetector.Type);
                AddTypesToBeRemoved(junkTypes, "Junk Classes");
                base.DeobfuscateEnd();
            }

            public override IEnumerable<int> GetStringDecrypterMethods()
            {
                var list = new List<int>();

                return list;
            }

        }

    }
}
