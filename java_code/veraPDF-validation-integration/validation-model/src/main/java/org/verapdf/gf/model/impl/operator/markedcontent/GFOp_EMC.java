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
package org.verapdf.gf.model.impl.operator.markedcontent;

import org.verapdf.cos.COSBase;
import org.verapdf.model.operator.Op_EMC;

import java.util.Collections;
import java.util.List;

/**
 * EMC ends the most recently opened marked-content sequence (ISO 32000-1 section 14.6.2). It has no operands, but its stream position is needed for nesting validation.
 *
 * <p> The model type is {@code Op_EMC} implementing {@code Op_EMC}. The parser creates these objects while walking the content stream; validation profiles query their typed links.</p>
 *
 * <p><b>.NET porting notes:</b> Suggested C# type: {@code VeraPdf.Validation.Gf.Model.Impl.Operator.MarkedContent.GFOp_EMC}, implementing {@code VeraPdf.Model.Operator.IOp_EMC}. Expose links through {@code IReadOnlyList<IModelObject> GetLinkedObjects(string)} and keep missing data as empty/null links. Map Java lists to read-only .NET collections and parser COS data to {@code VeraPdf.Parser.Cos.CosValue} types. Resource/parent-tree context should remain document-scoped rather than static.</p>
 *
 * @author Timur Kamalov
 */
public class GFOp_EMC extends GFOpMarkedContent implements Op_EMC {

	/** Type name for {@code GFOp_EMC} */
    public static final String OP_EMC_TYPE = "Op_EMC";

    public GFOp_EMC(List<COSBase> arguments) {
        super(arguments, OP_EMC_TYPE, null, Collections.emptyList(), false);
    }

}
