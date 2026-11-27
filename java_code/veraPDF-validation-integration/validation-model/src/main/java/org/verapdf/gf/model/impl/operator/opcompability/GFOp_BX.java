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
package org.verapdf.gf.model.impl.operator.opcompability;

import org.verapdf.cos.COSBase;
import org.verapdf.model.operator.Op_BX;

import java.util.List;

/**
 * BX begins a compatibility section where unrecognized operators may be ignored (ISO 32000-1 section 7.3.8.4). It has no operands and is paired in stream order with EX.
 *
 * <p> The model type is {@code Op_BX} implementing {@code Op_BX}. The content-stream parser creates these objects and validation profiles query their model links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.OpCompatibility.GFOp_BX}, implementing {@code VeraPdf.Model.Operator.IOp_BX}. Use {@code IReadOnlyList<IModelObject>} for links returned by {@code GetLinkedObjects(string)}, preserve raw operand order and nullable/missing values, and wrap COS operands with the corresponding {@code VeraPdf.Parser.Cos} types.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_BX extends GFOpCompatibility implements Op_BX {

	/** Type name for {@code GFOp_BX} */
    public static final String OP_BX_TYPE = "Op_BX";

    public GFOp_BX(List<COSBase> arguments) {
        super(arguments, OP_BX_TYPE);
    }

}
