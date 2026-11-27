/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.factory.functions;

import org.verapdf.gf.model.impl.pd.functions.*;
import org.verapdf.pd.function.*;

import java.util.logging.Logger;

/**
 * Creates validation-model wrappers for PDF function types 0, 2, 3, and 4.
 *
 * <p>An unknown or missing function type receives a generic wrapper, while supported types are mapped to typed interpolation,
 * exponential, stitching, or PostScript-calculator implementations.  ISO 32000 function dictionaries are used by color, shading,
 * and transparency validation.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Factory.Functions.FunctionFactory}.</li>
 *   <li>Map Java boxed {@code Long} to {@code long?} and dispatch on its nullable value.</li>
 *   <li>Map Java logger warnings to {@code ILogger.LogWarning}; implement typed models under
 *       {@code VeraPdf.Model.PDLayer} or {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Functions}.</li>
 * </ul>
 */
public class FunctionFactory {

    private static final Logger LOGGER = Logger.getLogger(FunctionFactory.class.getCanonicalName());

    private FunctionFactory() {
        //Disable default constructor
    }

    public static GFPDFunction createFunction(PDFunction function) {
        Long functionType = function.getFunctionType();

        if (functionType == null) {
            return new GFPDFunction(function);
        }

        switch (functionType.intValue()) {
            case 0:
                return new GFPDType0Function((PDType0Function) function);
            case 2:
                return new GFPDType2Function((PDType2Function) function);
            case 3:
                return new GFPDType3Function((PDType3Function) function);
            case 4:
                return new GFPDType4Function((PDType4Function) function);
            default:
                LOGGER.warning("Function type key must be {0, 2, 3, 4}");
                return new GFPDFunction(function);
        }
    }
}
