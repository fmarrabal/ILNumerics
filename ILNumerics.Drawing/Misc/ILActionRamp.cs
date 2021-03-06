#region Copyright GPLv3
//
//  This file is part of ILNumerics.Net. 
//
//  ILNumerics.Net supports numeric application development for .NET 
//  Copyright (C) 2007, H. Kutschbach, http://ilnumerics.net 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
//  Non-free licenses are also available. Contact info@ilnumerics.net 
//  for details.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ILNumerics.BuiltInFunctions; 

namespace ILNumerics.Drawing.Misc {
    
    public class ILActionRamp : List<ILActionRampElement> {

        public static ILActionRamp s_OverrideRamp;
        public static ILActionRamp Override {
            get {
                if (s_OverrideRamp == null) {
                    s_OverrideRamp = new ILActionRamp();
                    ILArray<double> tmp = ILMath.sin(ILMath.linspace(-ILMath.pi / 1.7, ILMath.pi / 1.7, 20));
                    tmp -= tmp[0];
                    tmp /= tmp["end"];
                    foreach (double val in tmp.Values) {
                        s_OverrideRamp.Add(new ILActionRampElement((float)val, .02f));
                    }
                }
                return s_OverrideRamp;
            }
        }
        public static ILActionRamp s_softRamp;
        public static ILActionRamp Soft {
            get {
                if (s_softRamp == null) {
                    s_softRamp = new ILActionRamp();
                    ILArray<double> tmp = ILMath.sin(ILMath.linspace(-ILMath.pi / 2, ILMath.pi / 2, 10));
                    tmp += 1;
                    tmp /= 2;
                    foreach (double val in tmp.Values) {
                        s_softRamp.Add(new ILActionRampElement((float)val, .02f));
                    }
                }
                return s_softRamp;
            }
        }
        public static ILActionRamp s_hardRamp;
        public static ILActionRamp Hard {
            get {
                if (s_hardRamp == null) {
                    s_hardRamp = new ILActionRamp();
                    ILArray<double> tmp = ILMath.sin(ILMath.linspace(0, 1, 10));
                    foreach (double val in tmp.Values) {
                        s_hardRamp.Add(new ILActionRampElement((float)val, .02f));
                    }
                }
                return s_hardRamp;
            }
        }
        public static ILActionRamp s_linearRamp;
        public static ILActionRamp Linear {
            get {
                if (s_linearRamp == null) {
                    s_linearRamp = new ILActionRamp(); 
                    for (int i = 0; i < 10; i++) {
                        s_linearRamp.Add(new ILActionRampElement(i / 9.0f, 0.02f));                        
                    }
                }
                return s_linearRamp;
            }
        }
        public static ILActionRamp s_noRamp;
        public static ILActionRamp NoRamp {
            get {
                if (s_noRamp == null) {
                    s_noRamp = new ILActionRamp();
                    s_noRamp.Add(new ILActionRampElement(1, 0));
                }
                return s_noRamp;
            }
        }

        public ILActionRamp() {}
    }
}
