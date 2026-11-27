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
import org.verapdf.model.operator.Op_m_moveto;

import java.util.List;

/**
 * m starts a new subpath at the supplied x,y point (ISO 32000-1 section 8.5.2). point preserves the raw PDF coordinates.
 *
 * <p> The model type is {@code Op_m_moveto} implementing {@code Op_m_moveto}. Named links include {@code point}. The content-stream parser creates these objects and validation profiles query their model links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.PathConstruction.GFOp_m_moveto}, implementing {@code VeraPdf.Model.Operator.IOp_m_moveto}. Use {@code IReadOnlyList<IModelObject>} for links returned by {@code GetLinkedObjects(string)}, preserve raw operand order and nullable/missing values, and wrap COS operands with the corresponding {@code VeraPdf.Parser.Cos} types.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_m_moveto extends GFOpPathConstruction implements Op_m_moveto {

	/** Type name for {@code GFOp_m_moveto} */
    public static final String OP_M_MOVETO_TYPE = "Op_m_moveto";

	/** Name of link to the point */
    public static final String POINT = "point";

    public GFOp_m_moveto(List<COSBase> arguments) {
        super(arguments, OP_M_MOVETO_TYPE);
    }

    @Override
    public List<? extends Object> getLinkedObjects(String link) {
        if (POINT.equals(link)) {
            return this.getPoint();
        }
        return super.getLinkedObjects(link);
    }

    private List<CosNumber> getPoint() {
        return this.getListOfNumbers();
    }

}
