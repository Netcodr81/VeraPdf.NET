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
package org.verapdf.gf.model.impl.operator.pathconstruction;

import org.verapdf.cos.COSBase;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosNumber;
import org.verapdf.model.operator.Op_c;

import java.util.List;

/**
 * c appends a cubic B?zier segment using six operands: two control points followed by the endpoint (ISO 32000-1 section 8.5.2). controlPoints contains all six numbers in source order.
 *
 * <p> The model type is {@code Op_c} implementing {@code Op_c}. The content-stream parser creates these objects and validation profiles query their model links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.PathConstruction.GFOp_c}, implementing {@code VeraPdf.Model.Operator.IOp_c}. Use {@code IReadOnlyList<IModelObject>} for links returned by {@code GetLinkedObjects(string)}, preserve raw operand order and nullable/missing values, and wrap COS operands with the corresponding {@code VeraPdf.Parser.Cos} types.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_c extends GFOpPathConstruction implements Op_c {

	/** Type name for {@code GFOp_c} */
    public static final String OP_C_TYPE = "Op_c";

    public GFOp_c(List<COSBase> arguments) {
        super(arguments, OP_C_TYPE);
    }

    @Override
    public List<? extends Object> getLinkedObjects(
            String link) {
        if (CONTROL_POINTS.equals(link)) {
            return this.getControlPoints();
        }
        return super.getLinkedObjects(link);
    }

    private List<CosNumber> getControlPoints() {
        return this.getListOfNumbers();
    }

}
